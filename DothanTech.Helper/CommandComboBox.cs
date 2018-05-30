/// <summary>
/// @file   CommandComboBox.cs
///	@brief  扩展 ComboBox 类，使得其具有 ICommandSource 特性。
/// @author	DothanTech 刘伟宏
/// 
/// Copyright(C) 2011~2014, DothanTech. All rights reserved.
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Markup;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Media.Imaging;

namespace Dothan.Helpers
{
    /// <summary>
    /// 扩展 ComboBox 类，使得其具有 ICommandSource 特性。
    /// </summary>
    public class CommandComboBox : ComboBox, ICommandSource
    {
        #region Execute Command

        public enum ExecuteReason
        {
            SelectionChanged = 0,   ///< OnSelectionChanged() 中触发

            CanLostFocus = 10,      ///< 此后的所有 Reason，都可以被剥夺焦点
            DropDownClosed,         ///< 下拉框被收起，可以认为用户输入完成了
            ReturnKeyDown,          ///< 用户按下确认键来确认选择
            AutoCommitTimer,        ///< 用户手动修改之后，超时定时器确认完成
        }

        public class ExecuteParameter
        {
            public ExecuteParameter(CommandComboBox source, object parameter, ExecuteReason reason)
            {
                this.Source = source;
                this.CommandParameter = parameter;
                this.Reason = reason;
            }

            public CommandComboBox Source { get; protected set; }
            public object CommandParameter { get; protected set; }
            public ExecuteReason Reason { get; protected set; }
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);

            ExecuteCommand(ExecuteReason.SelectionChanged);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (e.Key == Key.Return && Command != null)
            {
                e.Handled = true;
                //
                ExecuteCommand(ExecuteReason.ReturnKeyDown);
            }
        }

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            base.OnPreviewTextInput(e);

            StartAutoCommitTimer(2);
        }

        protected DispatcherTimer autoCommitTimer = null;
        protected void StartAutoCommitTimer(int seconds)
        {
            if (seconds <= 0)
            {
                if (autoCommitTimer != null)
                {
                    autoCommitTimer.Tick -= autoCommitTimer_Tick;
                    autoCommitTimer.Stop();
                    autoCommitTimer = null;
                }
            }
            else
            {
                if (autoCommitTimer == null)
                {
                    autoCommitTimer = new DispatcherTimer();
                    autoCommitTimer.Tick += autoCommitTimer_Tick;
                }
                else
                {
                    autoCommitTimer.Stop();
                }
                autoCommitTimer.Interval = new TimeSpan(0, 0, seconds);
                autoCommitTimer.Start();
            }
        }

        void autoCommitTimer_Tick(object sender, EventArgs e)
        {
            StartAutoCommitTimer(0);

            ExecuteCommand(ExecuteReason.AutoCommitTimer);
        }

        protected override void OnDropDownClosed(EventArgs e)
        {
            base.OnDropDownClosed(e);

            ExecuteCommand(ExecuteReason.DropDownClosed);
        }

        protected virtual bool ExecuteCommand(ExecuteReason reason)
        {
            // 简单检查当前状态
            if (!this.IsKeyboardFocusWithin)
                return false;
            if (Command == null)
                return false;

            // 已经结束的情况下，自动停止定时器
            if (reason >= ExecuteReason.CanLostFocus)
                StartAutoCommitTimer(0);

            // 执行命令
            Command.Execute(new ExecuteParameter(this, CommandParameter, reason));

            return true;
        }

        #endregion

        #region Command

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(CommandComboBox),
            new UIPropertyMetadata(null, new PropertyChangedCallback(OnCommandChanged)));

        [Category("Action")]
        [Localizability(LocalizationCategory.NeverLocalize)]
        [Bindable(true)]
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        protected static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CommandComboBox This = d as CommandComboBox;
            This.OnCommandChanged(e.OldValue as ICommand, e.NewValue as ICommand);
            CommandManager.InvalidateRequerySuggested();
        }

        /// <summary>
        /// 用于存放注册的回调函数，否则垃圾清理器会自动清理该函数注册，导致该注册不生效。
        /// </summary>
        private EventHandler canExecuteChangedHandler = null;
        private void OnCommandChanged(ICommand oldValue, ICommand newValue)
        {
            if (oldValue != null)
            {
                oldValue.CanExecuteChanged -= CanExecuteChanged;
            }
            if (newValue != null)
            {
                canExecuteChangedHandler = new EventHandler(CanExecuteChanged);
                newValue.CanExecuteChanged += canExecuteChangedHandler;
            }
        }

        private void CanExecuteChanged(object sender, EventArgs e)
        {
            if (this.Command != null)
            {
                RoutedCommand command = this.Command as RoutedCommand;

                // If a RoutedCommand.
                if (command != null)
                {
                    if (command.CanExecute(CommandParameter, CommandTarget))
                    {
                        this.IsEnabled = true;
                    }
                    else
                    {
                        this.IsEnabled = false;
                    }
                }
                // If a not RoutedCommand.
                else
                {
                    if (Command.CanExecute(CommandParameter))
                    {
                        this.IsEnabled = true;
                    }
                    else
                    {
                        this.IsEnabled = false;
                    }
                }
            }
        }

        #endregion

        #region CommandParameter

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof(object), typeof(CommandComboBox),
            new UIPropertyMetadata(null, new PropertyChangedCallback(OnCommandParameterChanged)));

        [Category("Action")]
        [Localizability(LocalizationCategory.NeverLocalize)]
        [Bindable(true)]
        public object CommandParameter
        {
            get { return (object)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        protected static void OnCommandParameterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion

        #region CommandTarget

        public static readonly DependencyProperty CommandTargetProperty =
            DependencyProperty.Register("CommandTarget", typeof(IInputElement), typeof(CommandComboBox),
            new UIPropertyMetadata(null, new PropertyChangedCallback(OnCommandTargetChanged)));

        [Category("Action")]
        [Localizability(LocalizationCategory.NeverLocalize)]
        [Bindable(true)]
        public IInputElement CommandTarget
        {
            get { return (IInputElement)GetValue(CommandTargetProperty); }
            set { SetValue(CommandTargetProperty, value); }
        }

        protected static void OnCommandTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion
    }
}

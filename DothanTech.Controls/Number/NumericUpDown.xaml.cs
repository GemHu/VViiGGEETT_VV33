/// <summary>
/// @file   NumericUpDown.cs
///	@brief  类似 SpinButton（带上下箭头滚动修改输入数值）的文本编辑框。
/// @author	DothanTech 吴桢楠
/// 
/// Copyright(C) 2011~2014, DothanTech. All rights reserved.
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;

namespace Dothan.Controls
{
    /// <summary>
    /// 类似 SpinButton（带上下箭头滚动修改输入数值）的文本编辑框。
    /// </summary>
    public partial class NumericUpDown : UserControl
    {
        /// <summary>
        /// 数值发生修改的原因。
        /// </summary>
        public enum ValueChangeReason
        {
            SetValue,           ///< 直接设置属性
            FormatChanged,      ///< 应该格式变化导致的数值修改
            ArrowUp,            ///< 由于按向上箭头导致的数值修改
            ArrowDown,          ///< 由于按向下箭头导致的数值修改
            Keyboard,           ///< 由于用户按键盘导致的数值修改
        }

        /// <summary>
        /// 数值修改前通知事件的参数。
        /// </summary>
        public class ValueChangingArgs : ValueChangedArgs
        {
            public ValueChangingArgs(ValueChangeReason reason, int oldValue, int newValue)
                : base(reason, oldValue, newValue)
            {
            }

            public bool rejectChange = false;
        }

        public delegate void ValueChangingHandler(object sender, ValueChangingArgs e);
        /// <summary>
        /// 数值发生修改前的通知事件。
        /// </summary>
        public event ValueChangingHandler OnValueChanging;

        /// <summary>
        /// 数值修改后通知事件的参数。
        /// </summary>
        public class ValueChangedArgs : EventArgs
        {
            public ValueChangedArgs(ValueChangeReason reason, int oldValue, int newValue)
            {
                this.Reason = reason;
                this.oldValue = oldValue;
                this.newValue = newValue;
            }

            public ValueChangeReason Reason;
            public int oldValue, newValue;
        }

        public delegate void ValueChangedHandler(object sender, ValueChangedArgs e);
        /// <summary>
        /// 数值发生修改后的通知事件。
        /// </summary>
        public event ValueChangedHandler OnValueChanged;

        #region Value

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(int), typeof(NumericUpDown),
            new UIPropertyMetadata((int)0, new PropertyChangedCallback(OnInterValueChanged)));

        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always), Category("Numeric")]
        [Description("当前值。")]
        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        protected static void OnInterValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as NumericUpDown).OnInterValueChanged(e);
        }
        protected void OnInterValueChanged(DependencyPropertyChangedEventArgs e)
        {
            if (!internalChange)
            {
                // 将数值设置到文本框
                SetShownText(ValueToString((int)e.NewValue));
            }

            // 发出通知事件
            if (OnValueChanged != null)
            {
                OnValueChanged.Invoke(this, new ValueChangedArgs(ValueChangeReason.SetValue, (int)e.OldValue, (int)e.NewValue));
            }
        }

        protected virtual string ValueToString(int value)
        {
            string str;
            if (this.DecimalDigits > 0)
            {
                str = Math.Abs(value).ToString("D" + this.DecimalDigits.ToString());
                if (str.Length <= this.DecimalDigits)
                    str = "0." + str;
                else
                    str = str.Insert(str.Length - this.DecimalDigits, ".");
            }
            else
            {
                str = Math.Abs(value).ToString();
            }
            if (value < 0) str = "-" + str;
            return str;
        }

        protected virtual bool StringToValue(string str, out int value)
        {
            if (this.DecimalDigits <= 0)
                return int.TryParse(str, out value);

            double dv;
            if (!double.TryParse(str, out dv))
            {
                value = 0;
                return false;
            }

            for (int i = this.DecimalDigits; i > 0; --i)
                dv *= 10.0;

            value = (int)(dv + 0.1);

            return true;
        }

        protected void SetShownText(string text)
        {
            if (this.txtNumeric.Text != text)
            {
                bool backup = internalChange;
                internalChange = true;
                this.txtNumeric.Text = text;
                internalChange = backup;
            }
        }

        protected bool internalChange = false;
        protected bool ChangeValue(int value, ValueChangeReason reason)
        {
            // 对设置值进行有效性检查
            if (value > this.MaxValue)
                value = this.MaxValue;
            if (value < this.MinValue)
                value = this.MinValue;

            // 数值没有变化？
            int oldValue = this.Value;
            if (value == oldValue)
            {
                // 只需要更新数据显示就可以了
                if (reason != ValueChangeReason.Keyboard)
                    SetShownText(ValueToString(value));
                return true;
            }

            // 发出通知，查看是否可以修改？
            if (OnValueChanging != null)
            {
                ValueChangingArgs e = new ValueChangingArgs(reason, oldValue, value);
                OnValueChanging.Invoke(this, e);
                if (e.rejectChange)
                    return false;

                // 可以通过 Changing 事件设置值
                value = e.newValue;
            }

            // 真正修改数据
            if (reason == ValueChangeReason.Keyboard && (!internalChange) &&
                StringToValue(this.txtNumeric.Text, out oldValue) && oldValue == value)
            {
                internalChange = true;
                this.Value = value;
                internalChange = false;
            }
            else
            {
                this.Value = value;
            }

            return true;
        }

        #endregion

        #region Change Step

        public static readonly DependencyProperty ChangeStepProperty =
            DependencyProperty.Register("ChangeStep", typeof(int), typeof(NumericUpDown),
            new UIPropertyMetadata((int)1, new PropertyChangedCallback(OnFormatChanged)));

        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always), Category("Numeric")]
        [Description("走一步的步长。")]
        public int ChangeStep
        {
            get { return (int)GetValue(ChangeStepProperty); }
            set { SetValue(ChangeStepProperty, value); }
        }

        #endregion

        #region Min Value

        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.Register("MinValue", typeof(int), typeof(NumericUpDown),
            new UIPropertyMetadata((int)0, new PropertyChangedCallback(OnFormatChanged)));

        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always), Category("Numeric")]
        [Description("最小值。")]
        public int MinValue
        {
            get { return (int)GetValue(MinValueProperty); }
            set { SetValue(MinValueProperty, value); }
        }

        #endregion

        #region Max Value

        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register("MaxValue", typeof(int), typeof(NumericUpDown),
            new UIPropertyMetadata((int)9999, new PropertyChangedCallback(OnFormatChanged)));

        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always), Category("Numeric")]
        [Description("最大值。")]
        public int MaxValue
        {
            get { return (int)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }

        #endregion

        #region Decimal Digits

        public static readonly DependencyProperty DecimalDigitsProperty =
            DependencyProperty.Register("DecimalDigits", typeof(int), typeof(NumericUpDown),
            new UIPropertyMetadata((int)0, new PropertyChangedCallback(OnFormatChanged)));

        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always), Category("Numeric")]
        [Description("小数位数。")]
        public int DecimalDigits
        {
            get { return (int)GetValue(DecimalDigitsProperty); }
            set { SetValue(DecimalDigitsProperty, value); }
        }

        #endregion

        protected static void OnFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NumericUpDown This = d as NumericUpDown;
            This.ChangeValue(This.Value, ValueChangeReason.FormatChanged);
        }

        public NumericUpDown()
        {
            InitializeComponent();
        }

        public new bool Focus()
        {
            return this.txtNumeric.Focus();
        }

        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            this.ChangeValue(this.Value + this.ChangeStep, ValueChangeReason.ArrowUp);

            this.txtNumeric.Focus();
        }

        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            this.ChangeValue(this.Value - this.ChangeStep, ValueChangeReason.ArrowDown);

            this.txtNumeric.Focus();
        }

        private void txtNumeric_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (internalChange) return;

            // 允许用户输入为空
            if (string.IsNullOrEmpty(this.txtNumeric.Text))
                return;

            int value;
            if (!StringToValue(this.txtNumeric.Text, out value) ||
                value > this.MaxValue || value < this.MinValue ||
                (!ChangeValue(value, ValueChangeReason.Keyboard))) // 提交修改
            {
                // 用户输入值无效，采用原来的值
                SetShownText(ValueToString(this.Value));
                this.txtNumeric.SelectAll();
            }
        }

        private void txtNumeric_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Space:
                    e.Handled = true;
                    break;
                case Key.Up:
                case Key.PageUp:
                case Key.VolumeUp:
                    btnUp_Click(this, null);
                    e.Handled = true;
                    break;
                case Key.Down:
                case Key.PageDown:
                case Key.VolumeDown:
                    btnDown_Click(this, null);
                    e.Handled = true;
                    break;
            }
        }
    }
}

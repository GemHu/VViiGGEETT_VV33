/// <summary>
/// @file   MessageViewer.cs
///	@brief  MessageBox 的显示界面。
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
using System.Windows.Shapes;
using System.ComponentModel;

namespace Dothan.Controls
{
    /// <summary>
    /// MessageBox 的显示界面。
    /// </summary>
    public partial class MessageViewer : Window, INotifyPropertyChanged
    {
        public MessageViewer()
        {
            InitializeComponent();
            this.DataContext = this;
            Btn1Visiblity = Visibility.Visible;
            Btn2Visiblity = Visibility.Collapsed;
            MessageTitle = "--提示信息--";
            MessageText = "";
            btn1Content = "确定";
            btn2Content = "取消";
            Height = 180;
        }

        private Visibility btn1Visiblity;
        /// <summary>
        /// Btn1显示与否
        /// </summary>
        public Visibility Btn1Visiblity
        {
            get { return btn1Visiblity; }
            set
            {
                btn1Visiblity = value;
                NotifyPropertyChanged("Btn1Visiblity");
            }
        }

        private Visibility btn2Visiblity;
        /// <summary>
        /// Btn2显示与否
        /// </summary>
        public Visibility Btn2Visiblity
        {
            get { return btn2Visiblity; }
            set
            {
                btn2Visiblity = value;
                NotifyPropertyChanged("Btn2Visiblity");
            }
        }

        private string messageTitle;
        /// <summary>
        /// 标题
        /// </summary>
        public string MessageTitle
        {
            get { return messageTitle; }
            set
            {
                messageTitle = value;
                NotifyPropertyChanged("MessageTitle");
            }
        }

        private string messageText;
        /// <summary>
        /// 消息内容
        /// </summary>
        public string MessageText
        {
            get { return messageText; }
            set
            {
                messageText = value;
                NotifyPropertyChanged("MessageText");
            }
        }

        private string btn1Content;
        /// <summary>
        /// 按钮1内容
        /// </summary>
        public string Btn1Content
        {
            get { return btn1Content; }
            set
            {
                btn1Content = value;
                NotifyPropertyChanged("Btn1Content");
            }
        }

        private string btn2Content;
        /// <summary>
        /// 按钮2内容
        /// </summary>
        public string Btn2Content
        {
            get { return btn2Content; }
            set
            {
                btn2Content = value;
                NotifyPropertyChanged("Btn2Content");
            }
        }

        /// <summary>
        /// 当按钮1点击时触发
        /// </summary>
        public Action Btn1Action { get; set; }
        /// <summary>
        /// 当按钮2点击时触发
        /// </summary>
        public Action Btn2Action { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// 属性改变时
        /// </summary>
        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 按钮1事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            if (Btn1Action != null)
                Btn1Action.Invoke();
            this.Close();
        }

        /// <summary>
        /// 按钮2事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            if (Btn2Action != null)
                Btn2Action.Invoke();
            this.Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch(e.Key)
            {
                case Key.Escape:
                case Key.Enter:
                    base.Close();
                    break;
            }
        }
    }
}

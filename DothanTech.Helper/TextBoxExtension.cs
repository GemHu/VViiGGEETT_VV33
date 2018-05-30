/// <summary>
/// @file   TextBoxExtension.cs
///	@brief  TextBox 类的扩展函数。
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
using System.Printing;
using System.Drawing.Printing;
using System.Runtime.InteropServices;

namespace Dothan.Helpers
{
    /// <summary>
    /// TextBox 的扩展类，得到输入焦点时，能自动选中全部的文本内容。
    /// </summary>
    public class MyTextBox : TextBox
    {
        public MyTextBox()
        {
            this.GotKeyboardFocus += MyTextBox_GotKeyboardFocus;
        }

        void MyTextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            this.SelectAll();
        }

        public Size FullSize()
        {
            Size sz = this.MeasureOverride(new Size(10000, 10000));
            if (sz.Width < 10) sz.Width = 10;
            if (sz.Height < 10) sz.Height = 5;
            return sz;
        }
    }

    /// <summary>
    /// TextBox 类的扩展函数。
    /// </summary>
    public static class TextBoxExtension
    {
        public static bool FocusSelectAll(this TextBox txt)
        {
            txt.SelectAll();
            return txt.Focus();
        }

        public static bool FocusToEnd(this TextBox txt)
        {
            if (!txt.Focus())
                return false;

            txt.SelectionLength = 0;
            txt.SelectionStart = txt.Text.Length;
            txt.ScrollToEnd();
            return true;
        }

        public static double GetDouble(this TextBox txt)
        {
            double value;
            if (double.TryParse(txt.Text, out value))
                return value;
            return 0.0;
        }
    }
}

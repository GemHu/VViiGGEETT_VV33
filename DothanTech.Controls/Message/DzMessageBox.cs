/// <summary>
/// @file   DzMessageBox.cs
///	@brief  公共的 MessageBox。
/// @author	DothanTech 吴桢楠
/// 
/// Copyright(C) 2011~2014, DothanTech. All rights reserved.
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dothan.Controls
{
    /// <summary>
    /// 公共的 MessageBox。
    /// </summary>
    public static class DzMessageBox
    {
        /// <summary>
        /// 静态的公共消息框
        /// </summary>
        private static MessageViewer msgBox { get; set; }

        /// <summary>
        /// 弹出 只需要信息内容
        /// </summary>
        /// <param name="message"></param>
        public static void Show(string message)
        {
            msgBox = new MessageViewer();
            msgBox.MessageText = message;
            msgBox.ShowDialog();
        }

        /// <summary>
        /// 弹出 需要信息内容和确定以后事件
        /// </summary>
        /// <param name="message"></param>
        public static void Show(string message, Action Btn1Clicked)
        {
            msgBox = new MessageViewer();
            msgBox.MessageText = message;
            msgBox.Btn1Action = Btn1Clicked;
            msgBox.ShowDialog();
        }

        public static void Show(string message, string title, double width)
        {
            msgBox = new MessageViewer();
            msgBox.Title = title;
            msgBox.MessageText = message;
            msgBox.Width = width;
            msgBox.ShowDialog();
        }

        /// <summary>
        /// 弹出 需要信息内容和标题
        /// </summary>
        /// <param name="message"></param>
        public static void Show(string message, string title)
        {
            msgBox = new MessageViewer();
            msgBox.Title = title;
            msgBox.MessageText = message;
            msgBox.ShowDialog();
        }

        /// <summary>
        /// 弹出 需要信息内容和确定以后事件
        /// </summary>
        /// <param name="message"></param>
        public static void Show(string message, Action Btn1Clicked, Action Btn2Clicked)
        {
            msgBox = new MessageViewer();
            msgBox.MessageText = message;
            msgBox.Btn2Visiblity = System.Windows.Visibility.Visible;
            msgBox.Btn1Content = "是";
            msgBox.Btn2Content = "否";
            msgBox.Btn1Action = Btn1Clicked;
            msgBox.Btn2Action = Btn2Clicked;
            msgBox.ShowDialog();
        }
    }
}

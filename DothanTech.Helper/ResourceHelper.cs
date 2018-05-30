/// <summary>
/// @file   ResourceHelper.cs
///	@brief  从资源文件中读取数据的辅助函数。
/// @author	DothanTech 刘伟宏
/// 
/// Copyright(C) 2011~2014, DothanTech. All rights reserved.
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Resources;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Dothan.Helpers
{
    public static class ResourceHelper
    {
        /// <summary>
        /// 加载指定文件名称的程序资源为 Cursor。
        /// </summary>
        /// <param name="fileName">资源文件名称</param>
        /// <param name="relative">Urikind是否为相对路径</param>
        /// <returns>Cursor</returns>
        public static Cursor LoadCursor(string fileName, bool relative = true)
        {
            try
            {
                StreamResourceInfo sri = Application.GetResourceStream(new Uri(fileName, relative ? UriKind.Relative : UriKind.Absolute));
                return new Cursor(sri.Stream);
            }
            catch (Exception ee)
            {
                Trace.WriteLine("### [" + ee.Source + "] Exception : " + ee.Message);
                Trace.WriteLine("### " + ee.StackTrace);
                return null;
            }
        }

        /// <summary>
        /// 加载指定文件名称的程序资源为 ImageSource。
        /// </summary>
        /// <param name="fileName">资源文件名称</param>
        /// <returns>ImageSource</returns>
        public static ImageSource LoadImage(string fileName)
        {
            try
            {
                StreamResourceInfo sri = Application.GetResourceStream(new Uri(fileName, UriKind.Relative));
                return BitmapFrame.Create(sri.Stream);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}

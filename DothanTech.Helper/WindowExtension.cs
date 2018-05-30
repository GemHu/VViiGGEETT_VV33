/// <summary>
/// @file   WindowExtension.cs
///	@brief  窗口（HWND）处理的一些扩展函数。
/// @author	DothanTech 刘伟宏
/// 
/// Copyright(C) 2011~2014, DothanTech. All rights reserved.
/// </summary>

using System;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Runtime.InteropServices;

namespace Dothan.Helpers
{
    /// <summary>
    /// HWND 的辅助处理函数。
    /// </summary>
    public static class HwndHelper
    {
        #region Foreground Window

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool BringWindowToTop(IntPtr hWnd);

        #endregion

        #region Find Window

        [DllImport("user32.dll", EntryPoint = "FindWindowW", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        public static IntPtr FindWindow(string lpWindowName)
        {
            return FindWindow((string)null, lpWindowName);
        }

        public static IntPtr FindWindowByClass(string lpClassName)
        {
            return FindWindow(lpClassName, (string)null);
        }

        #endregion

        #region Show Window

        public enum ShowWindowType
        {
            SW_HIDE = 0,
            SW_SHOWNORMAL = 1,
            SW_NORMAL = 1,
            SW_SHOWMINIMIZED = 2,
            SW_SHOWMAXIMIZED = 3,
            SW_MAXIMIZE = 3,
            SW_SHOWNOACTIVATE = 4,
            SW_SHOW = 5,
            SW_MINIMIZE = 6,
            SW_SHOWMINNOACTIVE = 7,
            SW_SHOWNA = 8,
            SW_RESTORE = 9,
            SW_SHOWDEFAULT = 10,
            SW_FORCEMINIMIZE = 11,
        }

        [DllImport("user32.dll")]
        public static extern int ShowWindow(IntPtr hWnd, ShowWindowType nCmdShow);

        #endregion

        #region Send Message

        public const int WM_USER = 0x0400;

        [DllImport("user32.dll", EntryPoint = "SendMessage")]
        public static extern long SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        public static long SendMessage(IntPtr hWnd, int Msg)
        {
            return SendMessage(hWnd, Msg, IntPtr.Zero, IntPtr.Zero);
        }

        public static long SendMessage(IntPtr hWnd, int Msg, uint wParam)
        {
            return SendMessage(hWnd, Msg, (IntPtr)wParam, IntPtr.Zero);
        }

        public static long SendMessage(IntPtr hWnd, int Msg, IntPtr wParam)
        {
            return SendMessage(hWnd, Msg, wParam, IntPtr.Zero);
        }

        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Ansi)]
        public static extern long SendMessageA(IntPtr hWnd, int Msg, IntPtr wParam, string lParam);

        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Unicode)]
        public static extern long SendMessageW(IntPtr hWnd, int Msg, IntPtr wParam, string lParam);

        #endregion

        #region StrLen

        [DllImport("kernel32.dll", EntryPoint = "lstrlenA")]
        public static extern int strlenA(IntPtr lpStr);

        [DllImport("kernel32.dll", EntryPoint = "lstrlenW")]
        public static extern int strlenW(IntPtr lpStr);

        #endregion

        #region WM_COPYDATA

        public const int WM_COPYDATA = 0x004A;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct CopyDataStruct
        {
            public uint dwData;
            public uint cbData;
            public IntPtr lpData;
        }

        public static long SendCopyDataA(IntPtr hWnd, uint dwData, string lpData)
        {
            if (lpData == null)
                return SendCopyData(hWnd, dwData, IntPtr.Zero, 0);

            IntPtr hData = Marshal.StringToHGlobalAnsi(lpData);
            try
            {
                return SendCopyData(hWnd, dwData, hData, (uint)(strlenA(hData) + 1));
            }
            finally
            {
                Marshal.FreeHGlobal(hData);
            }
        }

        public static long SendCopyDataW(IntPtr hWnd, uint dwData, string lpData)
        {
            if (lpData == null)
                return SendCopyData(hWnd, dwData, IntPtr.Zero, 0);

            IntPtr hData = Marshal.StringToHGlobalUni(lpData);
            try
            {
                return SendCopyData(hWnd, dwData, hData, (uint)(strlenW(hData) + 1) * 2);
            }
            finally
            {
                Marshal.FreeHGlobal(hData);
            }
        }

        public static long SendCopyData(IntPtr hWnd, uint dwData, IntPtr lpData, uint nLen)
        {
            CopyDataStruct data = new CopyDataStruct();
            data.dwData = dwData;
            data.cbData = nLen;
            data.lpData = (nLen <= 0 ? IntPtr.Zero : lpData);
            return SendMessage(hWnd, WM_COPYDATA, IntPtr.Zero, ref data);
        }

        [DllImport("user32.dll", EntryPoint = "SendMessage")]
        private static extern long SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, ref CopyDataStruct lParam);

        public static CopyDataStruct GetCopyData(IntPtr lParam)
        {
            return (CopyDataStruct)Marshal.PtrToStructure(lParam, typeof(CopyDataStruct));
        }

        public static uint GetCopyDataA(IntPtr lParam, out string data)
        {
            CopyDataStruct lpData = GetCopyData(lParam);
            if (lpData.lpData == IntPtr.Zero)
                data = null;
            else
                data = Marshal.PtrToStringAnsi(lpData.lpData);
            return lpData.dwData;
        }

        public static uint GetCopyDataW(IntPtr lParam, out string data)
        {
            CopyDataStruct lpData = GetCopyData(lParam);
            if (lpData.lpData == IntPtr.Zero)
                data = null;
            else
                data = Marshal.PtrToStringUni(lpData.lpData);
            return lpData.dwData;
        }

        #endregion

        #region Window Focus

        [DllImport("user32.dll")]
        public static extern IntPtr SetFocus(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetFocus();

        #endregion

        #region Window Style

        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool IsZoomed(IntPtr hWnd);

        #endregion

        #region Activate Window

        public static void ActivateWindow(IntPtr hWnd)
        {
            if (!IsWindowVisible(hWnd))
                ShowWindow(hWnd, ShowWindowType.SW_SHOWNORMAL);
            else if (IsZoomed(hWnd))
                ShowWindow(hWnd, ShowWindowType.SW_SHOWMAXIMIZED);
            else //if (IsIconic(hWnd))
                ShowWindow(hWnd, ShowWindowType.SW_SHOWNORMAL);

            SetForegroundWindow(hWnd);

            BringWindowToTop(hWnd);

            SetFocus(hWnd);
        }

        #endregion
    }

    /// <summary>
    /// Window 类的扩展函数。
    /// </summary>
    public static class WindowExtension
    {
        public static Window GetOwner(DependencyObject owner)
        {
            Window win = owner == null ? null : Window.GetWindow(owner);

            if (win == null) win = Application.Current.MainWindow;

            return win;
        }

        public static bool ShowDialog(this Window win, DependencyObject owner)
        {
            win.Owner = WindowExtension.GetOwner(owner);

            if (win.Owner != null && win.Icon == null && win.Owner.Icon != null)
                win.Icon = win.Owner.Icon;

            bool? result = win.ShowDialog();
            return result.HasValue && result.Value;
        }

        public static void EndDialog(this Window win, bool dialogResult)
        {
            win.DialogResult = dialogResult;
            win.Close();
        }

        public static IntPtr GetHandle(this Window window)
        {
            var helper = new WindowInteropHelper(window);
            return helper.Handle;
        }
    }

    /// <summary>
    /// CommonDialog 类的扩展函数。
    /// </summary>
    public static class CommonDialogExtension
    {
        public static bool ShowDialog(this CommonDialog dialog)
        {
            using (SpyCenterToActive cta = new SpyCenterToActive())
            {
                bool? result = dialog.ShowDialog(Application.Current.MainWindow);
                return result.HasValue && result.Value;
            }
        }

        public static bool ShowDialog(this CommonDialog dialog, DependencyObject owner)
        {
            using (SpyCenterToActive cta = new SpyCenterToActive())
            {
                Window win = WindowExtension.GetOwner(owner);

                bool? result = dialog.ShowDialog(win);
                return result.HasValue && result.Value;
            }
        }

        public static bool ShowCommonDialog(this CommonDialog dialog, DependencyObject owner)
        {
            return dialog.ShowDialog(owner);
        }
    }

    /// <summary>
    /// FrameworkElement 类的扩展函数。
    /// </summary>
    public static class FrameworkElementExtension
    {
        public static void MoveFocusNext(this FrameworkElement element)
        {
            element.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        public static void MoveFocusPrev(this FrameworkElement element)
        {
            element.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
        }

        public static void MoveFocus(this FrameworkElement element, FocusNavigationDirection dir)
        {
            element.MoveFocus(new TraversalRequest(dir));
        }
    }
}

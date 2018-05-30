using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Runtime.InteropServices;

namespace Dothan.Helpers
{
    public sealed class MyMessageBox
    {
        public static MessageBoxResult Show(DependencyObject owner, string messageBoxText)
        {
            return Show(owner, messageBoxText, null, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.None, MessageBoxOptions.None);
        }

        public static MessageBoxResult Show(DependencyObject owner, string messageBoxText, string caption)
        {
            return Show(owner, messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.None, MessageBoxOptions.None);
        }

        public static MessageBoxResult Show(DependencyObject owner, string messageBoxText, string caption, MessageBoxButton button)
        {
            return Show(owner, messageBoxText, caption, button, MessageBoxImage.Information, MessageBoxResult.None, MessageBoxOptions.None);
        }

        public static MessageBoxResult Show(DependencyObject owner, string messageBoxText, MessageBoxButton button, MessageBoxImage icon)
        {
            return Show(owner, messageBoxText, null, button, icon, MessageBoxResult.None, MessageBoxOptions.None);
        }

        public static MessageBoxResult Show(DependencyObject owner, string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            return Show(owner, messageBoxText, caption, button, icon, MessageBoxResult.None, MessageBoxOptions.None);
        }

        public static MessageBoxResult Show(DependencyObject owner, string messageBoxText, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult)
        {
            return Show(owner, messageBoxText, null, button, icon, defaultResult, MessageBoxOptions.None);
        }

        public static MessageBoxResult Show(DependencyObject owner, string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult)
        {
            return Show(owner, messageBoxText, caption, button, icon, defaultResult, MessageBoxOptions.None);
        }

        public static MessageBoxResult Show(DependencyObject owner, string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, MessageBoxOptions options)
        {
            using (SpyCenterToActive cta = new SpyCenterToActive())
            {
                Window win = WindowExtension.GetOwner(owner);
                //
                if (string.IsNullOrEmpty(caption))
                    caption = ApplicationTitle;

                return MessageBox.Show(win, messageBoxText, caption, button, icon, defaultResult, options);
            }
        }

        /// <summary>
        /// 缺省的消息框标题。
        /// </summary>
        public static string ApplicationTitle = string.Empty;
    }

    public enum BeepType
    {
        SimpleBeep = -1,
        IconAsterisk = 0x00000040,
        IconExclamation = 0x00000030,
        IconHand = 0x00000010,
        IconQuestion = 0x00000020,
        Ok = 0x00000000
    }

    public sealed class MyMessageBeep
    {
        [DllImport("user32.dll")]
        public static extern bool MessageBeep(BeepType beepType);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;

using Dothan.Helpers;

namespace Dothan.Controls
{
    public class DzDataGridComboBox:CommandComboBox
    {
        public DzDataGridComboBox()
        {
            this.BorderThickness = new Thickness(0);
            this.Background = new SolidColorBrush(Colors.Transparent);
            this.VerticalContentAlignment = VerticalAlignment.Center;
            this.IsEditable = true;
            this.IsSynchronizedWithCurrentItem = false;
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            this.BorderThickness = new Thickness(2);
            this.BorderBrush = new SolidColorBrush(Colors.Orange);
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnLostKeyboardFocus(e);
            this.BorderThickness = new Thickness(0);
            this.BorderBrush = new SolidColorBrush(Colors.Gray);
        }
    }
}

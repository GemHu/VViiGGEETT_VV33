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

namespace Dothan.Controls
{
    /// <summary>
    /// Interaction logic for CheckPassword.xaml
    /// </summary>
    public partial class CheckPassword : Window
    {
        public CheckPassword(Func<string, bool> checkFunc, Action okAction)
        {
            InitializeComponent();
            this.pwdFirst.Focus();
            
            this.CheckFunc = checkFunc;
            this.OkAction = okAction;
        }

        public string ShowName
        {
            set
            {
                 this.txtName.Text = value;
            }
        }

        protected Func<string, bool> CheckFunc;
        protected Action OkAction;

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (this.CheckFunc == null)
                return;

            if (string.IsNullOrEmpty(this.pwdFirst.Password))
                MessageBox.Show("请输入密码！", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            else if (!this.CheckFunc.Invoke(this.pwdFirst.Password))
                MessageBox.Show("密码输入错误，请重新输入！", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            else
            {
                if (this.OkAction != null)
                {
                    this.OkAction.Invoke();
                    this.Close();
                }
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

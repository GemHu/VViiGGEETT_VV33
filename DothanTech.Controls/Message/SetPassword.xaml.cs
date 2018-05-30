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
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class SetPassword : Window
    {
        #region Life cycle

        public SetPassword(Func<string, bool> checkFunc, Action<string> resultAction)
        {
            InitializeComponent();

            this.CheckFunc = checkFunc;
            this.ResultAction = resultAction;

            // 如果没有密码检查函数，则表示不需要显示输入原始密码
            if (this.CheckFunc == null)
            {
                this.pwdOriginal.Visibility = Visibility.Collapsed;
                this.rowOriginal.Height = new GridLength(0);
                pwdFirst.Focus();
            }
            this.pwdOriginal.Focus();
        }

        #endregion

        #region Data

        public string ShowName
        {
            set
            {
                this.txtName.Text = value;
            }
        }

        protected Func<string, bool> CheckFunc;

        protected Action<string> ResultAction;

        #endregion

        #region 事件处理函数

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (this.CheckFunc != null && string.IsNullOrEmpty(this.pwdOriginal.Password))
            {
                MessageBox.Show("请输入原始密码！", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (this.pwdFirst.Password != this.pwdSecond.Password)
            {
                MessageBox.Show("两次输入的密码不一致，请重新输入！", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (this.CheckFunc != null && !this.CheckFunc.Invoke(this.pwdOriginal.Password))
            {
                MessageBox.Show("密码输入错误，请重新输入！", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (this.ResultAction != null)
                this.ResultAction.Invoke(this.pwdFirst.Password);

            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ViGET.HWCEditor
{
    /// <summary>
    /// Interaction logic for UcHWCEditor.xaml
    /// </summary>
    public partial class UcHWCEditor : UserControl
    {
        public UcHWCEditor()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        #region FileName Property

        public static readonly DependencyProperty FileNameProperty =
            DependencyProperty.Register("FileName", typeof(String), typeof(UcHWCEditor),
                                        new PropertyMetadata(String.Empty));

        public String FileName
        {
            get { return (String)GetValue(FileNameProperty); }
            set { SetValue(FileNameProperty, value); }
        }

        #endregion

    }
}

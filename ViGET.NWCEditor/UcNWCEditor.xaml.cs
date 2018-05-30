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

namespace ViGET.NWCEditor
{
    /// <summary>
    /// Interaction logic for UcNWCEditor.xaml
    /// </summary>
    public partial class UcNWCEditor : UserControl
    {
        public UcNWCEditor()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        #region FileName Property

        public static readonly DependencyProperty FileNameProperty =
            DependencyProperty.Register("FileName", typeof(String), typeof(UcNWCEditor),
                                        new PropertyMetadata(String.Empty));

        public String FileName
        {
            get { return (String)GetValue(FileNameProperty); }
            set { SetValue(FileNameProperty, value); }
        }

        #endregion

    }
}

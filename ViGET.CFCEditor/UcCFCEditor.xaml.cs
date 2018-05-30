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

namespace DothanTech.ViGET.CFCEditor
{
    /// <summary>
    /// Interaction logic for UcCFCEditor.xaml
    /// </summary>
    public partial class UcCFCEditor : UserControl
    {
        public UcCFCEditor()
        {
            InitializeComponent();
        }

        public CFCEditor EditorModule 
        {
            get { return this._editorModule; }
            set
            {
                this._editorModule = value;
                this.DataContext = value;
            }
        }
        private CFCEditor _editorModule;
    }
}

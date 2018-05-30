using Dothan.Manager;
using DothanTech.ViGET.Manager;
using DothanTech.ViGET.SolutionExplorer.Model;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DothanTech.ViGET.SolutionExplorer.Views
{
    /// <summary>
    /// Interaction logic for BatchBuildProject.xaml
    /// </summary>
    public partial class BatchBuildProject : Window
    {
        public BatchBuildProject(ProjectManager proj)
        {
            InitializeComponent();

            this.DataContext = new BatchProjectViewModel(proj);
            //get Title info
            this.Title = this.Title + (this.DataContext as BatchProjectViewModel).Tittle;
            //close command
            (this.DataContext as BatchProjectViewModel).CloseCommand = new BatchProjectDelegateCommand(new Action(this.Close));
        }
    }
}

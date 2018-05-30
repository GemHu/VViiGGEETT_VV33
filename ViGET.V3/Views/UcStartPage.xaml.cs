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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.ComponentModel;
using Dothan.ViObject;
using Dothan.Helpers;
using Dothan.Manager;
using System.IO;
using Prism.Mvvm;
using Prism.Commands;
using DothanTech.ViGET.ViService;
using System.ComponentModel.Composition;

namespace DothanTech.ViGET.Shell
{
    /// <summary>
    /// Interaction logic for StartPageWPFControl.xaml
    /// </summary>
    [Export]
    public partial class UcStartPage : UserControl, IPartImportsSatisfiedNotification
    {
        /// <summary>
        /// Recent Project Item
        /// </summary>
        public class ProjectItem : BindableBase
        {
            public ProjectItem(String fileName)
            {
                this.Name = System.IO.Path.GetFileNameWithoutExtension(fileName);
                this.FileName = fileName;
            }

            public String Name
            {
                get { return _name; }
                set
                {
                    _name = value;
                    this.RaisePropertyChanged("Name");
                }
            }
            private String _name;

            public String FileName
            {
                get { return _fileName; }
                set
                {
                    _fileName = value;
                    this.RaisePropertyChanged("FileName");
                }
            }
            private String _fileName;

        }

        [Import(typeof(IViShellService))]
        private IViShellService ShellService;

        public UcStartPage()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        #region RecentProjects Property

        public ObservableCollection<ProjectItem> RecentProjects { get; set; }

        #endregion
        
        private void UpdateRecentProjects()
        {
            if (this.ShellService == null)
                return;

            List<String> dataList = this.ShellService.GetRecentProjects();
            if (this.RecentProjects == null)
                this.RecentProjects = new ObservableCollection<ProjectItem>();
            else
                this.RecentProjects.Clear();

            foreach (var item in dataList)
            {
                this.RecentProjects.Add(new ProjectItem(item));
            }
        }

        #region Command Operate

        public DelegateCommand<Object> NewProjectCmd
        {
            get
            {
                if (this._newProjectCmd == null)
                {
                    this._newProjectCmd = new DelegateCommand<Object>((p) =>
                    {
                        if (this.ShellService != null)
                        {
                            this.ShellService.NewProject();
                            this.UpdateRecentProjects();
                        }
                    });
                }
                return _newProjectCmd;
            }
        }
        private DelegateCommand<Object> _newProjectCmd;

        public DelegateCommand OpenProjectCmd
        {
            get
            {
                if (this._openProjectCmd == null)
                {
                    this._openProjectCmd = new DelegateCommand(() =>
                    {
                        if (this.ShellService != null)
                        {
                            this.ShellService.OpenProject(null);
                            this.UpdateRecentProjects();
                        }
                    });
                }
                return _openProjectCmd;
            }
        }
        private DelegateCommand _openProjectCmd;

        public DelegateCommand<Object> OpenRecentProjectCmd
        {
            get
            {
                if (this._openRecentProjectCmd == null)
                {
                    this._openRecentProjectCmd = new DelegateCommand<Object>((p) =>
                    {
                        
                    });
                }
                return _openRecentProjectCmd;
            }
        }
        private DelegateCommand<Object> _openRecentProjectCmd;

        #endregion

        private void listRecentProject_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point pos = e.GetPosition(listRecentProject);
            HitTestResult result = VisualTreeHelper.HitTest(listRecentProject, pos);
            ListBoxItem item = null;
            if (result != null)
                item = ViewHelper.FindVisualParent<ListBoxItem>(result.VisualHit);
            ProjectItem target = item == null ? null : item.DataContext as ProjectItem;
            if (target != null && this.ShellService != null)
            {
                this.ShellService.OpenProject(target.FileName);
                this.UpdateRecentProjects();
            }
        }

        public void OnImportsSatisfied()
        {
            this.UpdateRecentProjects();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Media;
using Dothan.Helpers;
using System.Windows.Forms;
using Dothan.Manager;
using DothanTech.ViGET.Manager;

namespace DothanTech.ViGET.SolutionExplorer.Model
{
    public class BatchProjectViewModel : BatchProjectNotificationObject
    {
        #region set treeviewitem Icon
        protected static ImageSource ParentNodeImg, ChildNodeImg;
        static BatchProjectViewModel()
        {
            ParentNodeImg = ResourceHelper.LoadImage(@"ViProjectManager;component\Views\Images\Project.ico");
            ChildNodeImg = ResourceHelper.LoadImage(@"ViProjectManager;component\Views\Images\CPU.ico");
        }
        #endregion

        ProjectManager projManager { get; set; }

        public BatchProjectViewModel(ProjectManager proj)
        {
            projManager = proj;
            _collectionItems = this.ShowBatchBuildView();

            //Command
            this.BuildCommand = new BatchProjectDelegateCommand(this.OnExecuteBuildMethod);
            this.ReBuildCommand = new BatchProjectDelegateCommand(this.OnExecuteReBuildMethod);
            this.SelectAllCommand = new BatchProjectDelegateCommand(this.OnExecuteSelectAllCommand);
            this.DeSelectAllCommand = new BatchProjectDelegateCommand(this.OnExecuteDeSelectAllCommand);
        }

        /// <summary>
        /// 窗体标题名称
        /// </summary>
        public string Tittle
        {
            get;
            set;
        }

        /// <summary>
        /// 管理Project和CPU的节点数据
        /// </summary>
        private ObservableCollection<ProjectModel> _collectionItems;
        public ObservableCollection<ProjectModel> CollectionItems
        {
            get
            {
                return _collectionItems;
            }
            set
            {
                _collectionItems = value;
                this.RaisePropertyChanged("CollectionItems");
            }
        }

        /// <summary>
        /// 获取视图相关用到的数据
        /// </summary>
        private ObservableCollection<ProjectModel> ShowBatchBuildView()
        {
            ObservableCollection<ProjectModel> modelCollection = new ObservableCollection<ProjectModel>();

            if (projManager == null) return null;

            Tittle = projManager.ProjectName;

            //得到父节点
            ProjectModel ParentModel = new ProjectModel()
            {
                IsSelected = true,
                NodeIcon = ParentNodeImg,
                NodeDisplayName = projManager.ProjectName
            };

            //得到子节点，并添加到父节点
            foreach (ViCPUInfo cpu in projManager.CPUs)
            {
                ProjectModel ChildMode = new ProjectModel()
                {
                    IsSelected = true,
                    NodeIcon = ChildNodeImg,
                    NodeDisplayName = cpu.Key,
                };

                ChildMode.Parent = ParentModel;
                ParentModel.ChildCPU.Add(ChildMode);
            }

            modelCollection.Add(ParentModel);
            return modelCollection;
        }


        #region Command
        /// <summary>
        /// CloseCommand
        /// </summary>
        public BatchProjectDelegateCommand CloseCommand { get; set; }

        #region Build Command
        public BatchProjectDelegateCommand BuildCommand { get; set; }
        public BatchProjectDelegateCommand ReBuildCommand { get; set; }
        public void OnExecuteBuildMethod()
        {
            ReBuildCPUs(false);
        }

        public void OnExecuteReBuildMethod()
        {
            ReBuildCPUs(true);
        }

        private void ReBuildCPUs(bool isReBuild)
        {
            foreach (ProjectModel model in CollectionItems)
            {
                if (model.ChildCPU == null) return;

                foreach (ProjectModel cpuModel in model.ChildCPU)
                {
                    if (cpuModel.Parent != null && (bool?)cpuModel.Parent.IsSelected == false)
                    {
                        MessageBox.Show("Not choose CPU!", "BatchBuild", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (cpuModel != null && (bool?)cpuModel.IsSelected == true)
                    {
                        //CPUInfoManager cpu = projManager.GetCPUInfo(cpuModel.NodeDisplayName);
                        //SolutionManager.theSolutionManager.Build(cpu, isReBuild);
                    }
                }
            }
        }
        #endregion

        #region Select Command
        public BatchProjectDelegateCommand SelectAllCommand { get; set; }
        public BatchProjectDelegateCommand DeSelectAllCommand { get; set; }
        public void OnExecuteSelectAllCommand()
        {
            SelectCPUs(true);
        }

        public void OnExecuteDeSelectAllCommand()
        {
            SelectCPUs(false);
        }

        private void SelectCPUs(bool isSelect)
        {
            foreach (ProjectModel model in CollectionItems)
            {
                if (model.ChildCPU == null) return;

                foreach (ProjectModel cpuModel in model.ChildCPU)
                    cpuModel.IsSelected = isSelect;
            }
        }
        #endregion

        #endregion


    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Media;
using Dothan.Manager;

namespace DothanTech.ViGET.SolutionExplorer.Model
{
    public class ProjectModel : BatchProjectNotificationObject
    {
        public ProjectModel()
        {
            this.ChildCPU = new ObservableCollection<ProjectModel>();
        }

        public ProjectModel Parent { get; set; }

        /// <summary>
        /// TreeviewItem CPU节点，是Parent的子节点
        /// </summary>
        public ObservableCollection<ProjectModel> ChildCPU
        {
            get;
            set;
        }

        /// <summary>
        /// 各节点的图标
        /// </summary>
        public ImageSource NodeIcon
        {
            get;
            set;
        }

        /// <summary>
        /// 各节点的名称
        /// </summary>
        public string NodeDisplayName
        {
            get;
            set;
        }

        /// <summary>
        /// 各节点是否选中状态
        /// </summary>
        private bool? _isSelected = null;
        public bool? IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                SetChecked(value);
            }
        }

        public bool? SelectedStatus
        {
            get
            {
                return IsSelected;
            }
        }

        /// <summary>
        /// 设置各个节点的选中状态
        /// </summary>
        /// <param name="status"></param>
        private void SetChecked(bool? status)
        {
            if (IsSelected == status) return;
            _isSelected = status;
            //选中和取消子类
            if (status.HasValue && this.ChildCPU != null)
                this.ChildCPU.ToLookup(child => child.IsSelected = status);

            //选中和取消父类
            if (this.Parent != null)
                this.Parent.CheckParentState();

            this.RaisePropertyChanged("IsSelected");
        }

        /// <summary>
        /// 检查父类是否选中
        /// 父类的子类中的状态是否一样，如果不一样父类就为NULL
        /// 循环子类节点,通过和子类的第一个节点对比是否是一样的选中状态
        /// </summary>
        private void CheckParentState()
        {
            bool? _currentState = this.IsSelected;
            bool? _firstState = null;
            for (int i = 0; i < this.ChildCPU.Count(); i++)
            {
                bool? childrenState = this.ChildCPU[i].IsSelected;
                if (i == 0)
                {
                    _firstState = childrenState;
                }
                else if (_firstState != childrenState)
                {
                    _firstState = null;
                }
            }
            if (_firstState != null) _currentState = _firstState;
            SetChecked(_firstState);
        }
    }
}

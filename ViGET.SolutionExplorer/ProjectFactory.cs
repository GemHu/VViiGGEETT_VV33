﻿using Dothan.ViObject;
using DothanTech.ViGET.ViCommand;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using Dothan.Manager;
using DothanTech.ViGET.ViService;
using System.ComponentModel.Composition;
using System.IO;
using Microsoft.Win32;
using Dothan.Helpers;
using Dothan.Controls;
using DothanTech.ViGET.Manager;
using System.ComponentModel.Composition.Hosting;
using DothanTech.ViGET.TemplateWizard;

namespace DothanTech.ViGET.SolutionExplorer
{
    /**
     * Solution Explorer视图中ViewModel管理器；
     * 对象Children中的第一个元素必须为 SolutionNode；
     * 如果需要进行排序，则第一个Child不参与排序；
     * 后期如果需要模仿VS中的效果，可以通过修改系统默认TreeView视图；在显示SolutionNode的时候不显示折叠图标即可；
     */
    [Export(typeof(IViProjectFactory))]
    public partial class ProjectFactory : ViFolderInfo<ProjectFactory>, IViProjectFactory, IPartImportsSatisfiedNotification
    {
        //private static ProjectFactory sInstance;
        //public static ProjectFactory GetInstance()
        //{
        //    if (ProjectFactory.sInstance == null)
        //        ProjectFactory.sInstance = new ProjectFactory();

        //    return ProjectFactory.sInstance;
        //}

        public ProjectFactory()
            : base(String.Empty)
        {

        }

        /// <summary>
        /// 新建项目模板向导；
        /// </summary>
        [Import]
        public ITemplateWizard TempWizard { get; set; }

        public IViDocManager DocManager
        {
            get { return null; }
        }

        public IViShellService ShellService
        {
            get { return null; }
        }

        public IViBuildManager BuildManager
        {
            get { return null; }
        }

        #region TheSolution

        public new SolutionManager TheSolution
        {
            get
            {
                if (this.Children.Count > 0)
                    return this.Children[0] as SolutionManager;
                return null;
            }
            protected set
            {
                // oldSolution
                SolutionManager oldSolution = this.Children.Count > 0 ? this.Children[0] as SolutionManager : null;
                if (oldSolution != null)
                {
                    oldSolution.Save();
                    oldSolution.CloseSolution();
                    this.RemoveChild(oldSolution);
                }
                // 2、添加新的Solution
                if (value != null)
                    this.AddChild(value);
                // 3
                this.RaiseSolutionChanged(oldSolution, value);
            }
        }

        #endregion

        #region 相关接口命令

        #region 通过向导方式，新建项目

        public void NewProject()
        {
            this.TempWizard.RunNewProjectWizard((file, projectType) =>
            {
                // 添加solutionNode节点；
                String slnPath = Path.GetDirectoryName(Path.GetDirectoryName(file));
                String slnFileName = Path.GetFileName(slnPath) + SolutionManager.Extention;
                String slnFile = System.IO.Path.Combine(slnPath, slnFileName);
                File.Create(slnFile).Close();
                //
                SolutionManager solution = SolutionManager.OpenSolution(slnFile);
                this.TheSolution = solution;
                // 添加项目；
                this.AddExistingProject(file);
            });
        }

        #endregion

        #region 打开已有项目

        public void OpenProject(String projectFile)
        {
            if (String.IsNullOrEmpty(projectFile))
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.InitialDirectory = "";
                dialog.Filter = "All Project Files(*.vgsln,*.vgstation)|*.vgsln;*.vgstation" +
                    "|Solution File(*.vgsln)|*.vgsln" +
                    "|Project File(*.vgstation)|*.vgstation";
                if (dialog.ShowDialog() == true)
                {
                    projectFile = dialog.FileName;
                }
            }

            if (!File.Exists(projectFile))
                return;

            if (SolutionManager.IsSolutionFile(projectFile))
            {
                SolutionManager solution = SolutionManager.OpenSolution(projectFile);
                if (solution == null)
                    return;

                this.TheSolution = solution;
            }
            else
            {
                this.AddExistingProject(projectFile);
            }
        }

        /// <summary>
        /// 新建一个Solution，并将给定的项目文件添加到改Solution中；
        /// </summary>
        /// <param name="projFile"></param>
        public void AddExistingProject(string projFile)
        {
            if (String.IsNullOrEmpty(projFile))
                return;
            if (!File.Exists(projFile))
                return;

            // 1、获取默认Solution名称；
            String slnName = Path.GetFileNameWithoutExtension(projFile) + SolutionManager.Extention;
            String slnFile = Path.Combine(Path.GetDirectoryName(projFile), slnName);
            slnFile = FileName.IncreateFileName(slnFile);
            File.Create(slnFile).Close();

            // 2、创建一个空的Solution；
            SolutionManager solution = SolutionManager.OpenSolution(slnFile);
            this.TheSolution = solution;
            // 3、添加已经存在的项目；
            this.TheSolution.OpenProject(projFile);
        }

        #endregion

        #region 以向导的方式添加新项目

        /// <summary>
        /// 通过向导方式添加新项目；
        /// </summary>
        public void AddNewProject()
        {
            SolutionManager solution = this.TheSolution;
            if (solution == null)
                return;

            this.TempWizard.RunAddNewProjectWizard(solution.DirectoryPath, (projectFile, type) =>
            {
                this.AddExistingProject(projectFile);
            });
        }

        #endregion

        #region 添加已经存在的项目

        /// <summary>
        /// 通过文件选择对话框的功能，添加已有项目；
        /// </summary>
        public void AddExistingProject()
        {
            if (this.TheSolution == null)
                return;

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.InitialDirectory = this.TheSolution.DirectoryPath;
            dialog.Filter = "Project Files(*.vgstation)|*.vgstation";
            if (dialog.ShowDialog() == true)
            {
                this.AddExistingProject(dialog.FileName);
            }
        }

        #endregion

        #region 关闭所有工程

        public void CloseSolution()
        {
            this.TheSolution = null;
        }

        #endregion

        #endregion

        public void OnImportsSatisfied()
        {
        }

        private void OnShellClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (ViMessageBox.Show("是否需要退出应用程序？", "Closing", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
            {
                this.TheSolution = null;
            }
            else
            {
                e.Cancel = true;
            }
        }
    }
}

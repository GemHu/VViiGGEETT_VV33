using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using DothanTech.ViGET.ViService;

namespace DothanTech.ViGET.Manager
{
    public partial class SolutionManager : IViBuildGroup
    {
        #region Build

        public void LoopBuildItem(Action<IViBuildItem> action)
        {
            foreach (var item in this.Children)
            {
                if (item is IViBuildGroup)
                {
                    (item as IViBuildGroup).LoopBuildItem(action);
                }
            }
        }

        //private void onBuildFinished(BuildTarget _target, bool success)
        //{
        //    bool lastjob = (theBuildTargets == null ||
        //                    theBuildTargets.Count() <= 0);

        //    switch (_target.action)
        //    {
        //        case BuildAction.DefaultAction:
        //        case BuildAction.BuildMakefile:
        //        case BuildAction.RebuildMakefile:
        //            {
        //                BuildMakefileTarget target = _target as BuildMakefileTarget;
        //                if (target != null && target.cpu != null)
        //                {
        //                    target.cpu.CurrProject.OnFileChanged(target.cpu, target.cpu.ViCPU.PCDFile, false);
        //                    target.cpu.CurrProject.OnBuildEnd(target.cpu, success, lastjob);
        //                    //target.cpu.Project.UpdateProject(ProjectManager.UpdateProjectType.Project);
        //                }
        //            }
        //            break;
        //        case BuildAction.CheckFileSyntax:
        //            {
        //                CheckFileSyntaxTarget target = _target as CheckFileSyntaxTarget;
        //                ProjectManager project = theSolutionManager.GetProject(target.fileName);
        //                if (project != null)
        //                    project.OnSyntaxCheckEnd(target.fileName, success, lastjob);
        //            }
        //            break;
        //    }
        //}

        #endregion

        #region SyntaxCheck

        ///// <summary>
        ///// 文件语法检查。
        ///// </summary>
        //public class CheckFileSyntaxTarget : BuildTarget
        //{
        //    public CheckFileSyntaxTarget(string fileName)
        //    {
        //        this.fileName = fileName;
        //        this.action = BuildAction.CheckFileSyntax;
        //    }

        //    public string fileName { get; protected set; }

        //    public override string Key { get { return fileName.ToUpper(); } }
        //    public override ProjectManager Project { get { return theSolutionManager.GetProject(fileName); } }
        //}

        ///// <summary>
        ///// 当前是否正在进行文件语法检查？
        ///// </summary>
        //public bool SyntaxCheckInProgress
        //{
        //    get
        //    {
        //        return (this.BuildTargetActions & BuildAction.CheckFileSyntax) == BuildAction.CheckFileSyntax;
        //    }
        //}

        ///// <summary>
        ///// 对指定的文件进行语法检查。
        ///// </summary>
        //public bool SyntaxCheck(string file)
        //{
        //    // invalid argument
        //    if (string.IsNullOrEmpty(file))
        //        return false;

        //    // prepare build item list
        //    addBuildTarget(new CheckFileSyntaxTarget(file));

        //    // kick-off DTE build
        //    return Build(false);
        //}

        #endregion

        #region SearchInFiles

        ///// <summary>
        ///// 工程中文本搜索。
        ///// </summary>
        //public class SearchInFilesTarget : BuildTarget
        //{
        //    public SearchInFilesTarget(ProjectManager project, string searchText)
        //    {
        //        this.project = project;
        //        this.searchText = searchText;
        //        this.action = BuildAction.SearchInFiles;
        //    }

        //    public override ProjectManager Project { get { return project; } }
        //    public ProjectManager project { get; protected set; }
        //    public string searchText { get; protected set; }

        //    public override string Key { get { return project.Key; } }
        //}

        /// <summary>
        /// 在指定的工程中进行文本搜索。
        /// </summary>
        public void SearchInFiles(ProjectManager project)
        {
            //project.theProjectNode.OnSearchInFiles();
        }

        /// <summary>
        /// 在指定的工程中进行文本搜索。
        /// </summary>
        public void SearchInFiles(ProjectManager project, string searchText)
        {
            //if (project == null) return;
            //if (string.IsNullOrEmpty(searchText)) return;

            //addBuildTarget(new SearchInFilesTarget(project, searchText));

            //Build(false);
        }

        #endregion
    }
}

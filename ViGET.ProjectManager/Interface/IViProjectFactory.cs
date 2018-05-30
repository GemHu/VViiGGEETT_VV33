using DothanTech.ViGET.ViService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DothanTech.ViGET.Manager
{
    public interface IViProjectFactory
    {
        /// <summary>
        /// 创建新项目(解决方案);
        /// </summary>
        void NewProject();

        /// <summary>
        /// 通过选择对话框的方式打开一个已有项目；
        /// </summary>
        void OpenProject(String projectFile);

        /// <summary>
        /// 通过向导方式，添加新项目；
        /// </summary>
        void AddNewProject();

        /// <summary>
        /// 添加一个已经存在的项目；
        /// </summary>
        void AddExistingProject(String projectFile);

        /// <summary>
        /// 关闭解决方案；
        /// </summary>
        void CloseSolution();

        ITemplateWizard TempWizard { get; }

        IViDocManager DocManager { get; }

        IViShellService ShellService { get; }

        IViBuildManager BuildManager { get; }

    }
}

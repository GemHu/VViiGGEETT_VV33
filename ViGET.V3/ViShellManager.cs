using Dothan.Helpers;
using DothanTech.ViGET.SolutionExplorer;
using DothanTech.ViGET.ViService;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xceed.Wpf.AvalonDock.Layout;

namespace DothanTech.ViGET.Shell
{
    [Export(typeof(IViShellService))]
    public class ViShellManager : IViShellService
    {
        /// <summary>
        /// 根据扩展名，获取对应的Type类型；
        /// </summary>
        private static Dictionary<String, Type> DFactoryTypes = new Dictionary<string, Type>();
        /// <summary>
        /// 根据Type，获取对应的编辑器工厂类；
        /// </summary>
        private static Dictionary<Type, IViEditorFactory> DEditorFactorys = new Dictionary<Type, IViEditorFactory>();

        public ViShellManager()
        {

        }

        /// <summary>
        /// 注册编辑器工厂类到给定的扩展名；
        /// </summary>
        public bool RegistorEditorFactory(string extention, Type type)
        {
            if (String.IsNullOrEmpty(extention) || type == null)
                return false;

            String key = extention.ToUpper();
            if (DFactoryTypes.ContainsKey(key))
                return false;

            DFactoryTypes[key] = type;
            return true;
        }

        /// <summary>
        /// 根据给定的扩展名，获取目标编辑器工厂实例；
        /// </summary>
        public static IViEditorFactory GetEditorFactory(String extention)
        {
            if (String.IsNullOrEmpty(extention))
                return null;

            String key = extention.ToUpper();
            Type factoryType = DFactoryTypes.ContainsKey(key) ? DFactoryTypes[key] : null;
            if (factoryType == null)
                return null;

            try
            {
                IViEditorFactory factory = DEditorFactorys.ContainsKey(factoryType) ? DEditorFactorys[factoryType] : null;
                if (factory == null)
                {
                    factory = Activator.CreateInstance(factoryType) as IViEditorFactory;
                    DEditorFactorys[factoryType] = factory;
                }

                return factory;
            }
            catch (Exception ee)
            {
                return null;
            }
        }

        #region 项目相关配置信息

        private const String RecentProjectKey = "RecentProjects";
        private const String ItemSeperator = ";";

        /// <summary>
        /// 获取最近打开项目列表；
        /// </summary>
        public List<String> GetRecentProjects()
        {
            // 取出最近10条记录；
            String values = new DzAppDataConfig().GetValue(RecentProjectKey);
            if (String.IsNullOrEmpty(values))
                return new List<string>();

            return values.Split(new String[]{ItemSeperator}, StringSplitOptions.RemoveEmptyEntries).Take(10).ToList();
        }

        /// <summary>
        /// 添加最近项目记录；
        /// </summary>
        public void AddRecentProject(String projectFile){
            List<String> valueList = GetRecentProjects();
            valueList.Remove(projectFile);
            valueList.Insert(0, projectFile);

            String newValue = String.Join(ItemSeperator, valueList);
            new DzAppDataConfig().SetValue(RecentProjectKey, newValue);
        }

        #endregion

        #region Closing事件

        public event CancelEventHandler Closing;

        public void RaiseClosing(object sender, CancelEventArgs e)
        {
            if (this.Closing != null)
            {
                this.Closing.Invoke(sender, e);
            }
        }

        #endregion

        public ProjectFactory ProjectFactory
        {
            get
            {
                return ProjectFactory.GetInstance();
            }
        }

        public void NewProject()
        {
            if (this.ProjectFactory != null)
            {
                if (this.ProjectFactory.NewProjectCmd.CanExecute())
                    this.ProjectFactory.NewProjectCmd.Execute();
            }
        }

        public bool OpenProject(string projectFile)
        {
            if (this.ProjectFactory == null)
                return false;

            if (this.ProjectFactory.OpenProjectCmd.CanExecute(projectFile))
            {
                this.ProjectFactory.OpenProjectCmd.Execute(projectFile);
                return true;
            }

            return false;
        }
    }
}

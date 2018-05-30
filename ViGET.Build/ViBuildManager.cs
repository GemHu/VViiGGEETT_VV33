using DothanTech.ViGET.ViService;
using Prism.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace DothanTech.ViGET.Compiler
{
    /// <summary>
    /// 项目编译管理器；
    /// 由于编译文件是一件比较耗时的操作，因此变异的过程需要放到工作现成中进行；
    /// </summary>
    [Export(typeof(IViBuildManager))]
    public class ViBuildManager : IViBuildManager
    {
        ///// <summary>
        ///// Output信息输出窗口；
        ///// </summary>
        //[Import]
        //private ILoggerFacade OutputWindow;

        ///// <summary>
        ///// ErrorList 错误提示窗口；
        ///// </summary>
        //[Import]
        //private IViErrorListLogger ErrorListWindow;

        private bool stop = false;

        public bool Build(IViBuildGroup group, bool rebuild)
        {
            this.loopBuildItem(group, (item) =>
            {
                this.BuildItem(item, rebuild);
            });

            return true;
        }

        public bool Build(IViBuildItem item, bool rebuild = false)
        {
            return this.BuildItem(item, rebuild);
        }

        public bool Clean(IViBuildGroup group)
        {
            this.loopBuildItem(group, (item) =>
            {
                this.CleanItem(item);
            });

            return true;
        }

        public bool Clean(IViBuildItem item)
        {
            return this.CleanItem(item);
        }

        private void loopBuildItem(IViBuildGroup group, Action<IViBuildItem> action)
        {
            //List<IViBuildItem> buildItems = new List<IViBuildItem>();
            group.LoopBuildItem((item) =>
            {
                //buildItems.Add(item);
                action(item);
            });
        }

        private bool BuildItem(IViBuildItem item, bool rebuild)
        {
            if (rebuild)
            {
                this.Log(String.Format("Start RebuildItem: {0}", item.BuildFile));
                this.Log(String.Format("End RebuildItem: {0}", item.BuildFile));
            }
            else
            {
                this.Log(String.Format("Start BuildItem: {0}", item.BuildFile));
                this.Log(String.Format("End BuildItem: {0}", item.BuildFile));
            }

            return true;
        }

        private bool CleanItem(IViBuildItem item)
        {
            this.Log(String.Format("Start CleanItem: {0}", item.BuildFile));
            this.Log(String.Format("End   CleanItem: {0}", item.BuildFile));

            return true;
        }

        private void Log(String msg)
        {
            Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() => {
                //this.OutputWindow.Log(msg, Category.Debug, Priority.None);
            }));
        }


        public void Stop()
        {
            this.stop = true;
        }
    }
}

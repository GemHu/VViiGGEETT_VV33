using Prism.Mef;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using DothanTech.ViGET.SolutionExplorer;
using DothanTech.ViGET.TemplateWizard;
using DothanTech.ViGET.ViService;
using DothanTech.ViGET.CFCEditor;
using DothanTech.ViGET.Build;
using ViGET.HWCEditor;
using ViGET.NWCEditor;
using DothanTech.ViGET.Online;
using System.Threading;
using System.Globalization;

namespace DothanTech.ViGET.Shell
{
    public class ViBootstrapper : MefBootstrapper
    {
        private readonly ViOutputLogger outputLogger = new ViOutputLogger();
        private readonly ViErrorListLogger errorListLogger = new ViErrorListLogger();

        protected override System.Windows.DependencyObject CreateShell()
        {
            // AvalanDock默认语言是繁体中文，还不如直接切换成英文；
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en");

            return this.Container.GetExportedValue<ViShell>();
        }

        protected override void InitializeShell()
        {
            base.InitializeShell();

            Application.Current.MainWindow = (ViShell)this.Shell;
            Application.Current.MainWindow.Show();
        }

        protected override void ConfigureAggregateCatalog()
        {
            // 配置相关需要注入的组件，否则无法通过MEF来实例化相关对象；
            // 该方法实在CreateShell之前调用的，也就是说在创建窗体事前，会先执行方法来配置相关MEF组件；
            base.ConfigureAggregateCatalog();

            // 添加当前程序集后，才可以通过依赖注入的方式来创建Shell主窗体；
            this.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(ViBootstrapper).Assembly));
            // 项目创建向导模块；
            this.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(TemplateWizardModule).Assembly));
            //// 配置SolutionExplorer模块；
            //this.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(SolutionExplorerModule).Assembly));
            //
            this.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(CFCEditorModule).Assembly));
            //
            this.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(HWCEditorModule).Assembly));
            //
            this.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(NWCEditorModule).Assembly));
            //
            this.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(ViBuildModule).Assembly));
            //
            this.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(ViOnlineModule).Assembly));
        }

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();

            this.Container.ComposeExportedValue<ViOutputLogger>(this.outputLogger);
            this.Container.ComposeExportedValue<ViErrorListLogger>(this.errorListLogger);
        }

        protected override Prism.Logging.ILoggerFacade CreateLogger()
        {
            return this.outputLogger;
        }
    }
}

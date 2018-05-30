using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace DothanTech.ViGET.Shell
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 通过boostrapper来创建主窗体对象；
            ViBootstrapper bootstrapper = new ViBootstrapper();
            bootstrapper.Run();
        }
    }
}

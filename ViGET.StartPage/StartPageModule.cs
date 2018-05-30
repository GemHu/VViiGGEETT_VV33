using DothanTech.ViGET.ViService;
using Prism.Mef.Modularity;
using Prism.Modularity;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViGET.StartPage
{
    [ModuleExport(typeof(StartPageModule))]
    public class StartPageModule : IModule
    {
        [ImportingConstructor]
        public StartPageModule(IViDocManager docManager)
        {
        }

        public void Initialize()
        {
            
        }
    }
}

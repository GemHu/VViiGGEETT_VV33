using Prism.Mef.Modularity;
using Prism.Modularity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DothanTech.ViGET.Online
{
    [ModuleExport(typeof(ViOnlineModule))]
    public class ViOnlineModule : IModule
    {
        public void Initialize()
        {
        }
    }
}

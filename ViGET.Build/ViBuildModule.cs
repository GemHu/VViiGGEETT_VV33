using Prism.Mef.Modularity;
using Prism.Modularity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DothanTech.ViGET.Build
{
    [ModuleExport(typeof(ViBuildModule))]
    public class ViBuildModule : IModule
    {
        public void Initialize()
        {
        }
    }
}

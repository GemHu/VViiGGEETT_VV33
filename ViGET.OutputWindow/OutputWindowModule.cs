using Prism.Mef.Modularity;
using Prism.Modularity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DothanTech.OutputWindow
{
    [ModuleExport(typeof(OutputWindowModule))]
    public class OutputWindowModule : IModule
    {
        public void Initialize()
        {
            
        }
    }
}

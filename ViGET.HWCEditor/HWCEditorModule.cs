using DothanTech.ViGET.ViService;
using Prism.Mef.Modularity;
using Prism.Modularity;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViGET.HWCEditor
{
    [ModuleExport(typeof(HWCEditorModule))]
    public class HWCEditorModule : IModule
    {
        [ImportingConstructor]
        public HWCEditorModule(IViShellService service)
        {
            if (service != null)
            {
                service.RegistorEditorFactory(Dothan.ViObject.Extension.HWCFile, typeof(HWCEditorFactory));
            }
        }
        public void Initialize()
        {
            
        }
    }
}

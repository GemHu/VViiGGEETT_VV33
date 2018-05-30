using DothanTech.ViGET.ViService;
using Prism.Mef.Modularity;
using Prism.Modularity;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViGET.NWCEditor
{
    [ModuleExport(typeof(NWCEditorModule))]
    public class NWCEditorModule : IModule
    {
        [ImportingConstructor]
        public NWCEditorModule(IViShellService service)
        {
            if (service != null)
            {
                service.RegistorEditorFactory(Dothan.ViObject.Extension.NWCFile, typeof(NWCEditorFactory));
            }
        }

        public void Initialize()
        {
        }
    }
}

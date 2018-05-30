using DothanTech.ViGET.ViService;
using Prism.Mef.Modularity;
using Prism.Modularity;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DothanTech.ViGET.CFCEditor
{
    [ModuleExport(typeof(CFCEditorModule))]
    public class CFCEditorModule : IModule
    {
        [ImportingConstructor]
        public CFCEditorModule(IViShellService service)
        {
            if (service != null)
            {
                service.RegistorEditorFactory(Dothan.ViObject.Extension.CFCProgram, typeof(CFCEditorFactory));
            }
        }

        public void Initialize()
        {
        }
    }
}

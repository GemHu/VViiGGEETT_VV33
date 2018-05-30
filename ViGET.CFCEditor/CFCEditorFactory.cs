using DothanTech.ViGET.ViService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DothanTech.ViGET.CFCEditor
{
    internal class CFCEditorFactory : IViEditorFactory
    {
        public IViEditor CreateEditorInstance()
        {
            return new CFCEditor();
        }
    }
}

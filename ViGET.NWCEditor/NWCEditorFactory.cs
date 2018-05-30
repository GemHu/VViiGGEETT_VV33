using DothanTech.ViGET.ViService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ViGET.NWCEditor
{
    public class NWCEditorFactory : IViEditorFactory
    {
        public IViEditor CreateEditorInstance()
        {
            return new NWCEditor();
        }
    }
}

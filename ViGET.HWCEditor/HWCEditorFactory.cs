using DothanTech.ViGET.ViService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ViGET.HWCEditor
{
    public class HWCEditorFactory : IViEditorFactory
    {
        public IViEditor CreateEditorInstance()
        {
            return new HWCEditor();
        }
    }
}

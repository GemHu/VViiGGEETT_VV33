using DothanTech.ViGET.ViService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DothanTech.ViGET.CFCEditor
{
    public class CFCEditor : ViEditor<UcCFCEditor>
    {
        protected override bool LoadFile(string fileName)
        {
            this.UIControl.EditorModule = this;
            return true;
        }

        protected override bool SaveFile(string fileName)
        {
            return true;
        }
    }
}

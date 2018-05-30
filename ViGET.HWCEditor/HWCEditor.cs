using DothanTech.ViGET.ViService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ViGET.HWCEditor
{
    public class HWCEditor : ViEditor<UcHWCEditor>
    {
        public HWCEditor()
        {

        }

        protected override bool LoadFile(string fileName)
        {
            this.UIControl.FileName = fileName;
            return true;
        }

        protected override bool SaveFile(string fileName)
        {
            return true;
        }
    }
}

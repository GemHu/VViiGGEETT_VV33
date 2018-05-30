using DothanTech.ViGET.ViService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace ViGET.NWCEditor
{
    public class NWCEditor : ViEditor<UcNWCEditor>
    {
        public NWCEditor()
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

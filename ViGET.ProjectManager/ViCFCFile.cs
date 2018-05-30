using Dothan.ViObject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DothanTech.ViGET.Manager
{
    public class ViCFCFile : ViFileInfo
    {
        public ViCFCFile(String file) : base(file)
        {
        }

        public override void DoubleClick(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.DoubleClick(e);

            if (File.Exists(this.FullPath) && this.TheFactory != null && this.TheFactory.DocManager != null)
            {
                this.TheFactory.DocManager.OpenDocument(this.FullPath);
            }
        }
    }
}

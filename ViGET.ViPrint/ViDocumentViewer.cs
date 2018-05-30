using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Printing;
using System.Drawing.Printing;
using System.Windows.Documents;

namespace Dothan.Print
{
    /// <summary>
    /// 系统默认的DocumentViewer在打印的时候无法修改打印机的相关参数，所以需要集成后，修改默认的打印响应函数。
    /// </summary>
    public class ViDocumentViewer : DocumentViewer
    {
        public FixedDocument PrintDocument
        {
            get;
            set;
        }

        protected override void OnPrintCommand()
        {
            DzPrinter.Print(this, this.PrintDocument);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Xps.Packaging;
using System.Windows.Xps;
using System.IO;
using System.Printing;

namespace Dothan.Print
{
    /// <summary>
    /// Interaction logic for PreviewDocument.xaml
    /// </summary>
    public partial class PrintPreviewer : Window
    {
        public PrintPreviewer(FixedDocument doc)
        {
            InitializeComponent();

            initDocumentPreview(doc);
        }

        private void initDocumentPreview(FixedDocument doc)
        {
            if (doc == null || doc.Pages.Count <= 0)
                return;

            this.printPreviewer.PrintDocument = doc;

            string tempFile = Path.GetTempFileName();
            if (!File.Exists(tempFile))
                File.Create(tempFile);

            XpsDocument xpsDocument = new XpsDocument(tempFile, FileAccess.Write);
            XpsDocumentWriter writer = XpsDocument.CreateXpsDocumentWriter(xpsDocument);
            writer.Write(doc);
            xpsDocument.Close();

            xpsDocument = new XpsDocument(tempFile, FileAccess.Read);
            printPreviewer.Document = xpsDocument.GetFixedDocumentSequence();
            xpsDocument.Close();
        }
    }
}

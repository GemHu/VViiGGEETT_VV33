using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Printing;
using System.Windows.Xps.Packaging;
using System.Windows.Xps;
using System.Drawing.Printing;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Xps.Serialization;
using System.Windows.Documents.Serialization;

using Dothan.Helpers;

namespace Dothan.Print
{
    public class DzPrinter
    {
        /// <summary>
        /// 打印指定的文档。
        /// </summary>
        /// <param name="owner">显示参数设置的父窗口，可以为 DependencyObject 或 Visual。为 null 表示使用当前激活窗口。</param>
        /// <param name="doc">需要打印的文档对象。</param>
        public static void Print(Object owner, FixedDocument doc)
        {
            if (doc == null || doc.Pages.Count <= 0)
                return;

            PrintDialog dialog = new PrintDialog();
            if (dialog.ShowDialog() != true)
                return;

            PrintQueue printer = dialog.PrintQueue;
            PrintTo(printer, doc);
        }

        /// <summary>
        /// 打印指定的一系列文档。
        /// </summary>
        /// <param name="owner">显示参数设置的父窗口，可以为 DependencyObject 或 Visual。为 null 表示使用当前激活窗口。</param>
        /// <param name="docs">需要打印的一系列文档对象。</param>
        public static void Print(Object owner, IEnumerable<FixedDocument> docs)
        {
            if (docs == null)
                return;

            PrintDialog dialog = new PrintDialog();
            if (dialog.ShowDialog() != true)
                return;

            PrintQueue printer = dialog.PrintQueue;

            foreach (var doc in docs)
                PrintTo(printer, doc);
        }

        /// <summary>
        /// 向指定的打印机对象打印文档。
        /// </summary>
        /// <param name="printer">指定的打印机对象。</param>
        /// <param name="doc">需要打印的文档对象。</param>
        public static void PrintTo(PrintQueue printer, FixedDocument doc)
        {
            if (printer == null || doc == null || doc.Pages.Count <= 0)
                return;

            try
            {
                PrintTicket setting = printer.UserPrintTicket ?? printer.DefaultPrintTicket;
                doc.PrintTicket = setting;
                //
                XpsDocumentWriter xpsWriter = PrintQueue.CreateXpsDocumentWriter(printer);
                xpsWriter.WritingPrintTicketRequired += (sender, e) =>
                {
                    if (e.CurrentPrintTicketLevel == PrintTicketLevel.FixedPagePrintTicket &&
                        e.Sequence >= 1 && e.Sequence <= doc.Pages.Count)
                    {
                        // 为每个页面设置自己的纸张打印方向
                        setting.PageOrientation = GetPageOrientation(doc.Pages[e.Sequence - 1]);
                        e.CurrentPrintTicket = setting;
                    }
                };
                xpsWriter.Write(doc, setting);
            }
            catch (Exception ee)
            {
                Trace.WriteLine("### [ " + ee.Source + " ] Exception : " + ee.Message);
                Trace.WriteLine("### " + ee.StackTrace);
            }
        }

        /// <summary>
        /// 打印预览指定的文档。
        /// </summary>
        /// <param name="owner">显示参数设置的父窗口，可以为 DependencyObject 或 Visual。为 null 表示使用当前激活窗口。</param>
        /// <param name="doc">需要打印预览的文档对象。</param>
        public static void PrintPreview(Object owner, FixedDocument doc)
        {
            if (doc == null || doc.Pages.Count <= 0)
                return;

            PrintPreviewer previewer = new PrintPreviewer(doc);
            SetWindowOwner(previewer, owner);
            previewer.ShowDialog();
        }

        /// <summary>
        /// 打印参数设置。
        /// </summary>
        /// <param name="owner">显示参数设置的父窗口，可以为 DependencyObject 或 Visual。为 null 表示使用当前激活窗口。</param>
        public static void PrintSetup(Object owner)
        {
            PrintPageSetup dialog = new PrintPageSetup();
            SetWindowOwner(dialog, owner);
            dialog.ShowDialog();
        }

        /// <summary>
        /// 得到本机打印机对象。
        /// </summary>
        public static PrintQueue GetPrintQueue(string printerName)
        {
            if (string.IsNullOrEmpty(printerName))
                return null;

            PrinterSettings.StringCollection pss = PrinterSettings.InstalledPrinters;
            if (pss == null || pss.Count <= 0)
                return null;

            for (int i = 0; i < pss.Count; i++)
            {
                if (pss[i].StartsWith(printerName, StringComparison.OrdinalIgnoreCase))
                {
                    using (LocalPrintServer local = new LocalPrintServer())
                    {
                        return new PrintQueue(local, pss[i]);
                    }
                }
            }

            return null;
        }

        protected static HwndSource GetSafeHwndSource(Object owner)
        {
            HwndSource source = null;
            if (owner is DependencyObject)
                source = HwndSource.FromDependencyObject(owner as DependencyObject) as HwndSource;
            else if (owner is Visual)
                source = HwndSource.FromVisual(owner as Visual) as HwndSource;
            else if (owner is IntPtr)
                source = HwndSource.FromHwnd((IntPtr)owner) as HwndSource;

            if (source != null)
                return source;

            return HwndSource.FromHwnd(HwndHelper.GetForegroundWindow()) as HwndSource;
        }

        protected static bool SetWindowOwner(Window window, Object owner)
        {
            HwndSource source = GetSafeHwndSource(owner);
            if (source == null) return false;

            new WindowInteropHelper(window) { Owner = source.Handle };
            return true;
        }

        protected static PageOrientation GetPageOrientation(PageContent page)
        {
            return GetPageOrientation(page != null ? page.Child : null);
        }

        protected static PageOrientation GetPageOrientation(FixedPage page)
        {
            if (page == null)
                return PageOrientation.Unknown;

            return page.Width > page.Height ?
                PageOrientation.Landscape : PageOrientation.Portrait;
        }
    }
}

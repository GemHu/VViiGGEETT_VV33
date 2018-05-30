/// <summary>
/// @file   ViPrintPage.cs
///	@brief  CFC页面打印相关的代码。
/// @author	DothanTech 胡殿兴
/// 
/// Copyright(C) 2011~2014, DothanTech. All rights reserved.
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

using Dothan.Helpers;
using System.IO;
using System.Diagnostics;

namespace Dothan.Print
{
    public class ViPrintPage : Canvas
    {
        #region Life Cycle

        /// <summary>
        /// 初始化，获取打印纸张的头部与尾部信息。
        /// </summary>
        public ViPrintPage(ViPrintParams printParams)
        {
            this._PrintParams = printParams;
        }

        #endregion

        #region PrintParams

        protected ViPrintParams PrintParams
        {
            get
            {
                return this._PrintParams;
            }
        }
        private ViPrintParams _PrintParams;

        #endregion

        #region ContentBox

        protected Rect ContentBox
        {
            get
            {
                return this.PrintParams.Paper.ContentBox;
            }
        }

        #endregion

        #region CurrentPage

        public int CurrentPage
        {
            get;
            set;
        }

        #endregion

        #region Draw Print Page

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            if (this.PrintParams == null || this._PrintParams.Paper == null)
                return;

            Rect printBox = this._PrintParams.Paper.PageBox;
            // Header
            double headerInfoHeight = (ContentBox.Top - printBox.Top) * 3f / 4f;
            Rect rcHeader = new Rect(ContentBox.Left, printBox.Top, ContentBox.Width, headerInfoHeight);
            this.DrawHeader(dc, rcHeader);
            //
            dc.DrawLine(this.PrintParams.BorderNormal, new Point(ContentBox.Left, headerInfoHeight), new Point(ContentBox.Right, headerInfoHeight));

            // Footer
            double footerInfoHeight = (printBox.Bottom - ContentBox.Bottom) * 3f / 4f;
            dc.DrawLine(this.PrintParams.BorderNormal, new Point(ContentBox.Left, printBox.Bottom - footerInfoHeight), new Point(ContentBox.Right, printBox.Bottom - footerInfoHeight));
            //
            Rect rcFooter = new Rect(ContentBox.Left, printBox.Bottom - footerInfoHeight, ContentBox.Width, footerInfoHeight);
            this.drawFooter(dc, rcFooter);
        }

        private void DrawHeader(DrawingContext dc, Rect rcHeader)
        {
            string txtLeft = this.GetPrintText(PrintPageSetup.HeaderLeftInfo);
            string txtCenter = this.GetPrintText(PrintPageSetup.HeaderCenterInfo);
            string txtRight = this.GetPrintText(PrintPageSetup.HeaderRightInfo);
            List<Rect> regionList = this.calcPageHeaderFootorReagions(dc, rcHeader, txtLeft, txtCenter, txtRight, true);

            // Left
            if (regionList[0] != Rect.Empty)
                //this.drawHeaderText(dc, txtLeft, regionList[0], HorizontalAlignment.Left);
                this.drawText(dc, txtLeft, regionList[0], HorizontalAlignment.Left, PrintPageSetup.HeaderLeftInfo, EMeasureType.MeasureHeader);

            // Center
            if (regionList[1] != Rect.Empty)
                //this.drawHeaderText(dc, txtCenter, regionList[1], HorizontalAlignment.Center);
                this.drawText(dc, txtCenter, regionList[1], HorizontalAlignment.Center, PrintPageSetup.HeaderCenterInfo, EMeasureType.MeasureHeader);

            // Right
            if (regionList[2] != Rect.Empty)
                //this.drawHeaderText(dc, txtRight, regionList[2], HorizontalAlignment.Right);
                this.drawText(dc, txtRight, regionList[2], HorizontalAlignment.Right, PrintPageSetup.HeaderRightInfo, EMeasureType.MeasureHeader);
        }

        protected double MeasureHeaderText(DrawingContext dc, string txtInfo)
        {
            if (string.IsNullOrEmpty(txtInfo))
                return 0.0;

            return dc.MeasureText(txtInfo, this.PrintParams.FontFaceHeader, this.PrintParams.FontSizeHeader, this.PrintParams.FontColorNormal);
        }

        private void drawHeaderText(DrawingContext dc, string text, Rect rcText, HorizontalAlignment hAlign)
        {
            if (string.IsNullOrEmpty(text))
                return;

            dc.DrawText(text, this.PrintParams.FontFaceHeader, this.PrintParams.FontSizeHeader, this.PrintParams.FontColorNormal,
                rcText, hAlign, VerticalAlignment.Bottom, true, TextTrimming.CharacterEllipsis);
        }

        private void drawFooter(DrawingContext dc, Rect rcFooter)
        {
            string txtLeft = this.GetPrintText(PrintPageSetup.FooterLeftInfo);
            string txtCenter = this.GetPrintText(PrintPageSetup.FooterCenterInfo);
            string txtRight = this.GetPrintText(PrintPageSetup.FooterRightInfo);
            List<Rect> regionList = this.calcPageHeaderFootorReagions(dc, rcFooter, txtLeft, txtCenter, txtRight, false);

            // Left
            if (regionList[0] != Rect.Empty)
                //this.drawFooterText(dc, txtLeft, regionList[0], HorizontalAlignment.Left);
                this.drawText(dc, txtLeft, regionList[0], HorizontalAlignment.Left, PrintPageSetup.FooterLeftInfo, EMeasureType.MeasureFooter);

            // Center
            if (regionList[1] != Rect.Empty)
                // this.drawFooterText(dc, txtCenter, regionList[1], HorizontalAlignment.Center);
                this.drawText(dc, txtCenter, regionList[1], HorizontalAlignment.Center, PrintPageSetup.FooterCenterInfo, EMeasureType.MeasureFooter);

            // Right
            if (regionList[2] != Rect.Empty)
                // this.drawFooterText(dc, txtRight, regionList[2], HorizontalAlignment.Right);
                this.drawText(dc, txtRight, regionList[2], HorizontalAlignment.Right, PrintPageSetup.FooterRightInfo, EMeasureType.MeasureFooter);
        }

        private double MeasureFooterText(DrawingContext dc, string txtInfo)
        {
            if (string.IsNullOrEmpty(txtInfo))
                return 0.0;

            return dc.MeasureText(txtInfo, this.PrintParams.FontFaceFooter, this.PrintParams.FontSizeFooter, this.PrintParams.FontColorNormal);
        }

        private void drawFooterText(DrawingContext dc, string text, Rect rcText, HorizontalAlignment hAlign)
        {
            if (string.IsNullOrEmpty(text))
                return;

            dc.DrawText(text, PrintParams.FontFaceFooter, PrintParams.FontSizeFooter, PrintParams.FontColorNormal,
                rcText, hAlign, VerticalAlignment.Top, true, TextTrimming.CharacterEllipsis);
        }

        private void drawNormalText(DrawingContext dc, string text, Rect rcText, HorizontalAlignment hAlign)
        {
            if (string.IsNullOrEmpty(text))
                return;

            dc.DrawText(text, PrintParams.FontFaceNormal, PrintParams.FontSizeNormal, PrintParams.FontColorNormal, 
                rcText, hAlign, VerticalAlignment.Center, true, TextTrimming.None);
        }

        /// <summary>
        /// 对drawHeaderText与drawFooterText函数进行了封装，当字符串过长的时候，可以进行相关的字符串截取操作；
        /// </summary>
        private void drawText(DrawingContext dc, string text, Rect rcText, HorizontalAlignment hAlign, string regType, EMeasureType drawHeader)
        {
            try
            {
                if (this.IsOnlyFilePath(regType))
                    text = this.SplitFilePath(text, rcText, dc, drawHeader);
                else
                    text = this.splitFromRight(text, rcText.Width, dc, drawHeader);

                if (drawHeader == EMeasureType.MeasureHeader)
                    this.drawHeaderText(dc, text, rcText, hAlign);
                else if (drawHeader == EMeasureType.MeasureFooter)
                    this.drawFooterText(dc, text, rcText, hAlign);
                else
                    this.drawNormalText(dc, text, rcText, hAlign);
            }
            catch (Exception ee)
            {
                Trace.WriteLine("### [" + ee.Source + "] Exception: " + ee.Message);
                Trace.WriteLine("### " + ee.StackTrace);
            }
        }

        /// <summary>
        /// 判断给定的类型是不是文件路径；
        /// </summary>
        private bool IsOnlyFilePath(string regData)
        {
            Dictionary<string, string> keys = ShowData.GetKeysByRegData(regData);
            if (keys.Count != 1)
                return false;

            foreach (string key in keys.Keys)
            {
                if (key.Equals(ShowData.STR_FILE_PATH) || key.Equals(ShowData.STR_PROJECT_PATH))
                    return true;
            }

            return false;
        }

        private double MeasureText(DrawingContext dc, string txtInfo, EMeasureType measureType)
        {
            double txtWidth = 0.0;
            switch (measureType)
            {
                case EMeasureType.MeasureHeader:
                    txtWidth = this.MeasureHeaderText(dc, txtInfo);
                    break;
                case EMeasureType.MeasureFooter:
                    txtWidth = this.MeasureFooterText(dc, txtInfo);
                    break;
                default:
                case EMeasureType.MeasureNormal:
                    txtWidth = this.MeasureNormalText(dc, txtInfo);
                    break;
            }

            return txtWidth;
        }

        protected double MeasureNormalText(DrawingContext dc, string txtInfo)
        {
            if (string.IsNullOrEmpty(txtInfo))
                return 0.0;

            return dc.MeasureText(txtInfo, this.PrintParams.FontFaceNormal, this.PrintParams.FontSizeNormal, this.PrintParams.FontColorNormal);
        }

        public enum EMeasureType
        {
            MeasureHeader,
            MeasureFooter,
            MeasureNormal
        }

        #endregion

        private List<Rect> calcPageHeaderFootorReagions(DrawingContext dc, Rect rcParent, string txtLeft, string txtCenter, string txtRight, bool isHeader)
        {
            Rect rcLeft;
            Rect rcCenter;
            Rect rcRight;
            double defaultTxtWidth = rcParent.Width / 3.0;
            double leftTxtWidth = isHeader ? this.MeasureHeaderText(dc, txtLeft) : this.MeasureFooterText(dc, txtLeft);
            double centerTxtWidth = isHeader ? this.MeasureHeaderText(dc, txtCenter) : this.MeasureFooterText(dc, txtCenter);
            double rightTxtWidth = isHeader ? this.MeasureHeaderText(dc, txtRight) : this.MeasureFooterText(dc, txtRight);

            if (string.IsNullOrEmpty(txtLeft))
            {
                if (string.IsNullOrEmpty(txtCenter))
                {
                    // 000
                    if (string.IsNullOrEmpty(txtRight))
                    {
                        rcLeft = rcCenter = rcRight = Rect.Empty;
                    }
                    // 001
                    else
                    {
                        rcLeft = rcCenter = Rect.Empty;
                        rcRight = rcParent;
                    }
                }
                else
                {
                    // 010
                    if (string.IsNullOrEmpty(txtRight))
                    {
                        rcLeft = rcRight = Rect.Empty;
                        rcCenter = rcParent;
                    }
                    // 011
                    else
                    {
                        if (rightTxtWidth < defaultTxtWidth)
                        {
                            leftTxtWidth = rightTxtWidth;
                            centerTxtWidth = defaultTxtWidth + (defaultTxtWidth - rightTxtWidth) * 2.0;
                        }
                        else if (centerTxtWidth < defaultTxtWidth)
                        {
                            leftTxtWidth = rightTxtWidth = defaultTxtWidth + (defaultTxtWidth - centerTxtWidth) / 2.0;
                        }
                        else
                        {
                            leftTxtWidth = centerTxtWidth = rightTxtWidth = defaultTxtWidth;
                        }

                        rcLeft = Rect.Empty;
                        rcCenter = new Rect(rcParent.Left + leftTxtWidth, rcParent.Top, centerTxtWidth, rcParent.Height);
                        rcRight = new Rect(rcParent.Left + leftTxtWidth + centerTxtWidth, rcParent.Top, rightTxtWidth, rcParent.Height);
                    }
                }
            }
            else
            {
                if (string.IsNullOrEmpty(txtCenter))
                {
                    // 100
                    if (string.IsNullOrEmpty(txtRight))
                    {
                        rcLeft = rcParent;
                        rcCenter = rcRight = Rect.Empty;
                    }
                    // 101
                    else
                    {
                        defaultTxtWidth = rcParent.Width / 2.0;
                        if (leftTxtWidth < defaultTxtWidth)
                            rightTxtWidth = defaultTxtWidth + (defaultTxtWidth - leftTxtWidth);
                        else if (rightTxtWidth < defaultTxtWidth)
                            leftTxtWidth = defaultTxtWidth + (defaultTxtWidth - rightTxtWidth);
                        else
                            leftTxtWidth = rightTxtWidth = defaultTxtWidth;

                        rcLeft = new Rect(rcParent.Left, rcParent.Top, leftTxtWidth, rcParent.Height);
                        rcCenter = Rect.Empty;
                        rcRight = new Rect(rcParent.Left + leftTxtWidth, rcParent.Top, rightTxtWidth, rcParent.Height);
                    }
                }
                else
                {
                    // 110
                    if (string.IsNullOrEmpty(txtRight))
                    {
                        if (leftTxtWidth < defaultTxtWidth)
                            centerTxtWidth = rcParent.Width - leftTxtWidth;
                        else if (centerTxtWidth < defaultTxtWidth)
                            leftTxtWidth = rightTxtWidth = defaultTxtWidth + (defaultTxtWidth - centerTxtWidth) / 2.0;
                        else
                            leftTxtWidth = centerTxtWidth = rightTxtWidth = defaultTxtWidth;

                        rcLeft = new Rect(rcParent.Left, rcParent.Top, leftTxtWidth, rcParent.Height);
                        rcCenter = new Rect(rcParent.Left + leftTxtWidth, rcParent.Top, centerTxtWidth, rcParent.Height);
                        rcRight = Rect.Empty;
                    }
                    // 111
                    else
                    {
                        if (centerTxtWidth < defaultTxtWidth)
                        {
                            leftTxtWidth = rightTxtWidth = defaultTxtWidth + (defaultTxtWidth - centerTxtWidth) / 2.0;
                        }
                        else if (leftTxtWidth < defaultTxtWidth && rightTxtWidth < defaultTxtWidth)
                        {
                            double offset = (defaultTxtWidth - leftTxtWidth > rightTxtWidth ? leftTxtWidth : rightTxtWidth) / 2.0;
                            leftTxtWidth = rightTxtWidth = defaultTxtWidth - offset;
                            centerTxtWidth = defaultTxtWidth + offset * 2.0;
                        }
                        else
                        {
                            leftTxtWidth = centerTxtWidth = rightTxtWidth = defaultTxtWidth;
                        }
                        rcLeft = new Rect(rcParent.Left, rcParent.Top, leftTxtWidth, rcParent.Height);
                        rcCenter = new Rect(rcParent.Left + leftTxtWidth, rcParent.Top, centerTxtWidth, rcParent.Height);
                        rcRight = new Rect(rcParent.Left + leftTxtWidth + centerTxtWidth, rcParent.Top, rightTxtWidth, rcParent.Height);
                    }
                }
            }

            List<Rect> regionList = new List<Rect>();
            regionList.Add(rcLeft);
            regionList.Add(rcCenter);
            regionList.Add(rcRight);

            return regionList;
        }

        private string GetPrintText(string regData)
        {
            string text = regData;
            Dictionary<string, string> keys = ShowData.GetKeysByRegData(regData);
            foreach (string key in keys.Keys)
            {
                text = text.Replace(keys[key], GetTextByKey(key));
            }

            return text;
        }

        private string GetTextByKey(string key)
        {
            DateTime dateTime = DateTime.Now;
            string printText = "";
            switch (key)
            {
                case ShowData.STR_PAGE:
                    printText = this.CurrentPage.ToString();
                    break;
                case ShowData.STR_PAGE_OF:
                    printText = this.PrintParams.TotalPages.ToString();
                    break;
                case ShowData.STR_CUR_DATE:
                    printText = dateTime.ToString("yyyy-MM-dd");
                    break;
                case ShowData.STR_CUR_TIME:
                    printText = dateTime.ToString("HH:mm");
                    break;
                case ShowData.STR_CUR_DATE_TIME:
                    printText = dateTime.ToString("yyyy-MM-dd HH:mm");
                    break;
                case ShowData.STR_FILE_PATH:
                    printText = this.PrintParams.CallBack.FileName;
                    break;
                case ShowData.STR_PROJECT_PATH:
                    printText = this.PrintParams.CallBack.ProjectPath;
                    break;
                case ShowData.STR_DATE_PRINTER:
                    printText = dateTime.ToString("yyyy-MM-dd HH:mm");
                    break;
                case ShowData.STR_DATE_CREATED:
                    dateTime = File.GetCreationTime(this.PrintParams.CallBack.FileName);
                    printText = dateTime.ToString("yyyy-MM-dd HH:mm");
                    break;
                case ShowData.STR_DATE_LAST_MODIFIED:
                    dateTime = File.GetLastWriteTime(this.PrintParams.CallBack.FileName);
                    printText = dateTime.ToString("yyyy-MM-dd HH:mm");
                    break;
                case ShowData.STR_OBJECT:
                    printText = Path.GetFileNameWithoutExtension(this.PrintParams.CallBack.FileName);
                    break;
                case ShowData.STR_OBJECT_TYPE:
                    printText = this.PrintParams.GetFileType();
                    break;
                default:
                    break;
            }

            return printText;
        }

        protected string SplitFilePath(string filePath, Rect region, DrawingContext dc, EMeasureType measerHeader)
        {
            // eg: D:\...\CPCFC1.CFC;
            if (string.IsNullOrEmpty(filePath))
                return filePath;
            filePath = filePath.Trim(new char[]{' ', '\\'});
            int startIndex = filePath.IndexOf('\\');
            int lastIndex = filePath.LastIndexOf('\\');
            if (startIndex <= 0 || lastIndex <= 0)
                return filePath;

            string prefix = filePath.Substring(0, startIndex + 1);
            string suffix = filePath.Substring(lastIndex);
            string splitContent = filePath.Substring(startIndex + 1, lastIndex - startIndex - 1);

            double prefixWidth = this.MeasureText(dc, prefix, measerHeader);
            double suffixWidth = this.MeasureText(dc, suffix, measerHeader);

            if (prefixWidth > region.Width)
                return prefix + "...";
            else if (prefixWidth + suffixWidth > region.Width)
                return prefix + this.splitFromLeft(suffix, region.Width - prefixWidth, dc, measerHeader);
            else
                return prefix + this.splitFromRight(splitContent, region.Width - prefixWidth - suffixWidth, dc, measerHeader) + suffix;
        }

        /// <summary>
        /// 从头部开始截取字符串。
        /// </summary>
        private string splitFromLeft(string target, double validLength, DrawingContext dc, EMeasureType measureHeader)
        {
            if (validLength <= 0 || string.IsNullOrEmpty(target))
                return string.Empty;
            if (this.MeasureText(dc, target, measureHeader) <= validLength)
                return target;

            // 当给定的长度太短的时候，只返回“...”；
            string prefix = "...";
            validLength -= this.MeasureText(dc, prefix, measureHeader);
            if (validLength <= 0)
                return prefix;
            
            // 截取字符串操作；
            string subString = target;
            int targetLength = target.Length;
            int index = 0;
            while (index < targetLength)
            {
                subString = target.Substring(index);
                if (this.MeasureText(dc, subString, measureHeader) < validLength)
                    break;

                index++;
            }

            return prefix + subString;
        }

        /// <summary>
        /// 从尾部开始截取字符串。
        /// </summary>
        private string splitFromRight(string target, double validLength, DrawingContext dc, EMeasureType measureHeader)
        {
            if (validLength <= 0 || string.IsNullOrEmpty(target))
                return string.Empty;
            if (this.MeasureText(dc, target, measureHeader) <= validLength)
                return target;

            // 当给定的长度太短的时候，只返回“...”；
            string suffix = "...";
            validLength -= this.MeasureText(dc, suffix, measureHeader);
            if (validLength <= 0)
                return suffix;

            // 截取字符串操作；
            string subString = target;
            int targetLength = target.Length;
            int length = targetLength;
            while (length >= 0)
            {
                subString = target.Substring(0, length);
                if (this.MeasureText(dc, subString, measureHeader) < validLength)
                    break;

                length--;
            }

            return subString + suffix;
        }
    }

}

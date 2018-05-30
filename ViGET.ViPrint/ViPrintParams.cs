using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dothan.Helpers;
using Dothan.ViObject;
using System.Windows.Media;
using System.Windows;

namespace Dothan.Print
{
    public class ViPrintParams
    {
        // 字体类型
        public Typeface FontFaceHeader;
        public Typeface FontFaceFooter;
        public Typeface FontFaceNormal;
        // 字体大小
        public double FontSizeHeader;
        public double FontSizeFooter;
        public double FontSizeNormal;
        // 字体颜色
        public Brush FontColorNormal;
        public Pen BorderNormal;
        
        public ViPrintParams(ViPaper paper, IPrintCallBack callBack)
        {
            this.Paper = paper;
            this.CallBack = callBack;

            // FontFace
            FontFaceHeader = new Typeface(new FontFamily("Arial"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);
            FontFaceFooter = FontFaceHeader;
            FontFaceNormal = new Typeface(new FontFamily("Arial"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

            // FontSize
            FontSizeHeader = 16.0f;
            FontSizeFooter = 16.0f;
            FontSizeNormal = 14.0f;
            //
            FontColorNormal = new SolidColorBrush(Colors.Black);
            BorderNormal = new Pen(new SolidColorBrush(Colors.Black), 1.0f);
        }

        #region PrintableArea

        public ViPaper Paper
        {
            get
            {
                return this._Paper;
            }
            protected set
            {
                this._Paper = value;
            }
        }
        private ViPaper _Paper;

        #endregion

        public int TotalPages
        {
            get;
            set;
        }

        public IPrintCallBack CallBack
        {
            get;
            set;
        }

        public string GetFileType()
        {
            return this.CallBack.GetFileType(this.CallBack.FileName);
        }

        /// <summary>
        /// 锁定 UI 绘制参数，以提高绘制效率。
        /// </summary>
        public virtual void Freeze()
        {
            FontColorNormal.Freeze();
            BorderNormal.Freeze();
        }

    }

    public interface IPrintCallBack
    {
        string ProjectPath { get; }

        string FileName { get; }

        string GetFileType(string fileName);
    }
}

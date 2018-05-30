/// <summary>
/// @file   ViPaper.cs
///	@brief  ViGET中所用到的纸张信息。
/// @author	DothanTech 胡殿兴
/// 
/// Copyright(C) 2011~2014, DothanTech. All rights reserved.
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dothan.Helpers;
using System.Windows;
using System.Windows.Documents;
using System.Printing;

namespace Dothan.Print
{
    /// <summary>
    /// ViGET中所用到的纸张信息。
    /// </summary>
    public class ViPaper
    {
        private const double marginLeftMM       = 15.0f;
        private const double marginRightMM      = 15.0f;
        private const double marginTopMM        = 12.0f;
        private const double marginBottomMM     = 12.0f;

        private double _PageWidthMM;
        private double _PageHeightMM;

        #region 初始化

        private ViPaper(double widthMM, double heightMM, PaperFormat paperFormat)
        {
            this._PageWidthMM = widthMM;
            this._PageHeightMM = heightMM;

            this._PaperFormat = paperFormat;
        }

        /// <summary>
        /// A4 的尺寸信息
        /// </summary>
        public static ViPaper CreateA4Paper(PageOrientation orientation)
        {
            return Create(297, 210, PaperFormat.A4, orientation);
        }

        /// <summary>
        /// A3 的尺寸信息
        /// </summary>
        public static ViPaper CreateA3Paper(PageOrientation orientation)
        {
            return Create(420, 297, PaperFormat.A3, orientation);
        }
        
        /// <summary>
        /// 创建指定尺寸的 Scale 参数信息。
        /// </summary>
        /// <param name="widthMM">纸张宽度（毫米）</param>
        /// <param name="heightMM">纸张高度（毫米）</param>
        /// <returns>指定尺寸的纸张信息。</returns>
        public static ViPaper Create(double widthMM, double heightMM, PaperFormat paperFormat, PageOrientation orientation)
        {
            ViPaper paper = new ViPaper(widthMM, heightMM, paperFormat);
            paper.Orientation = orientation;

            return paper;
        }

        #endregion

        #region 纸张大小

        public double PageWidthMM
        {
            get 
            {
                if (this.Orientation == PageOrientation.Landscape)
                    return this._PageWidthMM;
                else
                    return this._PageHeightMM;
            }
        }

        public double PageHeightMM
        {
            get
            {
                if (this.Orientation == PageOrientation.Landscape)
                    return this._PageHeightMM;
                else
                    return this._PageWidthMM;
            }
        }

        /// <summary>
        /// 纸张宽度，单位像素。
        /// </summary>
        public double PageWidth
        {
            get
            {
                if (this.Orientation == PageOrientation.Landscape)
                    return DzLength.Mm2Px(this._PageWidthMM);
                else
                    return DzLength.Mm2Px(this._PageHeightMM);
            }
        }

        /// <summary>
        /// 纸张高度，单位像素。
        /// </summary>
        public double PageHeight
        {
            get
            {
                if (this.Orientation == PageOrientation.Landscape)
                    return DzLength.Mm2Px(this._PageHeightMM);
                else
                    return DzLength.Mm2Px(this._PageWidthMM);
            }
        }

        /// <summary>
        /// 包括纸张边距在内的纸张尺寸，单位像素。
        /// </summary>
        public Size PageSize
        {
            get
            {
                return new Size(this.PageWidth, this.PageHeight);
            }
        }

        /// <summary>
        /// 包括纸张边距在内的纸张尺寸，单位像素。
        /// </summary>
        public Rect PageBox
        {
            get
            {
                return new Rect(0, 0, this.PageWidth, this.PageHeight);
            }
        }

        /// <summary>
        /// 不包括边界的纸张宽度，单位像素。
        /// </summary>
        public double ContentWidth
        {
            get
            {
                return this.PageWidth - this.MarginLeft - this.MarginRight;
            }
        }

        /// <summary>
        /// 不包括边界的纸张高度，单位像素。
        /// </summary>
        public double ContentHeight
        {
            get
            {
                return this.PageHeight - this.MarginTop - this.MarginBottom;
            }
        }

        /// <summary>
        /// 不包括边界的纸张尺寸，单位像素。
        /// </summary>
        public Size ContentSize
        {
            get
            {
                return new Size(this.ContentWidth, this.ContentHeight);
            }
        }

        /// <summary>
        /// 不包括边界的纸张尺寸，单位像素。
        /// </summary>
        public Rect ContentBox
        {
            get
            {
                return new Rect(this.MarginLeft, this.MarginTop, this.ContentWidth, this.ContentHeight);
            }
        }

        #endregion

        #region 纸张边距

        /// <summary>
        /// 左边距宽度，单位像素。
        /// </summary>
        public double MarginLeft
        {
            get
            {
                if (this.Orientation == PageOrientation.Landscape)
                    return DzLength.Mm2Px(marginLeftMM);
                else
                    return DzLength.Mm2Px(marginBottomMM);
            }
        }

        /// <summary>
        /// 右边距宽度，单位像素。
        /// </summary>
        public double MarginRight
        {
            get
            {
                if (this.Orientation == PageOrientation.Landscape)
                    return DzLength.Mm2Px(marginRightMM);
                else
                    return DzLength.Mm2Px(marginTopMM);
            }
        }

        /// <summary>
        /// 上边距宽度，单位像素。
        /// </summary>
        public double MarginTop
        {
            get
            {
                if (this.Orientation == PageOrientation.Landscape)
                    return DzLength.Mm2Px(marginTopMM);
                else
                    return DzLength.Mm2Px(marginLeftMM);
            }
        }

        /// <summary>
        /// 下边距宽度，单位像素。
        /// </summary>
        public double MarginBottom
        {
            get
            {
                if (this.Orientation == PageOrientation.Landscape)
                    return DzLength.Mm2Px(marginBottomMM);
                else
                    return DzLength.Mm2Px(marginRightMM);
            }
        }

        #endregion

        #region Orientation 纸张显示方向

        /// <summary>
        /// 纸张显示方向，横向或纵向。
        /// </summary>
        public PageOrientation Orientation
        {
            get 
            { 
                return this._Orientation; 
            }
            set
            {
                this._Orientation = value;
            }
        }
        private PageOrientation _Orientation = PageOrientation.Landscape;

        #endregion

        #region PaperFormat

        public PaperFormat PaperFormat
        {
            get { return _PaperFormat; }
        }
        private PaperFormat _PaperFormat;

        #endregion

        #region 其他常用方法

        /// <summary>
        /// 根据当前纸张大小，创建一个新的FixedPage。
        /// </summary>
        public FixedPage GetNewFixedPage()
        {
            Rect rect = this.PageBox;
            FixedPage fixedPage = new FixedPage();
            fixedPage.Width = rect.Width;
            fixedPage.Height = rect.Height;
            fixedPage.ContentBox = rect;
            fixedPage.BleedBox = rect;

            return fixedPage;
        }

        #endregion

    }

    /// <summary>
    /// 纸张样式，如：A3、A4等。
    /// </summary>
    public enum PaperFormat
    {
        A3, A4
    }

    /// <summary>
    /// 页面显示纸张类型。
    /// </summary>
    public enum PaperType
    {
        A3_Landscape,                            ///< A3纸张，横向显示 
        A3_Portrait,                             ///< A3纸张，纵向显示
        A4_Landscape,                            ///< A4纸张，横向显示
        A4_Portrait,                             ///< A4纸张，纵向显示
        Default                                 ///< 上面四种类型中的一种，不同文件可以有不同的默认纸张类型
    }

}

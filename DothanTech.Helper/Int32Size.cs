/// <summary>
/// @file   Int32Size.cs
///	@brief  公共的、通用的整型数的坐标大小类。
/// @author	DothanTech 刘伟宏
/// 
/// Copyright(C) 2011~2014, DothanTech. All rights reserved.
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dothan.Helpers
{
    /// <summary>
    /// 一些编辑器采用格子来放置对象，因此其对象坐标都是整型数，但是 .NET 并
    /// 没有整型数的坐标位置类（其有 Int32Rect 这个整型数矩形区域类），因此
    /// 此处创建一个公共的、通用的整型数的坐标大小类。
    /// </summary>
    public class Int32Size
    {
        #region Life cycle

        public static Int32Size Empty = new Int32Size(0, 0);

        public Int32Size()
        {
        }

        public Int32Size(Int32 width, Int32 height)
        {
            this.Width = width;
            this.Height = height;
        }

        public Int32Size(Int32Size clone)
        {
            this.Width = clone.Width;
            this.Height = clone.Height;
        }

        #endregion

        #region Compare

        public static bool operator !=(Int32Size size1, Int32Size size2)
        {
            if (object.ReferenceEquals(size1, null))
                return !object.ReferenceEquals(size2, null);
            if (object.ReferenceEquals(size2, null))
                return true;

            return size1.Width != size2.Width || size1.Height != size2.Height;
        }

        public static bool operator ==(Int32Size size1, Int32Size size2)
        {
            if (object.ReferenceEquals(size1, null))
                return object.ReferenceEquals(size2, null);
            if (object.ReferenceEquals(size2, null))
                return false;

            return size1.Width == size2.Width && size1.Height == size2.Height;
        }

        public bool Equals(Int32Size value)
        {
            return this == value;
        }

        public override bool Equals(object o)
        {
            return this == (o as Int32Size);
        }

        public static bool Equals(Int32Size size1, Int32Size size2)
        {
            return size1 == size2;
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            return string.Format("({0}*{1})", this.Width, this.Height);
        }

        #endregion

        /// <summary>
        /// 宽度
        /// </summary>
        public Int32 Width;

        /// <summary>
        /// 高度
        /// </summary>
        public Int32 Height;
    }
}

/// <summary>
/// @file   Int32Point.cs
///	@brief  公共的、通用的整型数的坐标位置类。
/// @author	DothanTech 刘伟宏
/// 
/// Copyright(C) 2011~2014, DothanTech. All rights reserved.
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Dothan.Helpers
{
    /// <summary>
    /// 一些编辑器采用格子来放置对象，因此其对象坐标都是整型数，但是 .NET 并
    /// 没有整型数的坐标位置类（其有 Int32Rect 这个整型数矩形区域类），因此
    /// 此处创建一个公共的、通用的整型数的坐标位置类。
    /// </summary>
    public class Int32Point
    {
        #region Life cycle

        /// <summary>
        /// 空对象。
        /// </summary>
        public static Int32Point Empty = new Int32Point(0, 0);

        /// <summary>
        /// Point 没有空对象，这边辅助定义一个。
        /// </summary>
        public static Point EmptyD = new Point(0, 0);

        public Int32Point()
        {
        }

        public Int32Point(Int32 X, Int32 Y)
        {
            this.X = X;
            this.Y = Y;
        }

        public Int32Point(Int32Point clone)
        {
            this.X = clone.X;
            this.Y = clone.Y;
        }

        public Int32Point(Point clone)
        {
            this.X = (int)clone.X;
            this.Y = (int)clone.Y;
        }

        #endregion

        #region Compare

        public static bool operator !=(Int32Point point1, Int32Point point2)
        {
            if (object.ReferenceEquals(point1, null))
                return !object.ReferenceEquals(point2, null);
            if (object.ReferenceEquals(point2, null))
                return true;

            return point1.X != point2.X || point1.Y != point2.Y;
        }

        public static bool operator ==(Int32Point point1, Int32Point point2)
        {
            if (object.ReferenceEquals(point1, null))
                return object.ReferenceEquals(point2, null);
            if (object.ReferenceEquals(point2, null))
                return false;

            return point1.X == point2.X && point1.Y == point2.Y;
        }

        public bool Equals(Int32Point value)
        {
            return this == value;
        }

        public override bool Equals(object o)
        {
            return this == (o as Int32Point);
        }

        public static bool Equals(Int32Point point1, Int32Point point2)
        {
            return point1 == point2;
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            return string.Format("({0}, {1})", this.X, this.Y);
        }

        #endregion

        /// <summary>
        /// 横坐标
        /// </summary>
        public Int32 X;

        /// <summary>
        /// 纵坐标
        /// </summary>
        public Int32 Y;
    }
}

/// <summary>
/// @file   Int32RectExtension.cs
///	@brief  Int32Rect 类的一些扩展函数。
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
    /// Int32Rect 类的一些扩展函数。
    /// </summary>
    public static class Int32RectExtension
    {
        public static Int32Point LeftTop(this Int32Rect This)
        {
            return new Int32Point(This.X, This.Y);
        }

        public static Int32Point Center(this Int32Rect This)
        {
            return new Int32Point(This.X + This.Width / 2, This.Y + This.Height / 2);
        }

        public static bool Contains(this Int32Rect This, Int32Point point)
        {
            return This.Contains(point.X, point.Y);
        }

        public static bool Contains(this Int32Rect This, Point point)
        {
            return This.Contains((int)point.X, (int)point.Y);
        }

        public static bool Contains(this Int32Rect This, Int32 X, Int32 Y)
        {
            return X >= This.X && X < This.X + This.Width &&
                   Y >= This.Y && Y < This.Y + This.Height;
        }

        public static bool Contains(this Int32Rect This, Int32Rect rect)
        {
            return rect.X >= This.X && rect.X + rect.Width <= This.X + This.Width &&
                   rect.Y >= This.Y && rect.Y + rect.Height <= This.Y + This.Height;
        }

        public static void Intersect(this Int32Rect This, Int32Rect rect)
        {
            int l = Math.Max(This.X, rect.X);
            int r = Math.Min(This.X + This.Width, rect.X + rect.Width);
            int t = Math.Max(This.Y, rect.Y);
            int b = Math.Min(This.Y + This.Height, rect.Y + rect.Height);

            if (l >= r || t >= b)
            {
                This.X = This.Y = This.Width = This.Height = 0;
            }
            else
            {
                This.X = l;
                This.Width = r - l;
                This.Y = t;
                This.Height = b - t;
            }
        }

        public static bool IntersectsWith(this Int32Rect This, Int32Rect rect)
        {
            int l = Math.Max(This.X, rect.X);
            int r = Math.Min(This.X + This.Width, rect.X + rect.Width);
            int t = Math.Max(This.Y, rect.Y);
            int b = Math.Min(This.Y + This.Height, rect.Y + rect.Height);

            return (l < r && t < b);
        }

        public static Int32Rect Union(this Int32Rect This, Int32Rect rect)
        {
            if (rect.IsEmpty) return This;

            if (This.IsEmpty)
            {
                return rect;
            }

            int l = Math.Min(This.X, rect.X);
            int r = Math.Max(This.X + This.Width, rect.X + rect.Width);
            int t = Math.Min(This.Y, rect.Y);
            int b = Math.Max(This.Y + This.Height, rect.Y + rect.Height);

            return new Int32Rect(l, t, r - l, b - t);
        }
    }
}

/// <summary>
/// @file   DrawingContextExtension.cs
///	@brief  DrawingContext 类的扩展函数。
/// @author	DothanTech 刘伟宏
/// 
/// Copyright(C) 2011~2014, DothanTech. All rights reserved.
/// </summary>

using System;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Dothan.Helpers
{
    /// <summary>
    /// DrawingContext 类的扩展函数。
    /// </summary>
    public static class DrawingContextExtension
    {
        public static void DrawHorizontalLine(this DrawingContext This, Pen pen, double x1, double x2, double y)
        {
            if (pen != null)
            {
                This.DrawLine(pen, new Point(x1, y + pen.Thickness / 2), new Point(x2, y + pen.Thickness / 2));
            }
        }

        public static void DrawVerticalLine(this DrawingContext This, Pen pen, double x, double y1, double y2)
        {
            if (pen != null)
            {
                This.DrawLine(pen, new Point(x + pen.Thickness / 2, y1), new Point(x + pen.Thickness / 2, y2));
            }
        }

        public static void DrawInsetRectangle(this DrawingContext This, Brush brush, Pen pen, Rect rectangle)
        {
            if (pen == null)
            {
                This.DrawRectangle(brush, null, rectangle);
            }
            else if (pen.Thickness * 2 >= rectangle.Width)
            {
                This.DrawLine(pen, new Point(rectangle.Top, (rectangle.Left + rectangle.Right) / 2),
                                   new Point(rectangle.Bottom, (rectangle.Left + rectangle.Right) / 2));
            }
            else if (pen.Thickness * 2 >= rectangle.Height)
            {
                This.DrawLine(pen, new Point(rectangle.Left, (rectangle.Top + rectangle.Bottom) / 2),
                                   new Point(rectangle.Right, (rectangle.Top + rectangle.Bottom) / 2));
            }
            else
            {
                This.DrawRectangle(brush, pen, new Rect(rectangle.Left + pen.Thickness / 2, rectangle.Top + pen.Thickness / 2,
                                                        rectangle.Width - pen.Thickness, rectangle.Height - pen.Thickness));
            }
        }

        public static void DrawOutsetRectangle(this DrawingContext This, Brush brush, Pen pen, Rect rectangle)
        {
            if (pen == null)
            {
                This.DrawRectangle(brush, null, rectangle);
            }
            else
            {
                This.DrawRectangle(brush, pen, new Rect(rectangle.Left - pen.Thickness / 2, rectangle.Top - pen.Thickness / 2,
                                                        rectangle.Width + pen.Thickness, rectangle.Height + pen.Thickness));
            }
        }

        /// <summary>
        /// 按照给定的位置，对齐方式等参数绘制文本。
        /// </summary>
        public static void DrawText(this DrawingContext This, string text, Typeface fontFace, double fontSize, Brush fontBrush, Point origin)
        {
            FormattedText drawingText = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, fontFace, fontSize, fontBrush);
            This.DrawText(drawingText, origin);
        }

		/// <summary>
		/// 按照给定的显示区域，对齐方式等参数绘制文本。
		/// </summary>
		public static void DrawText(this DrawingContext This, string text, Typeface fontFace, double fontSize, Brush fontBrush, Rect rectangle, HorizontalAlignment horiAlign = HorizontalAlignment.Left, VerticalAlignment vertAlign = VerticalAlignment.Center, bool restrict = true, TextTrimming trimming = TextTrimming.None)
		{
			// 生成绘制用文本对象
            FormattedText drawingText = This.GetFormattedText(text, fontFace, fontSize, fontBrush, rectangle, trimming);
			// 根据对齐方式，确定绘制起始位置
            Point? pt = This.GetDrawingPos(drawingText, rectangle, horiAlign, vertAlign);

            // 最终绘制文本
            if (drawingText != null && pt != null)
			    This.DrawText(drawingText, pt.Value);
		}

        public static FormattedText GetFormattedText(this DrawingContext This, string text, Typeface fontFace, double fontSize, Brush fontBrush, Rect? rc = null, TextTrimming trimming = TextTrimming.None)
        {
            if (string.IsNullOrEmpty(text))
                return null;

            // 生成绘制用文本对象
            FormattedText drawingText = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, fontFace, fontSize, fontBrush);
            // 限制文本的显示区域
            if (rc != null)
            {
                drawingText.MaxTextWidth = rc.Value.Width;
                drawingText.MaxTextHeight = rc.Value.Height;
                drawingText.Trimming = trimming;
            }


            return drawingText;
        }

        public static Point? GetDrawingPos(this DrawingContext This, FormattedText dt, Rect rectangle, HorizontalAlignment horiAlign = HorizontalAlignment.Left, VerticalAlignment vertAlign = VerticalAlignment.Center)
        {
            if (dt == null || rectangle.IsEmpty)
                return null;

            // 根据对齐方式，确定绘制起始位置
            Point pt = new Point();
            switch (horiAlign)
            {
                case HorizontalAlignment.Left:
                default:
                    pt.X = rectangle.Left;
                    break;
                case HorizontalAlignment.Center:
                    pt.X = rectangle.Left + (rectangle.Width - dt.Width) / 2.0;
                    break;
                case HorizontalAlignment.Right:
                    pt.X = dt.Width < rectangle.Width ? rectangle.Left + (rectangle.Width - dt.Width) : rectangle.Left;
                    break;
            }
            //
            switch (vertAlign)
            {
                case VerticalAlignment.Top:
                    pt.Y = rectangle.Top;
                    break;
                case VerticalAlignment.Center:
                default:
                    pt.Y = rectangle.Top + (rectangle.Height - dt.Height) / 2.0;
                    break;
                case VerticalAlignment.Bottom:
                    pt.Y = rectangle.Top + (rectangle.Height - dt.Height);
                    break;
            }

            return pt;
        }

        public static void DrawText(this DrawingContext This, string text, Typeface fontFace, double fontSize, Brush normalFont, Brush highLightFont, Brush highLightBack, int highLightStart, int highLightLength , Point startPos)
        {
            string text1 = null;
            string text2 = null;
            string text3 = null;

            text2 = text.Substring(highLightStart, highLightLength);
            if (highLightStart > 0)
                text1 = text.Substring(0, highLightStart);
            if (highLightStart + highLightLength < text.Length)
                text3 = text.Substring(highLightStart + highLightLength);

            FormattedText dt1 = null;
            FormattedText dt2 = null;
            FormattedText dt3 = null;
            if (!string.IsNullOrEmpty(text1))
                dt1 = new FormattedText(text1, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, fontFace, fontSize, normalFont);
            if (!string.IsNullOrEmpty(text2))
                dt2 = new FormattedText(text2, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, fontFace, fontSize, highLightFont);
            if (!string.IsNullOrEmpty(text3))
                dt3 = new FormattedText(text3, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, fontFace, fontSize, normalFont);

            Point pt1 = startPos, pt2 = startPos, pt3 = startPos;
            if (dt1 != null)
                pt2 = new Point(pt1.X + dt1.Width, pt1.Y);
            if (dt2 != null)
                pt3 = new Point(pt2.X + dt2.Width, pt2.Y);

            // 1、绘制高亮背景色
            if (dt2 != null)
                This.DrawRectangle(highLightBack, null, new Rect(pt2.X, pt2.Y, dt2.Width, dt2.Height));

            // 2、绘制文本字体
            if (dt1 != null)
                This.DrawText(dt1, pt1);
            if (dt2 != null)
                This.DrawText(dt2, pt2);
            if (dt3 != null)
                This.DrawText(dt3, pt3);
        }

        /// <summary>
        /// 计算给定字符串的长度。
        /// </summary>
        public static double MeasureText(this DrawingContext This, string text, Typeface fontFace, double fontSize, Brush fontBrush)
        {
            FormattedText drawingText = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, fontFace, fontSize, fontBrush);
            return drawingText.Width;
        }
    }
}

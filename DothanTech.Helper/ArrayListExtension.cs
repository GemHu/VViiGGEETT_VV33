/// <summary>
/// @file   ArrayListExtension.cs
///	@brief  ArrayList 的扩展接口类。
/// @author	DothanTech 刘伟宏
/// 
/// Copyright(C) 2011~2015, DothanTech. All rights reserved.
/// </summary>

using System;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Dothan.Helpers
{
    /// <summary>
    /// ArrayList 的扩展接口类。
    /// </summary>
    public static class ArrayListExtension
    {
        public static void Sort(this ArrayList This, Comparison<object> comparison)
        {
            This.Sort(new Comparison(comparison));
        }

        public static void Sort(this ArrayList This, int index, int count, Comparison<object> comparison)
        {
            This.Sort(index, count, new Comparison(comparison));
        }

        public static void SortIgnoreCase(this ArrayList This)
        {
            This.Sort(Comparison.IgnoreCaseComparison);
        }
    }

    /// <summary>
    /// 将 Comparison 转化为 IComparer 的辅助类。
    /// </summary>
    public class Comparison : IComparer
    {
        public Comparison(Comparison<object> comparison)
        {
            this.comparison = comparison;
        }

        public int Compare(object x, object y)
        {
            return comparison(x, y);
        }

        protected readonly Comparison<object> comparison;

        /// <summary>
        /// 不区分大小写的字符串比较对象。
        /// </summary>
        public static readonly Comparison IgnoreCaseComparison =
            new Comparison((a, b) =>
            {
                string stra = (a == null ? null : a.ToString());
                string strb = (b == null ? null : b.ToString());
                return string.Compare(stra, strb, true);
            });
    }
}

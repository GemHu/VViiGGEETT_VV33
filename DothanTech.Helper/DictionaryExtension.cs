/// <summary>
/// @file   DictionaryExtension.cs
///	@brief  Dictionary 的扩展接口类。
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
    /// Dictionary 的扩展接口类。
    /// </summary>
    public static class DictionaryExtension
    {
        public static void SortKey<TKey, TValue>(this Dictionary<TKey, TValue> This, Comparison comparison)
        {
            if (This.Count <= 0) return;

            // 将关键字导出成数组，然后进行排序
            ArrayList al = new ArrayList(This.Keys);
            al.Sort(comparison);

            // 生成一个克隆的字典，下面要根据该字典来进行关键字匹配
            Dictionary<TKey, TValue> clone = new Dictionary<TKey, TValue>(This);

            // 清空原来的字典，然后按照排序后的顺序，将元素逐步加入字典
            This.Clear();
            foreach (TKey key in al)
                This[key] = clone[key];
        }

        public static void SortIgnoreCase<TKey, TValue>(this Dictionary<TKey, TValue> This)
        {
            This.SortKey(Comparison.IgnoreCaseComparison);
        }
    }
}

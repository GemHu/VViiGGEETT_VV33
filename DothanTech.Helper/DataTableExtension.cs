/// <summary>
/// @file   DataTableExtension.cs
///	@brief  DataTable 类的扩展函数。
/// @author	DothanTech 刘伟宏
/// 
/// Copyright(C) 2011~2014, DothanTech. All rights reserved.
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Dothan.Helpers
{
    /// <summary>
    /// DataTable 类的扩展函数。
    /// </summary>
    public static class DataTableExtension
    {
        /// <summary>
        /// 比较两 DataTable 的数据是否一样？
        /// </summary>
        /// <param name="table">DataTable 对象</param>
        /// <param name="compareTo">DataTable 对象</param>
        /// <returns>比较结果：0 相等；> 0 表示 table > compareTo；< 0 表示 table < compareTo。</returns>
        public static int CompareTo(this DataTable table, DataTable compareTo)
        {
            try
            {
                if (table == null)
                    return (compareTo == null ? 0 : -1);
                if (compareTo == null)
                    return 1;

                // 行和列的个数比较
                if (table.Columns.Count != compareTo.Columns.Count)
                    return 1;
                if (table.Rows.Count != compareTo.Rows.Count)
                    return 1;

                // 列定义比较
                int colCount = table.Columns.Count;
                for (int i = colCount - 1; i >= 0; --i)
                {
                    DataColumn colThis = table.Columns[i];
                    DataColumn colComp = compareTo.Columns[i];
                    if (colThis.ColumnName != colComp.ColumnName ||
                        colThis.Caption != colComp.Caption ||
                        colThis.DataType != colComp.DataType)
                        return 1;
                }

                // 行值比较
                for (int i = table.Rows.Count - 1; i >= 0; --i)
                {
                    DataRow rowThis = table.Rows[i];
                    DataRow rowComp = compareTo.Rows[i];
                    for (int j = 0; j < colCount; ++j)
                    {
                        if (rowThis[j].ToString() != rowComp[j].ToString())
                            return 1;
                    }
                }

                // 真的相等
                return 0;
            }
            catch (Exception)
            {
                return 1;
            }
        }
    }
}

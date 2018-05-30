/// <summary>
/// @file   ComboBoxExtension.cs
///	@brief  提供 ComboBox 类的扩展函数。
/// @author	DothanTech 刘伟宏
/// 
/// Copyright(C) 2011~2014, DothanTech. All rights reserved.
/// </summary>

using System;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// 提供 ComboBox 类的扩展函数。
    /// </summary>
    public static class ComboBoxExtension
    {
        /// <summary>
        /// 将下拉框选中项目的 Tag 转成指定的枚举值。
        /// </summary>
        public static EnumType GetSelectedTag<EnumType>(this ComboBox combo)
        {
            return (EnumType)Enum.Parse(typeof(EnumType),
                (combo.SelectedItem as FrameworkElement).Tag as string, true);
        }

        /// <summary>
        /// 将下拉框指定项目的 Tag 转成指定的枚举值。
        /// </summary>
        public static EnumType GetItemTag<EnumType>(this ComboBox combo, int index)
        {
            return (EnumType)Enum.Parse(typeof(EnumType),
                (combo.Items[index] as FrameworkElement).Tag as string, true);
        }

        /// <summary>
        /// 得到下拉框选中项目的 Tag 的字符串。
        /// </summary>
        public static string GetSelectedTag(this ComboBox combo)
        {
            return (combo.SelectedItem as FrameworkElement).Tag as string;
        }

        /// <summary>
        /// 得到下拉框指定项目的 Tag 的字符串。
        /// </summary>
        public static string GetItemTag(this ComboBox combo, int index)
        {
            return (combo.Items[index] as FrameworkElement).Tag as string;
        }

        /// <summary>
        /// 根据指定的 Tag，选中下拉框中的项目。
        /// </summary>
        /// <returns>成功与否？</returns>
        public static bool SelectEnum<EnumType>(this ComboBox combo, EnumType value)
        {
            for (int i = 0; i < combo.Items.Count; ++i)
            {
                if (combo.GetItemTag<EnumType>(i).Equals(value))
                {
                    combo.SelectedIndex = i;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 根据指定的字符串 Tag，选中下拉框中的项目。
        /// </summary>
        /// <returns>成功与否？</returns>
        public static bool SelectString(this ComboBox combo, string value)
        {
            for (int i = 0; i < combo.Items.Count; ++i)
            {
                string tag = combo.GetItemTag(i);
                if (tag == null)
                {
                    if (value == null)
                    {
                        combo.SelectedIndex = i;
                        return true;
                    }
                }
                else if (tag.Equals(value, StringComparison.OrdinalIgnoreCase))
                {
                    combo.SelectedIndex = i;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 根据指定的 Tag 对象，选中下拉框中的项目。
        /// </summary>
        /// <returns>成功与否？</returns>
        public static bool SelectObject(this ComboBox combo, object value)
        {
            for (int i = 0; i < combo.Items.Count; ++i)
            {
                if ((combo.Items[i] as ComboBoxItem).Tag == value)
                {
                    combo.SelectedIndex = i;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 得到下拉框选中项目的 Content 的字符串。
        /// </summary>
        public static string GetSelectedContent(this ComboBox combo)
        {
            ComboBoxItem item = combo.SelectedItem as ComboBoxItem;
            if (item == null) return null;

            return item.Content as string;
        }

        /// <summary>
        /// 根据指定的字符串，选中下拉框中的项目。
        /// </summary>
        /// <returns>成功与否？</returns>
        public static bool SelectContent(this ComboBox combo, string value, bool setTextIfNotFound)
        {
            foreach (object _item in combo.Items)
            {
                ComboBoxItem item = _item as ComboBoxItem;
                if (item == null) continue;

                string str = item.Content as string;
                if (str == null)
                {
                    if (value == null)
                    {
                        combo.SelectedItem = item;
                        return true;
                    }
                }
                else if (string.Compare(str, value, true) == 0)
                {
                    combo.SelectedItem = item;
                    return true;
                }
            }

            if (setTextIfNotFound)
                combo.Text = value;

            return false;
        }

        /// <summary>
        /// 克隆一个 ComboBoxItem。
        /// </summary>
        public static ComboBoxItem Clone(this ComboBoxItem item)
        {
            item = ((DependencyObject)item).Clone() as ComboBoxItem;
            if (item == null) return null;
            return item;
        }

        /// <summary>
        /// 克隆一个 ComboBoxItem，同时设置其 DataContext。
        /// </summary>
        public static ComboBoxItem Clone(this ComboBoxItem item, object dataContext)
        {
            item = item.Clone();
            if (item == null) return null;
            item.DataContext = dataContext;
            return item;
        }

        /// <summary>
        /// 克隆一个 ComboBoxItem，同时设置其 DataContext。
        /// </summary>
        public static ComboBoxItem Clone(this ComboBoxItem item, object dataContext, object tag)
        {
            item = item.Clone();
            if (item == null) return null;
            item.DataContext = dataContext;
            item.Tag = tag;
            return item;
        }
    }
}

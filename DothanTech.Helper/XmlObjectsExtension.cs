/// <summary>
/// @file   XmlObjectsExtension.cs
///	@brief  Xml 相关类的一些扩展函数。
/// @author	DothanTech 刘伟宏
/// 
/// Copyright(C) 2011~2014, DothanTech. All rights reserved.
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Collections.ObjectModel;

namespace Dothan.Helpers
{
    /// <summary>
    /// XmlElement 类的扩展函数。
    /// </summary>
    public static class XmlElementExtension
    {
        /// <summary>
        /// 读取 bool 类型的 Attribute。
        /// </summary>
        public static bool GetAttributeBool(this XmlElement This, string name)
        {
            return This.GetAttribute(name).ToBool();
        }

        /// <summary>
        /// 读取 bool 类型的 Attribute。
        /// </summary>
        public static bool GetAttributeBool(this XmlElement This, string name, bool defaultValue)
        {
            return This.GetAttribute(name).ToBool(defaultValue);
        }
    }
}

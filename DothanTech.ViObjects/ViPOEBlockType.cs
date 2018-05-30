/// <summary>
/// @file   ViPOEBlockType.cs
///	@brief  ViGET 编辑器中的 POE 文件定义的功能块类型。
/// @author	DothanTech 刘伟宏
/// 
/// Copyright(C) 2011~2014, DothanTech. All rights reserved.
/// </summary>

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.ObjectModel;

namespace Dothan.ViObject
{
    /// <summary>
    /// ViGET 编辑器中的 POE 文件定义的功能块类型。
    /// </summary>
    public class ViPOEBlockType : ViFirmBlockType
    {
        #region Life cycle

        /// <summary>
        /// 构建对象。
        /// </summary>
        /// <param name="name">功能块名称</param>
        /// <param name="dataType">功能块返回的类型，为 null 表示是 FUNCTION BLOCK。</param>
        public ViPOEBlockType(string name, ViDataType dataType)
            : base(name, dataType)
        {
        }

        #endregion

        #region Parse

        /// <summary>
        /// 从功能块 Prototype 描述字符串中解析出功能块类型。
        /// </summary>
        /// <param name="decl">功能块 Prototype 描述字符串</param>
        /// <returns>解析出的功能块类型。</returns>
        public static ViPOEBlockType Parse(string decl)
        {
            return ViIECPOEFile.GetBlockType(string.Empty, new StringReader(decl));
        }

        #endregion
    }
}

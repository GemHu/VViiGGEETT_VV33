/// <summary>
/// @file   ViType.cs
///	@brief  ViGET 对象的类型定义描述。
/// @author	DothanTech 刘伟宏
/// 
/// Copyright(C) 2011~2014, DothanTech. All rights reserved.
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dothan.ViObject
{
    /// <summary>
    /// ViGET 对象的类型定义描述。
    /// </summary>
    public class ViType : ViNamedObject
    {
        #region Life cycle

        /// <summary>
        /// 构建对象。
        /// </summary>
        public ViType()
        {
        }

        /// <summary>
        /// 构建指定名称的对象。
        /// </summary>
        /// <param name="name">对象名称</param>
        public ViType(string name)
            : base(name)
        {
        }

        /// <summary>
        /// 构建指定名称的对象。
        /// </summary>
        /// <param name="name">对象名称</param>
        /// <param name="type">对象类型</param>
        public ViType(string name, ViType type)
            : base(name, type)
        {
        }

        #endregion
    }
}

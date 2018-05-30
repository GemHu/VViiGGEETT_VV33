/// <summary>
/// @file   ViGuid.cs
///	@brief  ViGET 生成字符串类型的 GUID 的辅助函数。
/// @author	DothanTech 刘伟宏
/// 
/// Copyright(C) 2011~2014, DothanTech. All rights reserved.
/// </summary>

using System;

namespace Dothan.ViObject
{
    /// <summary>
    /// ViGET 生成字符串类型的 GUID 的辅助函数。
    /// </summary>
    public static class ViGuid
    {
        /// <summary>
        /// 生成字符串类型的 GUID，格式为：00000000-0000-0000-0000-000000000000，都是大写。
        /// </summary>
        public static string NewGuid()
        {
            Guid id = Guid.NewGuid();
            string str = id.ToString("D");
            str = str.ToUpper();
            return str;
        }
    }
}

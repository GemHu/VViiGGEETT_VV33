/// <summary>
/// @file   DzLength.cs
///	@brief  长度相关的辅助函数。
/// @author	DothanTech 刘伟宏
/// 
/// Copyright(C) 2011~2014, DothanTech. All rights reserved.
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dothan.Helpers
{
    /// <summary>
    /// 长度相关的辅助函数。
    /// </summary>
    public static class DzLength
    {
        /// <summary>
        /// 将毫米转化为 Px（设备无关长度单位，1 英寸 = 96 Px）
        /// </summary>
        public static double Mm2Px(double mm)
        {
            return mm * 96.0 / 25.4;
        }

        /// <summary>
        /// 将 Px 转化为毫米。
        /// </summary>
        public static double Px2Mm(double px)
        {
            return px * 25.4 / 96.0;
        }
    }
}

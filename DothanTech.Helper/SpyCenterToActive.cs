/// <summary>
/// @file   SpyCenterToActive.cs
///	@brief  监控此后窗口的显示，使得其能根据其 Owner 窗口居中显示。
/// @author	DothanTech 刘伟宏
/// 
/// Copyright(C) 2011~2014, DothanTech. All rights reserved.
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Dothan.Helpers
{
    /// <summary>
    /// 监控此后窗口的显示，使得其能根据其 Owner 窗口居中显示。
    /// </summary>
    public class SpyCenterToActive : IDisposable
    {
        [DllImport("Dz.ODBCSelector.dll", CharSet = CharSet.Ansi)]
        private static extern void DZSpyCenterToActive(int start);

        public SpyCenterToActive()
        {
            try
            {
                DZSpyCenterToActive(1);
            }
            catch { }
        }

        public void Dispose()
        {
            try
            {
                DZSpyCenterToActive(0);
            }
            catch { }
        }
    }
}

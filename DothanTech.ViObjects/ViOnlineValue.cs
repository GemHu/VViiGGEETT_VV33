/// <summary>
/// @file   ViOnlineValue.cs
///	@brief  ViGET 编辑器中在线调试(Online)时的变量值。
/// @author	DothanTech 刘伟宏
/// 
/// Copyright(C) 2011~2014, DothanTech. All rights reserved.
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.ComponentModel;

namespace Dothan.ViObject
{
    /// <summary>
    /// ViGET 编辑器中在线调试(Online)时的变量值。
    /// </summary>
    public class ViOnlineValue : DependencyObject
    {
        #region Life Cycle

        public ViOnlineValue(string crdPath)
        {
            this.CrdPath = crdPath;
        }

        #endregion

        #region CrdPath

        /// <summary>
        /// Online Server 用来全局标识变量的路径/关键字。
        /// </summary>
        public string CrdPath { get; protected set; }

        #endregion

        #region D Value

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(string), typeof(ViOnlineValue),
                                        new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Online 时的变量值。
        /// </summary>
        public string Value
        {
            get
            {
                return (string)GetValue(ValueProperty);
            }
            set
            {
                if (value != null)
                    SetValue(ValueProperty, value);
                else
                    ClearValue(ValueProperty);
            }
        }

        #endregion
    }
}

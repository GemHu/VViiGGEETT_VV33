/// <summary>
/// @file   ViGETProjectInfo.cs
///	@brief  ViGET 工程管理器中工程文件基本信息的接口。
/// @author	DothanTech 刘伟宏
/// 
/// Copyright(C) 2011~2014, DothanTech. All rights reserved.
/// </summary>

using Dothan.ViObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dothan.Manager
{
    /// <summary>
    /// ViGET 工程管理器中工程文件基本信息的接口。
    /// </summary>
    public interface IViGETProjectInfo
    {
        /// <summary>
        /// 工程文件全路径名称 .VAR。
        /// </summary>
        string ProjectFile { get; }

        /// <summary>
        /// 工程名称（不包括路径，不包括 .VAR）。
        /// </summary>
        string ProjectName { get; }

        /// <summary>
        /// 工程文件所在的路径名称，以 \ 结尾。
        /// </summary>
        string ProjectPath { get; }

        /// <summary>
        /// 工程的 ENV 目录。
        /// </summary>
        string ProjectEnvPath { get; }

        /// <summary>
        /// 工程的 GEN 目录。
        /// </summary>
        string ProjectGenPath { get; }

        /// <summary>
        /// 得到指定文件名称的 PCSFile 文件对象。
        /// </summary>
        /// <param name="file">可以为文件名称、关键字，或者 PCSFile 对象。</param>
        /// <returns>指定文件名称的 PCSFile 文件对象。</returns>
        PCSFile GetPCSFile(String file);
    }
}

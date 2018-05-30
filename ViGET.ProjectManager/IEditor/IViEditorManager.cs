/// <summary>
/// @file   IViEditorManager.cs
///	@brief  编辑器相关的通用数据。
/// @author	DothanTech 胡殿兴
/// 
/// Copyright(C) 2011~2014, DothanTech. All rights reserved.
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dothan.ViObject;

namespace DothanTech.ViGET.Manager
{
    /// <summary>
    /// CFC编辑器管理器接口，通过该接口可以获取到其他的CFC编辑器。
    /// </summary>
    public interface IViEditorManager
    {
        /// <summary>
        /// 将给定的编辑器注册到当前Project中。
        /// </summary>
        /// <param name="editor">给定的编辑器</param>
        void RegisterOpenedEditor(IEditorInfo editor);

        /// <summary>
        /// 在编辑器关闭的时候需要将编辑器从 Project 中注销掉。
        /// </summary>
        bool UnRegisterOpenedEditor(IEditorInfo editor);

        /// <summary>
        /// 根据编辑器的类型与编辑器名称获取对应的编辑器。
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <returns>返回的编辑器实例</returns>
        IEditorInfo GetOpenedEditorInfo(string fileName);

        /// <summary>
        /// 获取指定类型的已打开的编辑器列表。
        /// </summary>
        IEnumerable<IEditorInfo> GetOpenedEditorList(EEditorType type);

        //IInvisibleCFC GetInvisibleCFC(string fileName, string cpu);

        /// <summary>
        /// 项目路径。
        /// </summary>
        string ProjectPath { get; }

        /// <summary>
        /// 得到当前编辑器的CPUPOUSorce
        /// </summary>
        /// <returns></returns>
        ViCPUPouSource GetCPUPouSource(string path);

    }

    /// <summary>
    /// 编辑器类型。
    /// </summary>
    public enum EEditorType
    {
        CFCEditor,          ///< CFC编辑器类型
        HWEditor,           ///< 硬件配置器类型
        NetEditor,          ///< 网络配置器类型
    }

    /// <summary>
    /// ProjectManager 所管理的编辑器接口。
    /// </summary>
    public interface IEditorInfo
    {
        /// <summary>
        /// 编辑器的本地文件全路径名称。
        /// </summary>
        string FileName { get; }

        /// <summary>
        /// 编辑器对象名称（不包括路径和后缀的文件名称）
        /// </summary>
        string EditorName { get; }

        /// <summary>
        /// 编辑器内容是否发生了修改。
        /// </summary>
        bool IsDirty { get; }

        /// <summary>
        /// 编辑器类型。
        /// </summary>
        EEditorType EditorType { get; }

    }

}

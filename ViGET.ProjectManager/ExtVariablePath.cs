/// <summary>
/// @file   ExtVariablePath.cs
///	@brief  对外的变量路径管理模块。
/// @author	DothanTech 刘伟宏
/// 
/// Copyright(C) 2011~2015, DothanTech. All rights reserved.
/// </summary>

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DothanTech.ViGET.Manager
{
    /// <summary>
    /// 对外的变量路径管理模块。
    /// </summary>
    public class ExtVariablePath
    {
        public ExtVariablePath(string project, string cpu, string variablePath)
        {
            if (!string.IsNullOrEmpty(project))
                ProjectName = project.Trim();
            if (!string.IsNullOrEmpty(cpu))
                CPUName = cpu.Trim();
            if (!string.IsNullOrEmpty(variablePath))
                VariablePath = variablePath.Trim();
        }

        public ExtVariablePath(ProjectManager project, ViCPUInfo cpu, string variablePath)
        {
            if (project != null)
                ProjectName = project.ProjectName;
            if (cpu != null)
                CPUName = cpu.Name;
            if (!string.IsNullOrEmpty(variablePath))
                VariablePath = variablePath.Trim();
        }

        public ExtVariablePath(string globalPath)
        {
            string[] items = globalPath.Split(Seperator);
            if (items != null && items.Length == 3)
            {
                if (!string.IsNullOrEmpty(items[0]))
                    ProjectName = items[0].Trim();
                if (!string.IsNullOrEmpty(items[1]))
                    CPUName = items[1].Trim();
                if (!string.IsNullOrEmpty(items[2]))
                    VariablePath = items[2].Trim();
            }
        }

        /// <summary>
        /// 工程名称。
        /// </summary>
        public readonly string ProjectName = "";

        /// <summary>
        /// CPU 名称。
        /// </summary>
        public readonly string CPUName = "";

        /// <summary>
        /// 变量内部 Shown Path 名称。
        /// </summary>
        public readonly string VariablePath = "";

        /// <summary>
        /// 变量备注信息。
        /// </summary>
        public readonly string Comment = "";

        /// <summary>
        /// 可用于外部交互的全局路径信息。
        /// </summary>
        public string GlobalPath
        {
            get
            {
                return ProjectName + Seperator + CPUName + Seperator + VariablePath;
            }
        }

        /// <summary>
        /// 是否是有效路径？
        /// </summary>
        public bool IsValid(bool shouldHasCPU)
        {
            if (string.IsNullOrEmpty(ProjectName))
                return false;

            if (string.IsNullOrEmpty(VariablePath))
                return false;

            if (shouldHasCPU &&
                string.IsNullOrEmpty(CPUName))
                return false;

            return true;
        }

        public override string ToString()
        {
            return this.GlobalPath;
        }

        public const char Seperator = '#';
    }
}

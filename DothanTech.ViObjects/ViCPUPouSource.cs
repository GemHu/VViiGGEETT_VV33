/// <summary>
/// @file   ViCPUPouSource.cs
///	@brief  ViGET CPU 的功能块数据来源。
/// @author	DothanTech 刘伟宏
/// 
/// Copyright(C) 2011~2014, DothanTech. All rights reserved.
/// </summary>

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections;
using System.Collections.ObjectModel;

using Dothan.Helpers;

namespace Dothan.ViObject
{
    /// <summary>
    /// ViGET CPU 的功能块数据来源。会处理 CPU 的属性变化事件，当相关属性发生变化时，会自动更新 CPU 的 POU 列表。
    /// </summary>
    public class ViCPUPouSource : ViPouSource
    {
        #region Life cycle

        /// <summary>
        /// 构建对象。
        /// </summary>
        /// <param name="sourceFile">功能块数据来源对应的文件。</param>
        /// <param name="pouAttributes">数据来源中的功能块的特性。</param>
        public ViCPUPouSource(IViPouSourceCPU cpu, ViPouAttributes pouAttributes)
            : base(cpu.ProjectFile, pouAttributes)
        {
            this.cpu = cpu;

            cpu.CPUInfoChanged += cpu_CPUInfoChanged;
        }

        /// <summary>
        /// 搜索时是否递归进入子对象进行搜索？
        /// </summary>
        protected override bool RecursiveSearch { get { return true; } }

        /// <summary>
        /// Pou Source 所属的 CPU。
        /// </summary>
        public IViPouSourceCPU CPU
        {
            get
            {
                return this.cpu;
            }
            set
            {
                this.cpu = value;
                this.SourceFile = value.ProjectFile;
                this.SetDirty();
            }
        }
        protected IViPouSourceCPU cpu = null;

        void cpu_CPUInfoChanged(object sender, ViPouSourceCPU.CPUInfoChangedType changedType)
        {
            // 强制刷新 CPU 信息
            this.CPU = this.cpu;
        }

        /// <summary>
        /// 释放与其它对象之间的弱引用。
        /// </summary>
        public override void Dispose()
        {
            if (this.cpu != null)
            {
                this.cpu.CPUInfoChanged -= cpu_CPUInfoChanged;

                this.cpu = null;
            }

            base.Dispose();
        }

        #endregion

        #region UpdatePOUs

        public override void SetDirty()
        {
            // 刷新POU的时候，强制刷新所有的功能块信息，包括520下的硬件库信息；
            foreach (ViNamedObject item in this.Children)
            {
                if (item is ViPouSource)
                    (item as ViPouSource).SetDirty();
            }

            base.SetDirty();
        }

        /// <summary>
        /// 更新功能块数组列表。
        /// </summary>
        public override void UpdatePOUs()
        {
            // 当CPU为null的时候，有可能当前类还未初始化完毕；
            if (this.CPU == null)
                return;

            this.DeleteAll();

            IniFile iniFile = new IniFile(this.SourceFile);
            string projectPath = FileName.GetFilePath(this.SourceFile);

            int sectionType = 0;    // 0 不关心；2 POE；9 LIBRARY
            iniFile.LoopSections((section) =>
                {
                    if (section.EndsWith("_FUNCTIONBLOCK", StringComparison.OrdinalIgnoreCase) ||
                        section.EndsWith("_FUNCTION", StringComparison.OrdinalIgnoreCase))
                    {
                        sectionType = 2;
                        return true;
                    }
                    if (string.Equals(section, "LIBRARY", StringComparison.OrdinalIgnoreCase))
                    {
                        sectionType = 9;
                        return true;
                    }

                    sectionType = 0;
                    return false;
                }, (key, value) =>
                {
                    if (!string.IsNullOrEmpty(value) && key.IsPrefixIndex("FILE"))
                    {
                        string file, hardwareType;
                        switch (sectionType)
                        {
                            case 2:
                                // 工程本身的功能块
                                {
                                    file = FileName.GetAbsoluteFileName(projectPath, value);
                                    file = FileName.GetNewExtFileName(file, ".POE");
                                    hardwareType = ViGlobal.GetPathHardwareName(projectPath, file);
                                    // 没有硬件类型名称，或者硬件类型名称匹配的情况下，才将功能块加入
                                    if (string.IsNullOrEmpty(hardwareType) ||
                                        string.Equals(hardwareType, this.CPU.HardwareType, StringComparison.OrdinalIgnoreCase))
                                    {
                                        ViPOEBlockType blockType = ViIECPOEFile.GetBlockType(file);
                                        if (blockType != null)
                                        {
                                            blockType.PouSource = this;
                                            this.AddChild(blockType);
                                        }
                                    }
                                }
                                break;
                            case 9:
                                // 工程使用的 ViGET 库工程的功能块
                                {
                                    file = FileName.GetAbsoluteFileName(ViGlobal.ProjODKPath, value);
                                    file = FileName.GetNewExtFileName(file, ".VAR");

                                    // 不能讲把自身作为一个库文件来使用，否则会造成死循环；
                                    if (this.CPU.ProjectFile != null && !this.CPU.ProjectFile.Equals(file, StringComparison.OrdinalIgnoreCase))
                                    {
                                        ViPouSourceCPU cpu = new ViPouSourceCPU()
                                        {
                                            ProjectFile = file,
                                            HardwareType = this.CPU.HardwareType,
                                        };

                                        ViCPUPouSource cpuSource = new ViCPUPouSource(cpu, ViPouAttributes.ProjectLibrary | (this.PouAttributes & ViPouAttributes.Public));
                                        if (cpuSource != null)
                                            this.AddChild(cpuSource);
                                    }
                                }
                                break;
                        }
                    }
                    return true;
                });

            // CPU 安装的硬件库的功能块
            if (!string.IsNullOrEmpty(this.CPU.CPUName))
            {
                string globProt = ViGlobal.GetCPUENVPath(projectPath, this.CPU.CPUName) + "GLOBPROT.INC";
                string infoFile = ViGlobal.GetCPUENVPath(projectPath, this.CPU.CPUName) + this.CPU.HardwareType + ".ini";
                this.AddChild(new ViIncPouSource(globProt, infoFile, ViPouAttributes.CPULibrary | (this.PouAttributes & ViPouAttributes.Public)));
            }

            // ViGET 全局的功能块。对于安装的库工程，就不要这些信息了，否则就重复了
            if (!string.IsNullOrEmpty(this.CPU.CPUName))
            {
                ArrayList alPouSources = ViGlobal.GetPouSources(this.CPU.HardwareType);
                if (alPouSources != null)
                {
                    foreach (ViPouSource source in alPouSources)
                        this.AddChild(source);
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// ViCPUPouSource 对象用来获取 CPU 信息的接口。
    /// </summary>
    public interface IViPouSourceCPU
    {
        /// <summary>
        /// CPU 信息发生变化的事件。
        /// </summary>
        event ViPouSourceCPU.CPUInfoChangedHandler CPUInfoChanged;

        /// <summary>
        /// CPU 所属的工程文件全路径名称。
        /// </summary>
        string ProjectFile { get; }
        /// <summary>
        /// CPU 的名称。
        /// </summary>
        string CPUName { get; }
        /// <summary>
        /// CPU 的硬件类型名称。
        /// </summary>
        string HardwareType { get; }
    }

    /// <summary>
    /// 内部使用的临时 ViPouSourceCPU 对象。
    /// </summary>
    public class ViPouSourceCPU : IViPouSourceCPU
    {
        /// <summary>
        /// CPU 信息发生变化的类型。
        /// </summary>
        [Flags]
        public enum CPUInfoChangedType
        {
            None = 0x00,            ///< 无
            ProjectFile = 0x01,     ///< 工程文件名称
            CPUName = 0x02,         ///< CPU 名称
            HardwareType = 0x04,    ///< CPU 硬件类型
        }

        /// <summary>
        /// CPU 信息发生变化的委托原型。
        /// </summary>
        /// <param name="sender">事件发出者</param>
        /// <param name="changedType">CPU 信息发生变化类型</param>
        public delegate void CPUInfoChangedHandler(object sender, CPUInfoChangedType changedType);

        /// <summary>
        /// CPU 信息发生变化的事件。
        /// </summary>
        public event CPUInfoChangedHandler CPUInfoChanged;

        /// <summary>
        /// CPU 所属的工程文件全路径名称。
        /// </summary>
        public string ProjectFile { get; set; }

        /// <summary>
        /// CPU 的名称。
        /// </summary>
        public string CPUName { get; set; }

        /// <summary>
        /// CPU 的硬件类型名称。
        /// </summary>
        public string HardwareType { get; set; }
    }
}

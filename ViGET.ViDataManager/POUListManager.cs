/// <summary>
/// @file   POUListManager.cs
///	@brief  ViGET 工程的 POU 功能块列表管理器，每个 CPU 都有一个这样的对象，管理该 CPU 下可用的所有功能块。
/// @author	DothanTech 刘伟宏
/// 
/// Copyright(C) 2011~2014, DothanTech. All rights reserved.
/// </summary>

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows;
using System.ComponentModel;
using System.Windows.Threading;

using Dothan.ViObject;

namespace Dothan.Manager
{
    /// <summary>
    /// ViGET 工程的 POU 功能块列表管理器，每个 CPU 都有一个这样的对象，管理该 CPU 下可用的所有功能块。
    /// </summary>
    public class POUListManager : IViPouSourceCPU, IViGETManager
    {
        #region Life cycle

        /// <summary>
        /// 构建对象。
        /// </summary>
        public POUListManager(string projectFile, string cpuName, string hardwareType)
            : this(projectFile, cpuName, hardwareType, Dispatcher.CurrentDispatcher)
        {
        }

        /// <summary>
        /// 构建对象。
        /// </summary>
        public POUListManager(string projectFile, string cpuName, string hardwareType, Dispatcher dispatcher)
        {
            this.projectFile = projectFile;
            this.cpuName = cpuName;
            this.hardwareType = hardwareType;

            this.Dispatcher = dispatcher;

            this.POUs = new ViCPUPouSource(this, ViPouAttributes.Public | ViPouAttributes.ProjectUser);
        }

        #endregion

        #region POUs

        /// <summary>
        /// CPU 的功能块列表信息。
        /// </summary>
        public ViCPUPouSource POUs { get; protected set; }

        #endregion

        #region CPUInfoChanged

        /// <summary>
        /// CPU 信息发生变化的事件。
        /// </summary>
        public event ViPouSourceCPU.CPUInfoChangedHandler CPUInfoChanged;

        /// <summary>
        /// 支持异步方式发出修改通知时间。
        /// </summary>
        /// <param name="addType"></param>
        protected void AddChangedType(ViPouSourceCPU.CPUInfoChangedType addType)
        {
            if (this.changedType == ViPouSourceCPU.CPUInfoChangedType.None)
            {
                this.changedType = addType;
                this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (this.CPUInfoChanged != null)
                            this.CPUInfoChanged.Invoke(this, this.changedType);
                        this.changedType = ViPouSourceCPU.CPUInfoChangedType.None;
                    }), DispatcherPriority.ApplicationIdle);
            }
            else
            {
                this.changedType |= addType;
            }
        }
        protected ViPouSourceCPU.CPUInfoChangedType changedType = ViPouSourceCPU.CPUInfoChangedType.None;

        /// <summary>
        /// 界面线程的 Dispatcher。
        /// </summary>
        protected Dispatcher Dispatcher = null;

        #endregion

        #region ProjectFile

        /// <summary>
        /// CPU 所属的工程文件全路径名称。
        /// </summary>
        public string ProjectFile
        {
            get
            {
                return this.projectFile;
            }
            set
            {
                if (value != this.projectFile)
                {
                    this.projectFile = value;
                    AddChangedType(ViPouSourceCPU.CPUInfoChangedType.ProjectFile);
                }
            }
        }
        protected string projectFile;

        #endregion

        #region CPUName

        /// <summary>
        /// CPU 的名称。
        /// </summary>
        public string CPUName
        {
            get
            {
                return this.cpuName;
            }
            set
            {
                if (value != this.cpuName)
                {
                    this.cpuName = value;
                    AddChangedType(ViPouSourceCPU.CPUInfoChangedType.CPUName);
                }
            }
        }
        protected string cpuName;

        #endregion

        #region HardwareType

        /// <summary>
        /// CPU 的硬件类型名称。
        /// </summary>
        public string HardwareType
        {
            get
            {
                return this.hardwareType;
            }
            set
            {
                if (value != this.hardwareType)
                {
                    this.hardwareType = value;
                    AddChangedType(ViPouSourceCPU.CPUInfoChangedType.HardwareType);
                }
            }
        }
        protected string hardwareType;

        #endregion
    }
}

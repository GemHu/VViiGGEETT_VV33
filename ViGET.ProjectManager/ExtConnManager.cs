/// <summary>
/// @file   ExtConnManager.cs
///	@brief  ViGET 工程的跨 CFC 之间的连线集合管理器，每个 CPU 都有一个这样的对象。
/// @author	DothanTech 胡殿兴
/// 
/// Copyright(C) 2011~2014, DothanTech. All rights reserved.
/// </summary>

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Dothan.ViObject;

namespace DothanTech.ViGET.Manager
{
    /// <summary>
    /// ViGET 工程的跨 CFC 之间的连线集合管理器，每个 CPU 都有一个这样的对象。
    /// </summary>
    public class ExtConnManager
    {
        #region 初始化

        private ViCPUInfo CPU;
        private string ExtConnPOEPath;

        public ExtConnManager(ViCPUInfo cpu)
        {
            this.CPU = cpu;
            string cpuKey = cpu.Key;
            this.ExtConnPOEPath = cpu.CurrProject.ProjectEnvPath + cpuKey + ViNamedObject.PathSeperator + cpuKey + "_ExtCon.POE";
        }

        public ExtConnManager(string makFile)
        {
            if (string.IsNullOrEmpty(makFile) || makFile.LastIndexOf(".") <= 0)
                return;

            this.ExtConnPOEPath = makFile.Substring(0, makFile.LastIndexOf("."));
            this.ExtConnPOEPath += "_ExtCon.POE";
        }

        #endregion

        #region 属性

        #region RegisteredConnector

        /// <summary>
        /// 已注册的管脚变量。
        /// </summary>
        protected IConnector RegisteredConnector;

        #endregion

        /// <summary>
        /// 对变量中的文件名称进行重命名。
        /// </summary>
        public void RenameProgram(string oldFile, string newFile)
        {
            if (this.ExtConnList == null)
                return;

            string oldName = Path.GetFileNameWithoutExtension(oldFile);
            string newName = Path.GetFileNameWithoutExtension(newFile);
            this.ExtConnList.RenameProgram(oldName, newName);
        }

        #region ExtConnList

        /// <summary>
        /// 外部连线变量列表。
        /// </summary>
        protected ViExternalConnections ExtConnList
        {
            get
            {
                if (this._ExtConnList == null)
                {
                    this._ExtConnList = new ViExternalConnections();
                    //if (File.Exists(this.ExtConnPOEPath))
                    //{
                    //    this._ExtConnList.Load(this.ExtConnPOEPath);
                    //}
                    //else
                    //{
                    //    this._ExtConnList.Save(this.ExtConnPOEPath);
                    //    this.CPU.ViCPU.AddFile(CPUFileType.GLOBAL, this.ExtConnPOEPath);
                    //}
                }

                return this._ExtConnList;
            }
        }
        private ViExternalConnections _ExtConnList;

        #endregion

        #endregion

        #region IExtConnManager Members

        /// <summary>
        /// 根据文件名称获取目标编辑器。
        /// </summary>
        /// <param name="planName">文件名称，而非文件全路径。</param>
        /// <param name="forceOpen">如果指定的文件未打开，则是否需要强制打开对应的文件?</param>
        /// <returns>目标CFC编辑器</returns>
        public IEditorInfo GetCfcEditor(string planName, bool forceOpen)
        {
            //IEditorInfo editor = this.CPU.OpenDocument(planName);
            //return editor;
            return null;
        }

        /// <summary>
        /// 注册 CFC 管脚。
        /// </summary>
        /// <param name="connector">注册目标</param>
        /// <param name="doConnect">注册前是否需要进行跨CFC连线？</param>
        public void RegisterConnector(IConnector connector, bool doConnect = true)
        {
            if (doConnect && this.RegisteredConnector != null)
            {
                if (this.RegisteredConnector.CanConnectToExtConnector(connector))
                {
                    this.ConnectToExtConn(connector);
                }
                else if (connector != null && !ViConnType.RetrievePlanName(connector.Path).Equals(ViConnType.RetrievePlanName(this.RegisteredConnector.Path), StringComparison.OrdinalIgnoreCase))
                {
                    string file1 = ViConnType.RetrievePlanName(this.RegisteredConnector.Path);
                    ICfcEditorInfo editor = this.GetCfcEditor(file1, false) as ICfcEditorInfo;
                    editor.DisSelectConnector(this.RegisteredConnector);
                }
            }

            this.RegisteredConnector = connector;
        }

        /// <summary>
        /// 建立CFC间的连线。
        /// </summary>
        /// <param name="connector">链接目标</param>
        /// <returns>夸CFC连接信息。</returns>
        public bool ConnectToExtConn(IConnector connector)
        {
            // source对应的为输出管脚信息
            ExternalConnection extConn = null;
            if (this.RegisteredConnector.IsOutputConn)
                extConn = this.createExtConn(this.RegisteredConnector, connector);
            else
                extConn = this.createExtConn(connector, this.RegisteredConnector);

            if (extConn != null)
            {
                this.RegisteredConnector.ConnectToExtConnector(extConn);
                connector.ConnectToExtConnector(extConn);
                extConn.ValidState = true;

                return true;
            }

            return false;
        }

        /// <summary>
        /// 建立与指定管脚路径的夸CFC连线。
        /// </summary>
        public bool ConnectToExtConn(string path)
        {
            IConnector connector = this.GetExternalConnector(path);
            if (connector == null || this.RegisteredConnector == null)
                return false;
            if (!this.RegisteredConnector.CanConnectToExtConnector(connector))
                return false;

            return this.ConnectToExtConn(connector);
        }

        public bool ConnectToExtConn(ExternalConnection extConn, IConnector connector)
        {
            if (extConn == null || connector == null)
                return false;
            if (this.GetExternalConnection(extConn.UUID) != null)
                return false;

            IConnector targetConn = null;
            if (extConn.SourcePath.Equals(connector.Path))
                targetConn = this.GetExternalConnector(extConn.TargetPath);
            else
                targetConn = this.GetExternalConnector(extConn.SourcePath);

            if (targetConn == null)
                return false;

            if (targetConn.CanConnectToExtConnector(connector))
            {
                this.ExtConnList.AddChild(extConn);
                targetConn.ConnectToExtConnector(extConn);
                return true;
            }

            return false;
        }

        private ExternalConnection createExtConn(IConnector sourceConn, IConnector targetConn)
        {
            if (sourceConn == null || targetConn == null)
                return null;

            ExternalConnection externalConn = ExtConnList.CreateVariable(sourceConn.DataType);
            externalConn.TargetPath = targetConn.Path;
            externalConn.TargetTaskType = targetConn.TaskName;
            externalConn.TargetBlockModes = targetConn.BlockMode;
            externalConn.SourcePath = sourceConn.Path;
            externalConn.SourceTaskType = sourceConn.TaskName;
            externalConn.SourceBlockModes = sourceConn.BlockMode;

            return externalConn;
        }

        /// <summary>
        /// 取消另一个文件中的连接信息。
        /// </summary>
        /// <param name="uuid">夸CFC连线变量ID</param>
        /// <param name="planName">发起请求的文件名称</param>
        public void DisConnectTo(string uuid, string requestFileName)
        {
            if (string.IsNullOrEmpty(requestFileName))
                return;
            ExternalConnection extConn = this.ExtConnList.GetGlobalVariable(uuid) as ExternalConnection;
            if (extConn == null)
                return;

            string targetConnPath = string.Empty;
            string targetFile = string.Empty;
            if (string.Compare(requestFileName, extConn.GetSourceProgramName(), true) == 0)
            {
                targetConnPath = extConn.TargetPath;
                targetFile = extConn.GetTargetProgramName();
            }
            else
            {
                targetConnPath = extConn.SourcePath;
                targetFile = extConn.GetSourceProgramName();
            }

            ICfcEditorInfo targetEditor = this.GetCfcEditor(targetFile, true) as ICfcEditorInfo;
            if (targetEditor != null)
            {
                this.ExtConnList.RemoveGlobalVariable(uuid);
                IConnector targetConn = targetEditor.GetConnector(targetConnPath);
                if (targetConn != null)
                    targetConn.DisConnectToExtConnector(extConn);
            }
            else
            {
                extConn.ValidState = false;
            }
        }

        /// <summary>
        /// 删除指定管脚的所有连线。
        /// </summary>
        public void DisConnectTo(string connPath)
        {
            IConnector connector = this.GetExternalConnector(connPath);
            if (connector != null)
                connector.DisConnectTo();
        }

        /// <summary>
        /// 获取指定管脚路径的外部管脚变量。
        /// </summary>
        public IConnector GetExternalConnector(string connPath)
        {
            if (string.IsNullOrEmpty(connPath))
                return null;

            string cpuName = ViConnType.RetrieveCpuName(connPath);
            string planName = ViConnType.RetrievePlanName(connPath);
            string blockName = ViConnType.RetrieveBlockName(connPath);
            string connName = ViConnType.RetrieveConnectorName(connPath);

            if (string.IsNullOrEmpty(cpuName) || string.IsNullOrEmpty(planName))
                return null;
            if (!this.CPU.Key.Equals(cpuName, StringComparison.OrdinalIgnoreCase))
                return null;
            ICfcEditorInfo editor = this.GetCfcEditor(planName, true) as ICfcEditorInfo;
            if (editor == null)
                return null;

            return editor.GetConnector(connPath);
        }

        /// <summary>
        /// 根据变量ID获取对应的外部链接变量。
        /// </summary>
        public ExternalConnection GetExternalConnection(string uuid)
        {
            return this.ExtConnList.GetGlobalVariable(uuid) as ExternalConnection;
        }

        /// <summary>
        /// 判断夸CFC连接变量是否发生了变化。
        /// </summary>
        public bool IsDirty
        {
            get
            {
                if (this.ExtConnList == null)
                    return false;

                return this.ExtConnList.IsDirty;
            }
        }

        /// <summary>
        /// 保存夸CFC连线变量信息。
        /// </summary>
        public void Save()
        {
            this.ExtConnList.Save(this.ExtConnPOEPath);
        }

        #endregion
    }

}

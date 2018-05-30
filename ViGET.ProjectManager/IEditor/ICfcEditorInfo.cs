using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dothan.ViObject;

namespace DothanTech.ViGET.Manager
{
    public interface ICfcEditorInfo : IEditorInfo
    {
        #region Block

        /// <summary>
        /// 获取指定名称的功能块。
        /// </summary>
        IBlock GetBlock(string blockName);

        #endregion

        #region Connector

        /// <summary>
        /// 获取指定管教路径的管脚接口对象。
        /// </summary>
        IConnector GetConnector(string connPath);

        /// <summary>
        /// 跳转到目标管脚所在的页面的MarginBar上。
        /// </summary>
        /// <param name="localPath">目标管脚路径。</param>
        /// <param name="remotePath">其他页面的管脚路径。如果remotePath为null，则直接跳转到localPath所对应的管脚上，否则跳转到remotePath所关联的MarginBar上。</param>
        void GotoRemoteConnector(string localPath, string remotePath);

        /// <summary>
        /// 去选择指定的管脚。
        /// </summary>
        void DisSelectConnector(IConnector connector);

        /// <summary>
        /// 将该编辑器设为活动文档。
        /// </summary>
        void Active();

        #endregion

        /// <summary>
        /// 跳转到指定的功能块或管脚位置。
        /// </summary>
        void NavigateTo(string pos);
    }

    /// <summary>
    /// 文件支持设置密码的编辑器接口。
    /// </summary>
    public interface IPasswordEditorInfo : ICfcEditorInfo
    {
        /// <summary>
        /// 设置文件密码。
        /// </summary>
        void DoSetPassword();
    }

    public interface IBlock
    {

    }

    /// <summary>
    /// 建立不同文件间管脚连接时，对应管脚需要实现的接口。
    /// </summary>
    public interface IConnector
    {
        /// <summary>
        /// 判断是否可以建立到指定管脚的连接。
        /// </summary>
        bool CanConnectToExtConnector(IConnector connector);

        /// <summary>
        /// 建立到外部管脚的连接。
        /// </summary>
        void ConnectToExtConnector(ExternalConnection extConn);

        /// <summary>
        /// 删除到外部管脚的连接。
        /// </summary>
        void DisConnectToExtConnector(ExternalConnection extConn);

        /// <summary>
        /// 删除当前管脚的所有连线。
        /// </summary>
        void DisConnectTo();

        /// <summary>
        /// 当前管脚的管脚路径。
        /// </summary>
        string Path { get; }

        /// <summary>
        /// 判断当前管脚是否是输入管脚。
        /// </summary>
        bool IsInputConn { get; }

        /// <summary>
        /// 判断当前管脚是否为输出管脚。
        /// </summary>
        bool IsOutputConn { get; }

        /// <summary>
        /// 当前管脚的类型。
        /// </summary>
        ViDataType DataType { get; }

        /// <summary>
        /// 当前管脚的运行时名称，如I1,I2,...T1,T2...等。
        /// </summary>
        string TaskName { get; }

        /// <summary>
        /// 当前管脚所属功能块的当前模式。
        /// </summary>
        string BlockMode { get; }
    }

}

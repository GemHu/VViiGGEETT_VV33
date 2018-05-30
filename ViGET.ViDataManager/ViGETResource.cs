/// <summary>
/// @file   ViGETResource.cs
///	@brief  ViGET 工程管理器中操作编译后资源信息的模块。
/// @author	DothanTech 刘伟宏
/// 
/// Copyright(C) 2011~2014, DothanTech. All rights reserved.
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PADTCRD32Lib;
using Dothan.ViObject;

/// <summary>
/// ViGET 编译后资源相关的命名空间。
/// </summary>
namespace Dothan.Manager.Resource
{
    /// <summary>
    /// 所有 ViGET 编译后资源的基类。
    /// </summary>
    public abstract class ViGETResource : ViNamedObject
    {
        #region Life cycle

        public ViGETResource()
        {
        }

        public ViGETResource(ResourceType type, string crdPath, string shownText)
        {
            this.Type = type;
            this.CrdPath = crdPath;
            this.ShownText = shownText;
        }

        public ViGETResource(IPadtCRDNode node)
        {
            this.CreateResource(node);
        }

        #endregion

        /// <summary>
        /// 资源类型。
        /// </summary>
        public new ResourceType Type { get; protected set; }

        /// <summary>
        /// 资源 CRD 路径。
        /// </summary>
        public string CrdPath { get; set; }

        /// <summary>
        /// 资源显示字符串。
        /// </summary>
        public string ShownText { get; protected set; }

        /// <summary>
        /// 资源关键字。
        /// </summary>
        public string Key
        {
            get { return CrdPath; }
        }

        /// <summary>
        /// 从 CRDNode 的资源信息加载资源对象信息。
        /// </summary>
        /// <param name="node">CRDNode 资源。</param>
        /// <returns>成功与否？</returns>
        public bool CreateResource(IPadtCRDNode node)
        {
            if (node == null) return false;
            int type = node.GetType();
            if (type <= (int)ResourceType.kCrdUndefNode ||
                type >= (int)ResourceType.kCrdResourceCount)
                return false;

            Type = (ResourceType)type;
            CrdPath = node.GetInstancePath();
            ShownText = node.GetInstanceName();
            return (!string.IsNullOrEmpty(CrdPath));
        }

        /// <summary>
        /// 资源的显示路径。
        /// </summary>
        public string ShownPath
        {
            get { return GetShownPath(CrdPath); }
        }

        /// <summary>
        /// 真正显示时的资源名称。
        /// </summary>
        public string ShownName
        {
            get { return string.IsNullOrEmpty(ShownText) ? ShownPath : ShownText; }
            set { ShownText = value; }
        }

        /// <summary>
        /// 得到资源显示路径。
        /// </summary>
        /// <param name="CrdPath">资源的 CRD 路径。</param>
        /// <returns>资源的显示路径。</returns>
        public static string GetShownPath(string CrdPath)
        {
            string cpu = null;
            return GetShownPath(CrdPath, ref cpu);
        }

        /// <summary>
        /// 得到资源显示路径。
        /// </summary>
        /// <param name="CrdPath">资源的 CRD 路径。</param>
        /// <param name="cpu">返回资源所属的 CPU 名称。</param>
        /// <returns>资源的显示路径。</returns>
        public static string GetShownPath(string CrdPath, ref string cpu)
        {
            // remove leading cpu name & TASKS

            // init return task info
            cpu = "";

            // seperate path
            string[] items = CrdPath.Split('.');
            if (items == null || items.Count() <= 2)
                return CrdPath;
            if (string.Compare(items[1], "TASKS", true) != 0)
                return CrdPath;

            // task info
            cpu = items[0];

            // remained shown path info
            string shown = items[2];
            for (int i = 3; i < items.Count(); ++i)
                shown += '.' + items[i];
            return shown;
        }

        /// <summary>
        /// 得到资源所属的 TASK（Program）名称。
        /// </summary>
        /// <param name="CrdPath">资源的 CRD 路径。</param>
        /// <returns>资源所属的 TASK（Program）名称。</returns>
        public static string GetTask(string CrdPath)
        {
            // seperate path
            string[] items = CrdPath.Split('.');
            if (items == null) return "";

            if (items.Count() > 1 &&
                items[1].Equals("TASKS", StringComparison.OrdinalIgnoreCase))
            {
                if (items.Count() > 2)
                    return items[2];
            }
            else
            {
                if (items.Count() > 0)
                    return items[0];
            }
            return "";
        }
    }

    /// <summary>
    /// 资源路径类型。
    /// </summary>
    public enum PathType
    {
        CrdPath,
        ShownPath,
    }

    /// <summary>
    /// 资源路径。
    /// </summary>
    public class CrdPath
    {
        public CrdPath(PathType type, string path)
        {
            this.Type = type;
            this.Path = path;
        }

        public PathType Type { get; protected set; }
        public string Path { get; protected set; }
    }

    /// <summary>
    /// 资源类型。
    /// </summary>
    public enum ResourceType
    {
        kCrdUndefNode = 0,
        kCrdBaseNode,
        kCrdVariableNode,
        kCrdProgramNode,
        kCrdFbNode,
        kCrdFunctionNode,
        kCrdArrayNode,
        kCrdArrayElementArrayNode,
        kCrdArrayElementVariableNode,
        kCrdArrayElementStructNode,
        kCrdStructNode,
        // kCrdFunctionBlockFirmwareNode ,
        // kCrdFunctionFirmwareNode ,
        kCrdTaskNode,
        kCrdExeNode,
        kCrdResourceNode,
        kCrdConfigNode,
        kCrdProjectNode,
        kCrdTuiTaskNode,
        kCrdTaskContainerNode,
        kCrdFbInstanceNode,
        kCrdResourceCount
    }

    /// <summary>
    /// 资源变量类型。
    /// </summary>
    public enum VariableType
    {
        kCbeTypeUndef = 0,
        kCbeTypeBool = 1,
        kCbeTypeByte = 2,
        kCbeTypeWord = 3,
        kCbeTypeDword = 4,
        kCbeTypeUsint = 5,
        kCbeTypeUint = 6,
        kCbeTypeUdint = 7,
        kCbeTypeSint = 8,
        kCbeTypeInt = 9,
        kCbeTypeDint = 10,
        kCbeTypeReal = 11,
        kCbeTypeTime = 12,
        kCbeTypeDate = 13,
        kCbeTypeTod = 14,
        kCbeTypeDt = 15,
        kCbeTypeLword = 16,
        kCbeTypeUlint = 17,
        kCbeTypeLint = 18,
        kCbeTypeLreal = 19,
        kCbeTypeString = 20,
        kCbeTypeEnum = 21,
        kCbeTypeArray = 22,
        kCbeTypeStruct = 23,
        kCbeTypeBit = 24,
        kCbeTypeBcdReal = 25,
        kCbeTypeReferenz = 26,
        kCbeTypeUser = 27,
        kCbeTypeArrayOfArray = 28,
        kCbeTypeArrayOfStruct = 29,
        kCbeTypeStructOfStruct = 30,
        kCbeTypeStructOfArray = 31,
        kCbeTypeInitvalues = 32,
        kCbeTypeFbInstance = 33,
        kCbeTypeInt24 = 34,
        kCbeTypeReal48 = 35,
        kCbeTypeAddress = 36,
        kCbeTypeWString = 37,
        /* ----------------------------*/
        kCbeTypeCount = 40
    }

    /// <summary>
    /// 资源变量路径类型。
    /// </summary>
    public enum VariablePathType
    {
        NotExist,
        CrdPath,
        ShownPath,
    }

    /// <summary>
    /// ViGET 编译后资源的 Task（对应到 Program，Global Variables 等）。
    /// </summary>
    public class ViGETResTask : ViGETResource
    {
        #region field

        public string mkNETDEP = "0";
        //public string mkNAME = "";
        public string mkTYPE = "CYCLIC";
        public string mkINTERRUPT_NAME = "";
        public string mkPRIORITY = "1";
        public string mkTIME = "1";
        public string mkNR = "0";
        public string mkOPTIMIZE = "3";

        #endregion

        #region Life cycle

        public ViGETResTask()
        {
        }

        public ViGETResTask(ResourceType type, string crdPath, string shownText)
            : base(type, crdPath, shownText)
        {
        }

        public ViGETResTask(IPadtCRDNode node)
        {
            this.CreateTask(node);
        }

        #endregion

        /// <summary>
        /// Task 对应的 Program 文件全路径名称。
        /// </summary>
        public string File { get; set; }

        /// <summary>
        /// Task 下包含的功能块列表。
        /// </summary>
        public Dictionary<string, ViGETResBlock> DBlocks =
            new Dictionary<string, ViGETResBlock>();

        /// <summary>
        /// Task 下直接包含的变量列表。
        /// </summary>
        public Dictionary<string, ViGETResVariable> DVariables =
            new Dictionary<string, ViGETResVariable>();

        /// <summary>
        /// 根据 PADT 的 Node 信息，创建 Task。
        /// </summary>
        /// <param name="node">PADT Task Node</param>
        /// <returns>成功与否？</returns>
        public bool CreateTask(IPadtCRDNode node)
        {
            if (!CreateResource(node))
                return false;

            IPadtCRDNodeList children = node.GetChildList() as IPadtCRDNodeList;
            if (children == null || children.GetCount() <= 0)
                return true;

            children.SetToHeadPos();
            while (true)
            {
                node = children.GetNext() as IPadtCRDNode;
                if (node == null) break;

                switch ((ResourceType)node.GetType())
                {
                    case ResourceType.kCrdProgramNode:
                    case ResourceType.kCrdFbNode:
                    case ResourceType.kCrdFunctionNode:
                        ViGETResBlock block = new ViGETResBlock();
                        if (block.CreateBlock(node))
                            DBlocks[block.Key] = block;
                        break;
                    case ResourceType.kCrdVariableNode:
                        ViGETResVariable variable = new ViGETResVariable();
                        if (variable.CreateVariable(node))
                            DVariables[variable.Key] = variable;
                        break;
                }
            }

            return true;
        }
    }

    /// <summary>
    /// ViGET 编译后资源的功能块。
    /// </summary>
    public class ViGETResBlock : ViGETResource
    {
        /// <summary>
        /// 功能块中包含的变量。
        /// </summary>
        public Dictionary<string, ViGETResVariable> DVariables =
            new Dictionary<string, ViGETResVariable>();

        /// <summary>
        /// 根据 PADT Node 信息，创建 Block 信息。
        /// </summary>
        /// <param name="node">PADT Block Node</param>
        /// <returns>成功与否？</returns>
        public bool CreateBlock(IPadtCRDNode node)
        {
            if (!CreateResource(node))
                return false;

            IPadtCRDNodeList children = node.GetChildList() as IPadtCRDNodeList;
            if (children == null || children.GetCount() <= 0)
                return true;

            children.SetToHeadPos();
            while (true)
            {
                node = children.GetNext() as IPadtCRDNode;
                if (node == null) break;

                switch ((ResourceType)node.GetType())
                {
                    case ResourceType.kCrdVariableNode:
                        ViGETResVariable variable = new ViGETResVariable();
                        if (variable.CreateVariable(node))
                            DVariables[variable.Key] = variable;
                        break;
                }
            }

            return true;
        }
    }

    /// <summary>
    /// ViGET 编译后资源的变量。
    /// </summary>
    public class ViGETResVariable : ViGETResource
    {
        /// <summary>
        /// 变量类型。
        /// </summary>
        public VariableType VarType = VariableType.kCbeTypeUndef;

        /// <summary>
        /// 根据 PADT Node 信息，创建变量信息。
        /// </summary>
        /// <param name="node">PADT Variable Node</param>
        /// <returns>成功与否？</returns>
        public bool CreateVariable(IPadtCRDNode node)
        {
            if (!CreateResource(node))
                return false;

            ShownText = node.GetInstanceText();
            VarType = (VariableType)node.GetVarType();

            return true;
        }
    }
}

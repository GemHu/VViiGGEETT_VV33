/// <summary>
/// @file   ViExternalConnections.cs
///	@brief  ViGET 跨文件连线类型文件的管理。
/// @author	DothanTech 刘伟宏
/// 
/// Copyright(C) 2011~2014, DothanTech. All rights reserved.
/// </summary>

using System;
using System.IO;
using System.Diagnostics;
using Dothan.Helpers;
using System.ComponentModel;
using System.Windows;

namespace Dothan.ViObject
{
    /// <summary>
    /// ViGET 跨文件连线类型文件的管理。
    /// </summary>
    public class ViExternalConnections : ViGlobalVariables
    {
        /// <summary>
        /// 文件名称的后缀。
        /// </summary>
        public const string FilePostFix = "_ExtCon";

        /// <summary>
        /// 变量名称的前缀。
        /// </summary>
        public const string VariablePrefix = "ExtCon_";

        public ViExternalConnections()
        {
            this.Children.PropertyChanged += this.OnChildrenPropertyChanged;
        }

        #region Variable ID

        protected int NextVariableID = 1;

        /// <summary>
        /// 创建一个变量。变量名称和UUID会自动生成。
        /// </summary>
        /// <param name="dataType">变量类型</param>
        /// <returns>创建的变量。</returns>
        /// @infor 创建的变量已经被加入到 Children 数组中。
        public ExternalConnection CreateVariable(ViDataType dataType)
        {
            // 生成变量名称
            string name = this.NextVariableName();
            ExternalConnection variable = new ExternalConnection(name, dataType);

            // 生成变量 UUID
            variable.UUID = ViGuid.NewGuid();

            // 将变量加入 Children 数组
            this.AddChild(variable);

            // 大功告成
            return variable;
        }

        /// <summary>
        /// 得到下一个未被使用的变量名称。
        /// </summary>
        /// <returns>下一个未被使用的变量名称。</returns>
        protected string NextVariableName()
        {
            for (; ; ++this.NextVariableID)
            {
                string name = VariablePrefix + this.NextVariableID.ToString();
                if (this.ChildByName(name) == null)
                {
                    ++this.NextVariableID;
                    return name;
                }
            }
        }

        /// <summary>
        /// 根据当前的变量名称情况，更新和完善 NextVariableID 属性。
        /// </summary>
        /// <returns>NextVariableID 是否发生变化？</returns>
        public bool PurifyNextVariableID()
        {
            int oldNextVariableID = this.NextVariableID;

            int maxVariableID = 0, varID;
            foreach (ViNamedObject obj in this.Children)
            {
                ViGlobalVariable variable = obj as ViGlobalVariable;
                if (variable == null) continue;

                if (variable.Name.StartsWith(VariablePrefix, StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(variable.Name.Substring(VariablePrefix.Length), out varID))
                    {
                        if (maxVariableID < varID)
                            maxVariableID = varID;
                    }
                }
            }
            //
            this.NextVariableID = maxVariableID + 1;

            return (oldNextVariableID != this.NextVariableID);
        }

        #endregion

        #region Children

        protected void OnChildrenPropertyChanged(object sender, ViDataChangedEventArgs e)
        {
            this.IsDirty = true;
        }

        /// <summary>
        /// 删除指定的全局变量。
        /// </summary>
        public override bool RemoveGlobalVariable(ViGlobalVariable var)
        {
            return this.DeleteChild(var);
        }

        /// <summary>
        /// 删除指定 UUID 所对应的全局变量。
        /// </summary>
        public override bool RemoveGlobalVariable(string uuid)
        {
            return this.DeleteChild(this.GetGlobalVariable(uuid));
        }

        /// <summary>
        /// 遍历所有子节点，如果满足要求则进行重命名操作。
        /// </summary>
        public void RenameProgram(string oldName, string newName)
        {
            if (string.IsNullOrEmpty(oldName) || string.IsNullOrEmpty(newName))
                return;

            foreach (ExternalConnection item in this.Children)
            {
                item.RenameProgram(oldName, newName);
            }
        }

        #endregion

        #region IsDirty

        public bool IsDirty
        {
            get;
            set;
        }

        #endregion

        #region Load

        /// <summary>
        /// 从指定文件中读取信息。
        /// </summary>
        /// <param name="reader">文件对象</param>
        /// <returns>成功/失败？</returns>
        public override bool Load(TextReader reader)
        {
            base.Load(reader);

            this.PurifyNextVariableID();
            this.IsDirty = false;
            return true;
        }

        protected override bool OnVariableLine(string line)
        {
            int i = line.IndexOf("Next Index:");
            if (i >= 0)
            {
                i = line.IndexOf("(", i);
                int j = line.IndexOf(")", i);
                string index = line.Substring(i + 1, j - i - 1);
                if (j > i)
                {
                    try
                    {
                        this.NextVariableID = int.Parse(index);
                    }
                    catch (System.Exception ee)
                    {
                        Trace.WriteLine("### [" + ee.Source + "] Exception: " + ee.Message);
                        Trace.WriteLine("### " + ee.StackTrace);
                    }
                }
            }
            else
            {
                ExternalConnection connType = ExternalConnection.Parse(line, ViTypeCreation.Create);
                if (connType == null || connType.Type == null)
                    return false;

                this.AddChild(connType);
            }

            return true;
        }

        #endregion

        #region Save

        /// <summary>
        /// 保存变量信息。
        /// </summary>
        /// <param name="writer">文本对象</param>
        protected override void SaveVariables(TextWriter writer)
        {
            this.PurifyNextVariableID();
            writer.WriteLine("{0} Next Index: ({1}) {2}",
                ViTextFile.CommentBegin, this.NextVariableID, ViTextFile.CommentEnd);

            base.SaveVariables(writer);
            this.IsDirty = false;
        }

        #endregion
    }

    public class ExternalConnection : ViGlobalVariable
    {
        #region Life Cycle 

        public ExternalConnection(string name, ViDataType dataType)
            : base(name, dataType)
        {
            this.UUID = Guid.NewGuid().ToString();
        }

        #endregion

        #region 常量字符串

        //const char sHierarchyPathSeparator_g = '\\';
        /// <summary>
        /// 常量字符串："to:"
        /// </summary>
        public const string sTargetPathIdentifier = "to:";
        /// <summary>
        /// 常量字符串："from:"
        /// </summary>
        public const string sSourcePathIdentifier = "from:";
        /// <summary>
        /// 常量字符串："targetTask:"
        /// </summary>
        public const string sTargetTaskTypeIdentifier = "targetTask:";
        /// <summary>
        /// 常量字符串："sourceTask:"
        /// </summary>
        public const string sSourceTaskTypeIdentifier = "sourceTask:";
        /// <summary>
        /// 常量字符串："targetBlockMode:"
        /// </summary>
        public const string sTargetBlockModeIdentifier = "targetBlockMode:";
        /// <summary>
        /// 常量字符串："sourceBlockMode:"
        /// </summary>
        public const string sSourceBlockModeIdentifier = "sourceBlockMode:";
        /// <summary>
        /// 常量字符串："UUID:"
        /// </summary>
        public const string sUuidIdentifier = "UUID:";
        /// <summary>
        /// 常量字符串："(*Invalid "
        /// </summary>
        public const string sInvalidGlobVarPrefix = "(*Invalid ";

        #endregion

        #region 属性

        #region D TargetPath

        public static readonly DependencyProperty TargetPathProperty =
            DependencyProperty.Register("TargetPath", typeof(string), typeof(ExternalConnection),
                                        new FrameworkPropertyMetadata(string.Empty));

        public string TargetPath
        {
            get 
            {
                return this.GetValue(TargetPathProperty) as string;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    this.SetValue(TargetPathProperty, value);
                else
                    this.ClearValue(TargetPathProperty);
            }
        }

        #endregion

        #region D TargetTaskType

        public static readonly DependencyProperty TargetTaskTypeProperty =
            DependencyProperty.Register("TargetTaskType", typeof(string), typeof(ExternalConnection),
                                        new FrameworkPropertyMetadata(string.Empty));

        /// <summary>
        /// 链接的目标管脚信息，通常为输入管脚。
        /// </summary>
        public string TargetTaskType
        {
            get 
            { 
                return (string)this.GetValue(TargetTaskTypeProperty); 
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    this.ClearValue(TargetTaskTypeProperty);
                else
                    this.SetValue(TargetTaskTypeProperty, value);
            }
        }

        #endregion

        #region D TargetBlockModes

        public static readonly DependencyProperty TargetBlockModesProperty =
            DependencyProperty.Register("TargetBlockModes", typeof(string), typeof(ExternalConnection),
                                        new FrameworkPropertyMetadata(string.Empty));

        public string TargetBlockModes
        {
            get 
            { 
                return (string)this.GetValue(TargetBlockModesProperty); 
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    this.ClearValue(TargetBlockModesProperty);
                else
                    this.SetValue(TargetBlockModesProperty, value);
            }
        }

        #endregion

        #region D SourcePath

        public static readonly DependencyProperty SourcePathProperty =
            DependencyProperty.Register("SourcePath", typeof(string), typeof(ExternalConnection),
                                        new FrameworkPropertyMetadata(string.Empty));

        public string SourcePath
        {
            get 
            { 
                return (string)this.GetValue(SourcePathProperty); 
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    this.ClearValue(SourcePathProperty);
                else
                    this.SetValue(SourcePathProperty, value);
            }
        }

        #endregion

        #region D SourceTaskType

        public static readonly DependencyProperty SourceTaskTypeProperty =
            DependencyProperty.Register("SourceTaskType", typeof(string), typeof(ExternalConnection),
                                        new FrameworkPropertyMetadata(string.Empty));

        /// <summary>
        /// 连接的源管脚，通常为输出管脚。
        /// </summary>
        public string SourceTaskType
        {
            get 
            { 
                return (string)this.GetValue(SourceTaskTypeProperty); 
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    this.ClearValue(SourceTaskTypeProperty);
                else
                    this.SetValue(SourceTaskTypeProperty, value);
            }
        }

        #endregion

        #region D SourceBlockModes

        public static readonly DependencyProperty SourceBlockModesProperty =
            DependencyProperty.Register("SourceBlockModes", typeof(string), typeof(ExternalConnection),
                                        new FrameworkPropertyMetadata(string.Empty));

        public string SourceBlockModes
        {
            get 
            { 
                return (string)this.GetValue(SourceBlockModesProperty); 
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    this.ClearValue(SourceBlockModesProperty);
                else
                    this.SetValue(SourceBlockModesProperty, value);
            }
        }

        #endregion

        #region D ValidState

        public static readonly DependencyProperty ValidStateProperty =
            DependencyProperty.Register("ValidState", typeof(bool), typeof(ExternalConnection),
                                        new FrameworkPropertyMetadata(false));

        public bool ValidState
        {
            get 
            { 
                return (bool)this.GetValue(ValidStateProperty); 
            }
            set
            {
                if (value)
                    this.SetValue(ValidStateProperty, value);
                else
                    this.ClearValue(ValidStateProperty);
            }
        }

        #endregion

        public bool IsValid()
        {
            // all values must be set
            bool fParameterValid =
                !string.IsNullOrEmpty(GetSourceConnectorName()) &&
                !string.IsNullOrEmpty(GetSourceBlockName()) &&
                !string.IsNullOrEmpty(GetSourceProgramName()) &&
                //!string.IsNullOrEmpty(SourceTaskType) && 
                !string.IsNullOrEmpty(GetTargetConnectorName()) &&
                !string.IsNullOrEmpty(GetTargetBlockName()) &&
                !string.IsNullOrEmpty(GetTargetProgramName()); 
                //!string.IsNullOrEmpty(TargetTaskType);

            return this.ValidState && fParameterValid;
        }

        #endregion

        #region CFC plan methods

        /// <summary>
        /// 获取 ExternalConnection 源管脚的 Program 名称。
        /// </summary>
        public string GetSourceProgramName()
        {
            return ViConnType.RetrievePlanName(this.SourcePath);
        }

        /// <summary>
        /// 获取 ExternalConnection 目标管脚的 Program 名称。
        /// </summary>
        public string GetTargetProgramName()
        {
            return ViConnType.RetrievePlanName(this.TargetPath);
        }

        /// <summary>
        /// 获取 ExternalConnection 源管脚的  CPU 名称。
        /// </summary>
        /// <returns></returns>
        public string GetSourceResourceName()
        {
            return ViConnType.RetrieveCpuName(this.SourcePath);
        }

        /// <summary>
        /// 获取 ExternalConnection 目标管脚的 CPU 名称。
        /// </summary>
        public string GetTargetResourceName()
        {
            return ViConnType.RetrieveCpuName(this.TargetPath);
        }

        /// <summary>
        /// Replaces the old program name by the new on in the 'from' or 'to' path. 
        /// If the program name does not exist in the path then the method just returns.
        /// </summary>
        public void RenameProgram(string oldName, string newName)
        {
            this.SourcePath = RenameProgramInPath(oldName, newName, this.SourcePath);
            this.TargetPath = RenameProgramInPath(oldName, newName, this.TargetPath);
        }

        /// <summary>
        /// Replaces the old program name by the new on in the 'from' or 'to' path.
        ///	If the resource name does not exist in the path then the method just returns.
        /// </summary>
        public void RenameResource(string newName)
        {
            this.SourcePath = RenameResourceInPath(newName, this.SourcePath);
            this.TargetPath = RenameResourceInPath(newName, this.TargetPath);
        }

        /// <summary>
        /// 对给定管脚路径中的 Program 部分进行重命名操作。
        /// </summary>
        private string RenameProgramInPath(string oldName, string newName, string path)
        {
            if (string.IsNullOrEmpty(oldName) || string.IsNullOrEmpty(newName) || string.IsNullOrEmpty(path))
                return path;
            if (ExternalConnection.RetrievePlanName(path) != oldName)
                return path;
            
            int firstPathSeparator = path.IndexOf(ViNamedObject.PathSeperator);
            int secondPathSeparator = path.IndexOf(ViNamedObject.PathSeperator, firstPathSeparator + 1);

            string programName = path.Substring(firstPathSeparator + 1, secondPathSeparator - firstPathSeparator - 1);
            programName = programName.Replace(oldName, newName);

            return string.Format("{0}{1}{2}", path.Substring(0, firstPathSeparator + 1), programName, path.Substring(secondPathSeparator));
        }

        /// <summary>
        /// 给定路径中的 CPU 部分进行重命名操作。
        /// </summary>
        private string RenameResourceInPath(string newName, string path)
        {
            int firstPathSeparator = path.IndexOf(ViNamedObject.PathSeperator);
            return string.Format("{0}{1}", newName, path.Substring(firstPathSeparator));
        }

        #endregion

        #region block and connector names

        /// <summary>
        /// 获取 ExternalConnection 的源功能块名称。
        /// </summary>
        public string GetSourceBlockName()
        {
            return ViConnType.RetrieveBlockName(this.SourcePath);
        }

        /// <summary>
        /// 获取 ExternalConnection 的目标功能块名称。
        /// </summary>
        public string GetTargetBlockName()
        {
            return ViConnType.RetrieveBlockName(this.TargetPath);
        }

        /// <summary>
        /// 获取 ExternalConnection 的源管脚名称。
        /// </summary>
        public string GetSourceConnectorName()
        {
            return ViConnType.RetrieveConnectorName(this.SourcePath);
        }

        /// <summary>
        /// 获取 ExternalConnection 目标管脚名称。 
        /// </summary>
        public string GetTargetConnectorName()
        {
            return ViConnType.RetrieveConnectorName(this.TargetPath);
        }

        #endregion

        #region Parse

        /// <summary>
        /// 将文本行描述的变量定义，解析为变量对象。
        /// </summary>
        /// <param name="line">变量的文本行描述</param>
        /// <param name="typeCreation">自动创建变量数据类型的方式</param>
        /// <returns>变量对象，null 表示解析失败</returns>
        public static new ExternalConnection Parse(string line, ViTypeCreation typeCreation)
        {
            ExternalConnection variable = ParseExternalConnection(line);
            if (variable == null) return null;

            int targetPathStart = line.IndexOf(sTargetPathIdentifier);
            int sourcePathStart = line.IndexOf(sSourcePathIdentifier);
            int targetTaskStart = line.IndexOf(sTargetTaskTypeIdentifier);
            int sourceTaskStart = line.IndexOf(sSourceTaskTypeIdentifier);
            int targetBlockModeStart = line.IndexOf(sTargetBlockModeIdentifier);
            int sourceBlockModeStart = line.IndexOf(sSourceBlockModeIdentifier);
            int uuidStart = line.IndexOf(sUuidIdentifier);
            int endIndex = line.IndexOf("*)");

            //targetPath
            if (sourcePathStart > targetPathStart + sTargetPathIdentifier.Length)
                variable.TargetPath = line.Substring(targetPathStart + sTargetPathIdentifier.Length, sourcePathStart - targetPathStart - sTargetPathIdentifier.Length).Trim();
            //sourcePath
            if (targetTaskStart > sourcePathStart + sSourcePathIdentifier.Length)
                variable.SourcePath = line.Substring(sourcePathStart + sSourcePathIdentifier.Length, targetTaskStart - sourcePathStart - sSourcePathIdentifier.Length).Trim();
            //targetTask
            if (sourceTaskStart > targetTaskStart + sTargetTaskTypeIdentifier.Length)
                variable.TargetTaskType = line.Substring(targetTaskStart + sTargetTaskTypeIdentifier.Length, sourceTaskStart - targetTaskStart - sTargetTaskTypeIdentifier.Length).Trim();
            //sourceTask
            if (targetBlockModeStart > sourceTaskStart + sSourceTaskTypeIdentifier.Length)
                variable.SourceTaskType = line.Substring(sourceTaskStart + sSourceTaskTypeIdentifier.Length, targetBlockModeStart - sourceTaskStart - sSourceTaskTypeIdentifier.Length).Trim();
            //targetBlockMode
            if (sourceBlockModeStart > targetBlockModeStart + sTargetBlockModeIdentifier.Length)
                variable.TargetBlockModes = line.Substring(targetBlockModeStart + sTargetBlockModeIdentifier.Length, sourceBlockModeStart - targetBlockModeStart - sTargetBlockModeIdentifier.Length).Trim();
            //sourceBlockMode
            if (uuidStart > sourceBlockModeStart + sSourceBlockModeIdentifier.Length)
                variable.SourceBlockModes = line.Substring(sourceBlockModeStart + sSourceBlockModeIdentifier.Length, uuidStart - sourceBlockModeStart - sSourceBlockModeIdentifier.Length).Trim();
            //UUID
            if (endIndex > uuidStart + sUuidIdentifier.Length)
                variable.UUID = line.Substring(uuidStart + sUuidIdentifier.Length, endIndex - uuidStart - sUuidIdentifier.Length).Trim();

            return variable;
        }

        private static ExternalConnection ParseExternalConnection(string line)
        {
            if (string.IsNullOrEmpty(line) || !line.EndsWith("*)"))
                return null;

            string name = string.Empty;
            string dataType = string.Empty;
            ExternalConnection variable = null;

            int nameStart;
            int typeStart;
            int typeEnd;
            if (line.StartsWith(sInvalidGlobVarPrefix))
            {
                //非法
                nameStart = sInvalidGlobVarPrefix.Length;
                typeStart = line.IndexOf(':', nameStart) + 1;
                if (typeStart <= nameStart)
                    return null;
                name = line.Substring(nameStart, typeStart - nameStart - 1);

                typeEnd = line.IndexOf(';', typeStart);
                if (typeEnd <= typeStart)
                    return null;
                dataType = line.Substring(typeStart, typeEnd - typeStart);

                variable = new ExternalConnection(name, ViDataType.GetDataType(dataType));
                variable.ValidState = false;
            }
            else
            {
                // 合法
                nameStart = 0;
                typeStart = line.IndexOf(':') + 1;
                if (typeStart <= nameStart)
                    return null;
                name = line.Substring(nameStart, typeStart - nameStart - 1);

                typeEnd = line.IndexOf(';', typeStart);
                if (typeEnd <= typeStart)
                    return null;
                dataType = line.Substring(typeStart, typeEnd - typeStart);

                variable = new ExternalConnection(name, ViDataType.GetDataType(dataType));
                variable.ValidState = true;
            }

            return variable;
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            return this.ToString(false);
        }

        public override string ToString(bool comment)
        {
            string str = string.Empty;

            if (IsValid())
            {
                str = string.Format("{0}:{1}; (*{2}{3} {4}{5} {6}{7} {8}{9} {10}{11} {12}{13} {14}{15}*)",
                    this.Name, this.Type.Name,
                    sTargetPathIdentifier, this.TargetPath,
                    sSourcePathIdentifier, this.SourcePath,
                    sTargetTaskTypeIdentifier, this.TargetTaskType,
                    sSourceTaskTypeIdentifier, this.SourceTaskType,
                    sTargetBlockModeIdentifier, this.TargetBlockModes,
                    sSourceBlockModeIdentifier, this.SourceBlockModes,
                    sUuidIdentifier, this.UUID);
            }
            else
            {
                str = string.Format("{0}{1}:{2}; {3}{4} {5}{6} {7}{8} {9}{10} {11}{12} {13}{14} {15}{16}*)",
                    sInvalidGlobVarPrefix, this.Name, this.Type.Name,
                    sTargetPathIdentifier, this.TargetPath,
                    sSourcePathIdentifier, this.SourcePath,
                    sTargetTaskTypeIdentifier, this.TargetTaskType,
                    sSourceTaskTypeIdentifier, this.SourceTaskType,
                    sTargetBlockModeIdentifier, this.TargetBlockModes,
                    sSourceBlockModeIdentifier, this.SourceBlockModes,
                    sUuidIdentifier, this.UUID);
            }

            return str;
        }

        #endregion
    }
}

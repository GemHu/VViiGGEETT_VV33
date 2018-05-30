/// <summary>
/// @file   ViGETCPUInfo.cs
///	@brief  ViGET CPU 信息。包含 CPU 信息本身和 CPU 下的 Program/Block/Variable 等。
/// @author	DothanTech 刘伟宏
/// 
/// Copyright(C) 2011~2014, DothanTech. All rights reserved.
/// </summary>

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Dothan.Helpers;
using Dothan.ViObject;
using Dothan.Manager.Resource;

using PADTCRD32Lib;
using System.Xml;

namespace Dothan.Manager
{
    /// <summary>
    /// CPU Status, kick-off build or not according to this.
    /// </summary>
    public enum CPUDataStatus
    {
        Invalid,    // Invalid cpu name
        OutOfDate,  // CPU related file is changed, need build
        Effective,  // CPU's build result is ok, no need build
    }

    /// <summary>
    /// CPU Makefile 中的文件类型。
    /// </summary>
    public enum CPUFileType
    {
        GLOBAL,
        DIRECT_GLOBAL,
        PIV_FILES,
        TASKS,
    }

    //指定从什么操作来更新$ENV$目录下的CPU XML文件
    public enum UpdateENVPathXMLType
    {
        Rename,
        AddFiles,
        DeleteFiles,
        Normal,
    }

    /// <summary>
    /// ViGET CPU 信息。包含 CPU 信息本身和 CPU 下的 Program/Block/Variable 等。
    /// </summary>
    public class ViGETCPUInfo : ViGETResource
    {
        /// <summary>
        /// CPU 所属的工程。
        /// </summary>
        public IViGETProjectInfo Project { get; protected set; }

        /// <summary>
        /// CPU 对应的 Makefile 文件。
        /// </summary>
        public string MakeFile { get; protected set; }

        /// <summary>
        /// CPU 对应的 Setting XML 文件。
        /// </summary>
        public string SettingFile
        {
            get
            {
                return FileName.GetNewExtFileName(this.MakeFile, ".XML");
            }
        }

        /// <summary>
        /// CPU 所属的 Station 名称。
        /// </summary>
        public string Station
        {
            get
            {
                return _station;
            }
            set
            {
                if (!_station.Equals(value))
                {
                    _station = value;

                    IniFile iniFile = new IniFile(this.MakeFile);
                    iniFile.SetValue(makSection, "STATION", _station);
                }
            }
        }
        protected string _station;

        /// <summary>
        /// CPU 的硬件类型。
        /// </summary>
        public string Hardware
        {
            get
            {
                return _hardware;
            }
            set
            {
                _hardware = value;

                IniFile iniFile = new IniFile(this.MakeFile);
                iniFile.SetValue(makSection, "HARDWARE", _hardware);
            }
        }
        protected string _hardware;

        /// <summary>
        /// 当前CPU所对应的 INI文件。
        /// </summary>
        public string HardwareIniFile
        {
            get
            {
                if (string.IsNullOrEmpty(this._hardware))
                    return null;

                string modulesFile = ViGlobal.ProjODKPath + "MODULES\\MODULES.INI";
                return ViGlobal.ProjODKPath + "MODULES\\"
                    + new IniFile(modulesFile).GetValueS("MODULES", _hardware);
            }
        }

        /// <summary>
        /// CPU 的连接类型。
        /// </summary>
        public string Connection
        {
            get
            {
                return _connection;
            }
            set
            {
                _connection = value;

                IniFile iniFile = new IniFile(this.MakeFile);
                iniFile.SetValue(makSection, "CONNECTION", _connection);
            }
        }
        protected string _connection;

        /// <summary>
        /// CPU 使用到的工程文件。
        /// </summary>
        public Dictionary<string, PCSFile> DFiles =
            new Dictionary<string, PCSFile>();
        protected Dictionary<string, CPUFileType> DTypes =
            new Dictionary<string, CPUFileType>();

        /// <summary>
        /// 指定文件在 CPU 中是否被使用？
        /// </summary>
        /// <param name="file">可以为文件名称、关键字，或者 PCSFile 对象。</param>
        /// <returns>指定文件在 CPU 中是否被使用？</returns>
        public bool IsFileExist(object file)
        {
            if (file == null)
                return false;

            string key = null;
            if (file is PCSFile)
                key = (file as PCSFile).Key;
            else
                key = PCSFile.GetFileKey(file.ToString());

            return this.DFiles.ContainsKey(key);
        }

        /// <summary>
        /// 向 CPU 添加使用的文件。
        /// </summary>
        /// <param name="fileType">文件类型。</param>
        /// <param name="fileName">文件全路径名称。</param>
        /// <returns>成功与否？</returns>
        public bool AddFile(CPUFileType fileType, string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return false;

            if (IsFileExist(fileName))
                return false;

            PCSFile pcsFile = this.Project.GetPCSFile(fileName);
            if (pcsFile == null && fileType != CPUFileType.TASKS)
                pcsFile = new PCSFile(FileType.Variable, fileName);
            if (pcsFile == null)
                return false;

            if (fileType == CPUFileType.TASKS)
                this.DTasks[pcsFile.Key] = new ViGETResTask(ResourceType.kCrdTuiTaskNode, pcsFile.Key, pcsFile.Key);
            this.DFiles[pcsFile.Key] = pcsFile;
            this.DTypes[pcsFile.Key] = fileType;
            //
            this.UnloadPCDInfo();

            ViGETVarFile.AddFileInfo(new IniFile(this.MakeFile), fileType.ToString(),
                ViGETVarFile.GetSaveFileName(fileName, this.Project.ProjectPath, false));

            return true;
        }

        /// <summary>
        /// 修改 CPU 使用的文件名称。
        /// </summary>
        /// <param name="oldName">旧的文件名称。</param>
        /// <param name="newName">新的文件名称。</param>
        /// <returns>成功与否？</returns>
        public bool RenameFile(string oldName, string newName)
        {
            if (string.IsNullOrEmpty(oldName) ||
                string.IsNullOrEmpty(newName))
                return false;

            if (!IsFileExist(oldName))
                return false;

            string oldKey = PCSFile.GetFileKey(oldName);
            string newKey = PCSFile.GetFileKey(newName);
            if (oldKey != newKey && IsFileExist(newName))
                return false;

            CPUFileType fileType = this.DTypes[oldKey];
            string oldFile = this.DFiles[oldKey].File;

            PCSFile pcsFile = this.Project.GetPCSFile(newName);
            if (pcsFile == null && fileType != CPUFileType.TASKS)
                pcsFile = new PCSFile(FileType.Variable, newName);
            if (pcsFile == null)
                return false;

            if (oldKey != newKey)
            {
                this.DFiles.Remove(oldKey);
                this.DTypes.Remove(oldKey);
                if (fileType == CPUFileType.TASKS)
                    this.DTasks.Remove(oldKey);
            }

            if (fileType == CPUFileType.TASKS)
                this.DTasks[pcsFile.Key] = new ViGETResTask(ResourceType.kCrdTuiTaskNode, pcsFile.Key, pcsFile.Key);
            this.DFiles[pcsFile.Key] = pcsFile;
            this.DTypes[pcsFile.Key] = fileType;
            //
            this.UnloadPCDInfo();

            ViGETVarFile.RenameFileInfo(new IniFile(this.MakeFile), fileType.ToString(),
                ViGETVarFile.GetSaveFileName(oldFile, this.Project.ProjectPath, false),
                ViGETVarFile.GetSaveFileName(pcsFile.File, this.Project.ProjectPath, false));

            return true;
        }

        /// <summary>
        /// 删除 CPU 使用的文件。
        /// </summary>
        /// <param name="fileName">文件名称。</param>
        /// <returns>成功与否？</returns>
        public bool DeleteFile(string fileName)
        {
            if (!IsFileExist(fileName))
                return false;

            string fileKey = PCSFile.GetFileKey(fileName);
            fileName = this.DFiles[fileKey].File;
            CPUFileType fileType = this.DTypes[fileKey];
            //
            this.DFiles.Remove(fileKey);
            this.DTypes.Remove(fileKey);
            this.DTasks.Remove(fileKey);
            //
            this.UnloadPCDInfo();

            ViGETVarFile.DeleteFileInfo(new IniFile(this.MakeFile), fileType.ToString(),
                ViGETVarFile.GetSaveFileName(fileName, this.Project.ProjectPath, false));

            return true;
        }

        /// <summary>
        /// CPU 使用到的 TASK（Program） 类型工程文件。
        /// </summary>
        public Dictionary<string, ViGETResTask> DTasks =
            new Dictionary<string, ViGETResTask>();

        /// <summary>
        /// CrdPath ==》Variable 的字典。
        /// </summary>
        public Dictionary<string, ViGETResVariable> DCVariables =
            new Dictionary<string, ViGETResVariable>();

        /// <summary>
        /// ShownPath ==》Variable 的字典。
        /// </summary>
        public Dictionary<string, ViGETResVariable> DSVariables
        {
            get
            {
                if (this._DSVariables == null)
                {
                    this._DSVariables = new Dictionary<string, ViGETResVariable>();
                    this.LoadPCDInfo();
                }

                return this._DSVariables;
            }
        }
        private Dictionary<string, ViGETResVariable> _DSVariables;

        /// <summary>
        /// 构建对象。
        /// </summary>
        /// <param name="project">CPU 所属的工程。</param>
        /// <param name="makFile">CPU 对应的 Makefile。</param>
        public ViGETCPUInfo(IViGETProjectInfo project, string makFile)
        {
            this.Type = ResourceType.kCrdExeNode;
            this.CrdPath = PCSFile.GetFileKey(makFile);

            this.Project = project;
            this.MakeFile = makFile;
            this.Name = makFile;
            this.Stage = CPUDataStage.Empty;

            this.LoadMakInfo(makFile);
        }

        public virtual void Rename(string makFile)
        {
            this.CrdPath = PCSFile.GetFileKey(makFile);
            this.MakeFile = makFile;
            this.PCDFile = string.Format(@"{0}{1}\{1}.PCD", this.Project.ProjectGenPath, this.CrdPath);
            this.Name = makFile;
            this.ShownText = this.CrdPath;
        }

        /// <summary>
        /// 得到当前的数据状态。
        /// </summary>
        /// <returns>当前的数据状态。</returns>
        public CPUDataStatus GetDataStatus()
        {
            DateTime pcdTime, itemTime;

            // modify time of pcd file
            try
            {
                pcdTime = File.GetLastWriteTime(PCDFile);
            }
            catch (Exception)
            {
                return CPUDataStatus.OutOfDate;
            }

            // modify time of make file
            try
            {
                itemTime = File.GetLastWriteTime(this.MakeFile);
                if (itemTime >= pcdTime)
                    return CPUDataStatus.OutOfDate;
            }
            catch (Exception)
            {
                return CPUDataStatus.OutOfDate;
            }

            // modify time of setting xml file
            try
            {
                string xmlFile = this.SettingFile;
                itemTime = File.GetLastWriteTime(xmlFile);
                if (itemTime >= pcdTime)
                    return CPUDataStatus.OutOfDate;
            }
            catch (Exception)
            {
                return CPUDataStatus.OutOfDate;
            }

            // modify time of item(s) linked to cpu
            foreach (KeyValuePair<string, PCSFile> kp in DFiles)
            {
                try
                {
                    itemTime = File.GetLastWriteTime(kp.Value.File);
                    if (itemTime >= pcdTime)
                        return CPUDataStatus.OutOfDate;
                }
                catch (Exception)
                {
                    return CPUDataStatus.OutOfDate;
                }
            }

            return CPUDataStatus.Effective;
        }

        /// <summary>
        /// 文件发生改变时由外部调用。
        /// </summary>
        /// <param name="file">发生改变的文件全路径名称。</param>
        public void OnFileChanged(string file)
        {
            if (Stage == CPUDataStage.Base)
                return;

            if (file.Equals(this.PCDFile) ||
                file.Equals(this.MakeFile) ||
                file.Equals(this.SettingFile) ||
                this.DFiles.ContainsKey(PCSFile.GetFileKey(file)))
            {
                Stage = CPUDataStage.Base;
                UnloadPCDInfo();
            }
        }

        /// <summary>
        /// 到数据的稳定状态。
        /// </summary>
        public void ToStableStage()
        {
            if (Stage > CPUDataStage.Base)
                return;

            switch (GetDataStatus())
            {
                case CPUDataStatus.Invalid:
                case CPUDataStatus.OutOfDate:
                default:
                    UnloadPCDInfo();
                    Stage = CPUDataStage.FromMak;
                    break;
                case CPUDataStatus.Effective:
                    if (LoadPCDInfo())
                    {
                        Stage = CPUDataStage.FromPCD;
                    }
                    else
                    {
                        UnloadPCDInfo();
                        Stage = CPUDataStage.FromMak;
                    }
                    break;
            }
        }

        /// <summary>
        /// 得到变量路径的类型。
        /// </summary>
        /// <param name="path">变量路径。</param>
        /// <returns>变量路径类型。</returns>
        public VariablePathType GetVarPathType(string path)
        {
            if (string.IsNullOrEmpty(path))
                return VariablePathType.NotExist;

            // make sure data is to stable stage
            ToStableStage();

            path = path.ToUpper();
            if (DCVariables.ContainsKey(path))
                return VariablePathType.CrdPath;
            if (DSVariables.ContainsKey(path))
                return VariablePathType.ShownPath;

            return VariablePathType.NotExist;
        }

        /// <summary>
        /// CPU 的文件是否包含指定的 TASK？
        /// </summary>
        /// <param name="task">TASK 名称。</param>
        /// <returns>是否包含？</returns>
        public bool IsTaskExist(string task)
        {
            if (string.IsNullOrEmpty(task))
                return false;

            //// make sure data is to stable stage
            //ToStableStage();

            task = PCSFile.GetFileKey(task);
            return DFiles.ContainsKey(task);
        }

        /// <summary>
        /// CPU 的变量是否包含指定的名称？
        /// </summary>
        /// <param name="variable">变量名称。</param>
        /// <returns>是否包含？</returns>
        public bool IsVariableExist(string variable)
        {
            return GetVarPathType(variable) != VariablePathType.NotExist;
        }

        /// <summary>
        /// 将 CPU 中的数据保存到 MakFile 文件。
        /// </summary>
        public void SaveFilesToMakFile(FileType type)
        {
            IniFile makFile = new IniFile(this.MakeFile);
            if (type == FileType.Variable)
            {
                int index = 0;
                string value;
                string section = "PIV_FILES";
                makFile.SetValue(section, "COUNT", index);
                foreach (PCSFile file in DFiles.Values)
                {
                    if (file.Type != type) continue;

                    value = FileName.GetRelativeFileName(this.Project.ProjectPath, file.File);
                    makFile.SetValue(section, "FILE" + index.ToString(), value);

                    // next one
                    index++;
                }
                makFile.DeleteKeys(section, "FILE", index);
                makFile.SetValue(section, "COUNT", index);
            }
            else if (((uint)(type & FileType.PROGRAM)) != 0)
            {
                // save task array info
                int index = 0; string value;
                makFile.SetValue("TASKS", "COUNT", index);
                foreach (ViGETResTask task in this.DTasks.Values)
                {
                    value = FileName.GetRelativeFileName(this.Project.ProjectPath, task.File);
                    value = FileName.GetFilePathWithoutExtension(value);
                    makFile.SetValue("TASKS", "FILE" + index.ToString(), value);

                    string taskSection = "TASK_FILE" + index.ToString();
                    makFile.SetValue(taskSection, "NETDEP", task.mkNETDEP);
                    makFile.SetValue(taskSection, "NAME", task.Name);
                    makFile.SetValue(taskSection, "TYPE", task.mkTYPE);
                    makFile.SetValue(taskSection, "INTERRUPT_NAME", task.mkINTERRUPT_NAME);
                    makFile.SetValue(taskSection, "PRIORITY", task.mkPRIORITY);
                    makFile.SetValue(taskSection, "TIME", task.mkTIME);
                    makFile.SetValue(taskSection, "NR", task.mkNR);
                    makFile.SetValue(taskSection, "OPTIMIZE", task.mkOPTIMIZE);

                    // next one
                    index++;
                }
                makFile.DeleteKeys("TASKS", "FILE", index);
                makFile.SetValue("TASKS", "COUNT", index);
            }
        }

        public enum CPUDataStage
        {
            Empty,          // not read yet
            Base,           // Only base info is ready
            FromMak,        // Task info is read from make file
            FromPCD,        // Task info is read from PCD file
        }

        public CPUDataStage Stage { get; protected set; }

        protected void LoadMakInfo(string makFile)
        {
            IniFile iniFile = new IniFile(makFile);

            this.Stage = CPUDataStage.Base;
            this.ShownText = iniFile.GetValue(makSection, "SHOWNTEXT", PCSFile.GetFileKey(this.MakeFile));
            this._station = iniFile.GetValue(makSection, "STATION", "");
            this._hardware = iniFile.GetValue(makSection, "HARDWARE", "");
            this._connection = iniFile.GetValue(makSection, "CONNECTION", "");

            this.PCDFile = string.Format(@"{0}{1}\{1}.PCD", this.Project.ProjectGenPath, this.CrdPath);

            this.DFiles.Clear();
            this.DTypes.Clear();
            this.DTasks.Clear();
            this.DCVariables.Clear();
            if (this._DSVariables != null)
                this._DSVariables.Clear();

            CPUFileType fileType = CPUFileType.TASKS;
            PCSFile pcsFile; ViGETResTask task;
            iniFile.LoopSections(
                (section) =>
                {
                    if (section.Equals("TASKS", StringComparison.OrdinalIgnoreCase))
                    {
                        fileType = CPUFileType.TASKS;
                    }
                    else if (section.Equals("GLOBAL", StringComparison.OrdinalIgnoreCase))
                    {
                        fileType = CPUFileType.GLOBAL;
                    }
                    else if (section.Equals("DIRECT_GLOBAL", StringComparison.OrdinalIgnoreCase))
                    {
                        fileType = CPUFileType.DIRECT_GLOBAL;
                    }
                    else if (section.Equals("PIV_FILES", StringComparison.OrdinalIgnoreCase))
                    {
                        fileType = CPUFileType.PIV_FILES;
                    }
                    else
                    {
                        return false;
                    }
                    return true;
                },
                (key, value) =>
                {
                    if (key.IsPrefixIndex("FILE") && !string.IsNullOrEmpty(value))
                    {
                        if (fileType != CPUFileType.TASKS)
                        {
                            pcsFile = new PCSFile(FileType.Variable,
                                ViGETVarFile.GetFullFileName(value, this.Project.ProjectPath, ".POE"));
                        }
                        else
                        {
                            pcsFile = this.Project.GetPCSFile(value);
                        }

                        if (pcsFile != null)
                        {
                            key = PCSFile.GetFileKey(value);
                            if (fileType == CPUFileType.TASKS)
                            {
                                task = new ViGETResTask(ResourceType.kCrdTuiTaskNode, key, key);
                                task.File = pcsFile.File;
                                this.DTasks[key] = task;
                            }
                            //
                            this.DFiles[key] = pcsFile;
                            this.DTypes[key] = fileType;
                        }
                    }
                    return true;
                });
        }

        protected bool LoadPCDInfo()
        {
            UnloadPCDInfo();

            try
            {
                if (!File.Exists(PCDFile))
                    return false;

                PadtCodeRepository repository = new PadtCodeRepository();
                if (repository.LoadRepositoryEx(PCDFile, 1) == 0)
                {
                    Trace.WriteLine(string.Format("Load repository from PCD file [{0}] failed!", PCDFile));
                    return false;
                }
                IPadtCRDNode root = repository.GetBaseNode() as IPadtCRDNode;
                if (root == null) return false;

                ResourceType type = (ResourceType)root.GetType();
                if (type != ResourceType.kCrdExeNode)
                {
                    Trace.WriteLine(string.Format("Root node's type is {0}, it should be {1}!", type, ResourceType.kCrdExeNode));
                    return false;
                }

                string path = root.GetInstancePath();
                if (string.Compare(CrdPath, path, true) != 0)
                {
                    Trace.WriteLine(string.Format("CPU name in PCD file is {0}, it should be {1}!", path, CrdPath));
                    return false;
                }

                string shownText = ShownText;
                if (!CreateResource(root))
                    return false;
                CrdPath = path.ToUpper();
                ShownText = shownText;
                CreateTasks(root.GetChildList() as IPadtCRDNodeList);
                CreateDVariables();
            }
            catch (Exception ee)
            {
                Trace.WriteLine("### [" + ee.Source + "] Exception: " + ee.Message);
                Trace.WriteLine("### " + ee.StackTrace);
                return false;
            }

            return true;
        }

        protected void UnloadPCDInfo()
        {
            ArrayList noFileTasks = null;
            foreach (KeyValuePair<string, ViGETResTask> kp in DTasks)
            {
                if (string.IsNullOrEmpty(kp.Value.File))
                {
                    if (noFileTasks == null)
                        noFileTasks = new ArrayList();
                    noFileTasks.Add(kp.Key);
                }
                else
                {
                    kp.Value.DBlocks.Clear();
                }
            }

            if (noFileTasks != null)
            {
                foreach (string key in noFileTasks)
                {
                    DTasks.Remove(key);
                }
            }

            DCVariables.Clear();
            DSVariables.Clear();
        }

        protected void CreateDVariables()
        {
            this.DCVariables.Clear();
            this.DSVariables.Clear();

            foreach (ViGETResTask task in DTasks.Values)
            {
                foreach (ViGETResBlock block in task.DBlocks.Values)
                {
                    foreach (ViGETResVariable variable in block.DVariables.Values)
                    {
                        DCVariables[variable.CrdPath] = variable;
                        DSVariables[variable.ShownPath] = variable;
                    }
                }
                foreach (ViGETResVariable variable in task.DVariables.Values)
                {
                    DCVariables[variable.CrdPath] = variable;
                    DSVariables[variable.ShownPath] = variable;
                }
            }
        }

        protected void CreateTasks(IPadtCRDNodeList nodes)
        {
            if (nodes == null || nodes.GetCount() <= 0)
                return;

            nodes.SetToHeadPos();
            while (true)
            {
                IPadtCRDNode node = nodes.GetNext() as IPadtCRDNode;
                if (node == null) break;

                switch ((ResourceType)node.GetType())
                {
                    case ResourceType.kCrdTaskNode:
                    case ResourceType.kCrdTaskContainerNode:
                        CreateTasks(node.GetChildList() as IPadtCRDNodeList);
                        break;
                    case ResourceType.kCrdTuiTaskNode:
                        ViGETResTask task = new ViGETResTask();
                        if (task.CreateTask(node))
                        {
                            if (DTasks.ContainsKey(task.Key))
                            {
                                DTasks[task.Key].DBlocks = task.DBlocks;
                                DTasks[task.Key].DVariables = task.DVariables;
                            }
                            else
                            {
                                string shownPath = task.ShownPath;
                                if (DTasks.ContainsKey(shownPath))
                                {
                                    ViGETResTask exist = DTasks[shownPath];
                                    exist.CrdPath = task.CrdPath;
                                    exist.DBlocks = task.DBlocks;
                                    exist.DVariables = task.DVariables;
                                }
                                else
                                {
                                    DTasks[task.Key] = task;
                                }
                            }
                        }
                        break;
                }
            }
        }

        const string makSection = "INFORMATION";

        public string PCDFile { get; protected set; }

        //更新$ENV$\CPU\下的xml文件，主要是在重命名、删除或添加文件时不让其xml标签中的排序顺序变混乱
        public void UpdateENVPathXML(string oldName, string newName, UpdateENVPathXMLType enumMode)
        {
            try
            {
                //得到$ENV$\CPU\目录下的xml文件
                string xmlFile = FileName.GetNewExtFileName(this.MakeFile, ".XML");
                if (string.IsNullOrEmpty(xmlFile)) return;

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlFile);
                XmlNode RootNode = xmlDoc.DocumentElement;

                //得到.MAK文件中 [TASKS]-COUNT 值
                IniFile makFile = new IniFile(this.MakeFile);
                int TasksCount = 0;

                switch (enumMode)
                {
                    //重命名CFC文件后遍历所有节点，如果有该CFC文件，就去更新新命名
                    case UpdateENVPathXMLType.Rename:
                        foreach (XmlNode node in RootNode)
                        {
                            if (node.Name.Equals("ProgramExecution", StringComparison.OrdinalIgnoreCase))
                            {
                                foreach (XmlNode childNode in node)
                                {
                                    foreach (XmlNode ProgramNode in childNode)
                                    {
                                        if (ProgramNode.InnerText.Equals(oldName, StringComparison.OrdinalIgnoreCase))
                                        {
                                            ProgramNode.InnerText = newName;
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    //添加文件时也同时更新一下xml文件
                    case UpdateENVPathXMLType.AddFiles:
                        //获取.MAK文件中的任务总数，默认是针对任务T1的，[TASKS]=》COUNT选项
                        if (string.IsNullOrEmpty((makFile.GetValue("TASKS", "COUNT", "")))) return;
                        TasksCount = Convert.ToInt32(makFile.GetValue("TASKS", "COUNT", ""));

                        foreach (XmlNode node in RootNode)
                        {
                            if (node.Name.Equals("ProgramExecution", StringComparison.OrdinalIgnoreCase))
                            {
                                foreach (XmlNode childNode in node)
                                {
                                    //默认添加在T1任务下，新添加一个文件时，自动添加一个<Program>标签，并且TasksCount需要加1
                                    if (childNode.Name.Equals("T1", StringComparison.OrdinalIgnoreCase))
                                    {
                                        childNode.Attributes["count"].Value = (++TasksCount).ToString();

                                        //创建一个新标签元素
                                        XmlElement elem = xmlDoc.CreateElement("Program" + TasksCount);
                                        elem.InnerText = newName;

                                        childNode.AppendChild(elem);
                                    }
                                }
                            }
                        }
                        break;
                    //删除文件时也同时更新一下XML文件
                    case UpdateENVPathXMLType.DeleteFiles:
                        if (string.IsNullOrEmpty((makFile.GetValue("TASKS", "COUNT", "")))) return;
                        TasksCount = Convert.ToInt32(makFile.GetValue("TASKS", "COUNT", ""));

                        foreach (XmlNode node in RootNode)
                        {
                            if (node.Name.Equals("ProgramExecution", StringComparison.OrdinalIgnoreCase))
                            {
                                foreach (XmlNode childNode in node)
                                {
                                    //考虑其他任务中也有可能会有相同的一个CFC文件;
                                    //在删除时需要将其他的任务总数减1,这需要先获取该任务的总数，再去减1;
                                    //如果是在默认的T1任务就可以获得.MAK文件中的[TASKS]=》COUNT选项;
                                    if (childNode.Name.Equals("T1", StringComparison.OrdinalIgnoreCase))
                                    {
                                        TasksCount = Convert.ToInt32(makFile.GetValue("TASKS", "COUNT", ""));
                                    }
                                    else
                                    {
                                        if (Convert.ToInt32(childNode.Attributes["count"].Value) > 0)
                                            TasksCount = Convert.ToInt32(childNode.Attributes["count"].Value) - 1;
                                    }

                                    foreach (XmlNode ProgramNode in childNode)
                                    {
                                        //在所有任务下移除含有删除的CFC文件的标签节点
                                        if (ProgramNode.InnerText.Equals(oldName, StringComparison.OrdinalIgnoreCase))
                                        {
                                            XmlElement xe = (XmlElement)ProgramNode;
                                            xe.ParentNode.Attributes["count"].Value = (TasksCount).ToString();
                                            xe.ParentNode.RemoveChild(xe);
                                        }
                                    }
                                }
                            }
                        }
                        break;
                }

                xmlDoc.Save(xmlFile);
            }
            catch (Exception ee)
            {
                Trace.WriteLine("### [" + ee.Source + "] Exception: " + ee.Message);
                Trace.WriteLine("### " + ee.StackTrace);
            }
        }
    }
}

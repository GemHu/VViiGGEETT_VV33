/// <summary>
/// @file   SharedMemoryManager.cs
///	@brief  ViGET 工程的共享内存变量列表管理器。
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
using System.IO;
using System.Xml;
using System.Windows;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Dothan.ViObject;
using System.Diagnostics;

namespace Dothan.Manager
{
    /// <summary>
    /// ViGET 工程的共享内存变量列表管理器，每个工程有一个这样的对象。
    /// </summary>
    public class SharedMemoryManager : DependencyObject, ISharedMemoryManager, IViGETManager
    {
        /// <summary>
        /// Object info is changed or not.
        /// </summary>
        public bool Dirty { get; set; }

        //the list to manage shared memory variables
        SharedMemoryVariableList ListOfShmVariables;

        //the global list to manage all connected shared memory variables (connections)
        //not connected variables are treated as an empty connection
        SharedMemoryConnectionList ListOfShmConnections;

        private string projectPath = string.Empty;

        public SharedMemoryManager(string projectPath)
        {
            ListOfShmVariables = new SharedMemoryVariableList();
            ListOfShmConnections = new SharedMemoryConnectionList();

            this.projectPath = projectPath;

            ListOfShmVariables.CollectionChanged += value_CollectionChanged;
            ListOfShmConnections.CollectionChanged += value_CollectionChanged;

            // reset dirty flag
            Dirty = false;
        }

        void value_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.Dirty = true;

            if (e.OldItems != null && e.OldItems.Count > 0)
            {
                foreach (var v in e.OldItems)
                {
                    INotifyPropertyChanged npc = v as INotifyPropertyChanged;
                    if (npc != null)
                        npc.PropertyChanged -= npc_PropertyChanged;
                }
            }

            if (e.NewItems != null && e.NewItems.Count > 0)
            {
                foreach (var v in e.NewItems)
                {
                    INotifyPropertyChanged npc = v as INotifyPropertyChanged;
                    if (npc != null)
                        npc.PropertyChanged += npc_PropertyChanged;
                }
            }
        }

        void npc_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.Dirty = true;
        }

        public void Init(XmlNodeList ShmVariableList, ref string msg)
        {
            ListOfShmVariables.Clear();
            ListOfShmConnections.Clear();

            try
            {
                ////call piv importer now. if successful a string message is stored in msg 
                //if (Import(FindAPivFile(), ref ListOfShmVariables, ref  ListOfShmConnections, ref msg))
                //    return;

                foreach (XmlNode node in ShmVariableList)
                {
                    XmlNode subNode = null;
                    string name = node.SelectSingleNode("Name").InnerText;
                    //string variableID = node.SelectSingleNode("VariableID").InnerText;
                    string variableID = (subNode = node.SelectSingleNode("VariableID")) != null ? subNode.InnerText : string.Empty;
                    string dataType = node.SelectSingleNode("DataType").InnerText;
                    string comment = node.SelectSingleNode("Comment").InnerText;
                    bool isFast = bool.Parse(node.SelectSingleNode("IsFast").InnerText);
                    
                    XmlNodeList connectionList = node.SelectNodes("Connections//Connection");

                    SharedMemoryVariable newVariable = new SharedMemoryVariable(name, variableID, dataType, comment, isFast);
                    newVariable.ConnectionCounter = connectionList.Count;

                    ListOfShmVariables.Add(newVariable);

                    //add connection into connection list
                    bool result;
                    foreach (XmlNode connection in connectionList)
                    {
                        SharedMemoryConnection newConnection = new SharedMemoryConnection();
                        newConnection.ConnectorPath = connection.SelectSingleNode("ConnectorPath").InnerText;
                        if (bool.TryParse(connection.SelectSingleNode("IsReadingSignal").InnerText, out result))
                        {
                            newConnection.IsReadingSignal = result;
                        }
                        else
                        {
                            //unconnected variable
                            newConnection.IsReadingSignal = null;
                        }
                        newConnection.TaskName = connection.SelectSingleNode("TaskName").InnerText;
                        newConnection.BlockType = connection.SelectSingleNode("BlockType").InnerText;
                        newConnection.Variable = newVariable;

                        ListOfShmConnections.Add(newConnection);
                    }

                    //add not connected variable into connection list too
                    if (newVariable.ConnectionCounter == 0)
                    {
                        SharedMemoryConnection newConnection = new SharedMemoryConnection();
                        newConnection.ConnectorPath = string.Empty;
                        newConnection.Variable = newVariable;

                        ListOfShmConnections.Add(newConnection);
                    }
                }
            }
            catch (Exception ee)
            {
                Trace.WriteLine("### [" + ee.Source + "] Exception: " + ee.Message);
                Trace.WriteLine("### " + ee.StackTrace);
            }
            finally
            {
                // reset dirty flag
                Dirty = false;
            }
        }

        public void CleanList()
        {
            ListOfShmConnections.Clear();

            foreach (SharedMemoryVariable var in ListOfShmVariables)
            {
                var.Reset();

                //add empty connection entry to the connection list
                SharedMemoryConnection newConnection = new SharedMemoryConnection();
                newConnection.ConnectorPath = string.Empty;
                newConnection.Variable = var;

                ListOfShmConnections.Add(newConnection);
            }
        }

        public bool IsVariablesListValid()
        {
            return ListOfShmVariables.IsValid;
        }

        private void RemoveShmConnection(SharedMemoryConnection item)
        {
            ListOfShmConnections.RemoveConnection(item);
        }

        public void AddShmConnection(string VarName, string ConnectorPath, int IsReadingSignal, string TaskName, string BlockType)
        {
            //lsu 20120417: remove first, otherwise a connection can be created twice caused by compound connector
            RemoveShmConnection(VarName, ConnectorPath);

            SharedMemoryConnection newConnection = new SharedMemoryConnection();
            newConnection.ConnectorPath = ConnectorPath;
            if (IsReadingSignal == 1) //convert from int to nullable bool
            {
                newConnection.IsReadingSignal = true;
            }
            else if (IsReadingSignal == 0)
            {
                newConnection.IsReadingSignal = false;
            }
            else
            {
                newConnection.IsReadingSignal = null;
            }
            newConnection.TaskName = TaskName;
            newConnection.BlockType = BlockType;

            SharedMemoryVariable variable = ListOfShmVariables.GetVariableByName(VarName);
            if (variable != null)
            {
                newConnection.Variable = variable;
                ListOfShmConnections.AddConnection(newConnection);
            }
            else
            {
                //throw new Exception(); //ignore this, otherwise ViGET will crash
            }
        }

        public void RemoveShmConnection(string VarName, string ConnectorPath)
        {
            //lsu 20120410: comparing TargetConnector instead of ConnectorPath. 
            //              After allowing moving block over page border, the ConnectorPath of margin connector may change (page number)
            int iStart = ConnectorPath.LastIndexOf('\\', ConnectorPath.LastIndexOf('\\') - 1);
            string targetConnector = ConnectorPath.Substring(iStart + 1, ConnectorPath.Length - 1 - iStart).Trim();

            //lsu 20120523: comparison must respect plan name, TargetConnector may be not unique in one CPU.
            string planName;
            int indexOfFirstBackslash = ConnectorPath.IndexOf('\\');
            int indexOfSecondBackslash = ConnectorPath.IndexOf('\\', indexOfFirstBackslash + 1);
            bool isNewVersion = ConnectorPath[indexOfSecondBackslash - 1] == ')';

            if (isNewVersion)
            {
                iStart = ConnectorPath.IndexOf('\\');
                int iEnd = ConnectorPath.IndexOf('(', iStart + 1);
                planName = ConnectorPath.Substring(iStart + 1, iEnd - iStart - 2).Trim();
            }
            else
            {
                int iEnd = ConnectorPath.IndexOf('\\');
                planName = ConnectorPath.Substring(0, iEnd).Trim();
            }

            var listOfConnectionsToBeRemoved = ListOfShmConnections.Where(c => c.Variable.Name.Equals(VarName, StringComparison.OrdinalIgnoreCase) &&
                                                                               c.ConnectorPath.Equals(ConnectorPath, StringComparison.OrdinalIgnoreCase)).ToList();
            foreach (var item in listOfConnectionsToBeRemoved)
            {
                RemoveShmConnection(item);
            }
        }

        public void RemoveShmConnections(string planName)
        {
            var listOfConnectionsToBeRemoved = ListOfShmConnections.Where(c => c.PlanName.Equals(planName, StringComparison.OrdinalIgnoreCase)).ToList();
            foreach (var item in listOfConnectionsToBeRemoved)
            {
                RemoveShmConnection(item);
            }
        }

        public void GetShmConnectionList(ref SharedMemoryConnectionList list)
        {
            list = ListOfShmConnections;
        }

        public void GetShmVariableList(ref SharedMemoryVariableList list)
        {
            list = ListOfShmVariables;
        }

        public List<SharedMemoryVariable> GetShmVariableList2()
        {
            List<SharedMemoryVariable> list = new List<SharedMemoryVariable>();

            foreach (SharedMemoryVariable v in ListOfShmVariables)
            {
                list.Add(v);
            }

            list.Sort();

            return list;
        }

        public bool IsAnyWriterInCPU(string sSharedMemoryVariable, string sCPUName)
        {
            return ListOfShmConnections.Where(c => c.Variable.Name.Equals(sSharedMemoryVariable, StringComparison.OrdinalIgnoreCase) &&
                                                   c.CPUName.Equals(sCPUName, StringComparison.OrdinalIgnoreCase) &&
                                                   c.IsReadingSignal == false).ToList().Count > 0;
        }

        public bool IsAnyReaderInCPU(string sSharedMemoryVariable, string sCPUName)
        {
            return ListOfShmConnections.Where(c => c.Variable.Name.Equals(sSharedMemoryVariable, StringComparison.OrdinalIgnoreCase) &&
                                                   c.CPUName.Equals(sCPUName, StringComparison.OrdinalIgnoreCase) &&
                                                   c.IsReadingSignal == true).ToList().Count > 0;
        }

        public string GetSharedMemoryWriter(string sSharedMemoryVariable)
        {
            var list = ListOfShmConnections.Where(c => c.Variable.Name.Equals(sSharedMemoryVariable, StringComparison.OrdinalIgnoreCase)).ToList();
            foreach (SharedMemoryConnection item in list)
            {
                if (item.IsReadingSignal == false)
                {
                    return item.ConnectorPath;
                }
            }

            return string.Empty;
        }

        public string GetSharedMemoryReader(string sSharedMemoryVariable)
        {
            string ret = string.Empty;
            var list = ListOfShmConnections.Where(c => c.Variable.Name.Equals(sSharedMemoryVariable, StringComparison.OrdinalIgnoreCase)).ToList();
            foreach (SharedMemoryConnection item in list)
            {
                if (item.IsReadingSignal == true)
                {
                    ret += item.ConnectorPath + "|";
                }
            }
            return ret.TrimEnd(new char[] { '|' });
        }

        public string GetSharedMemoryVariableList()
        {
            string ret = string.Empty;
            foreach (SharedMemoryVariable item in ListOfShmVariables)
            {
                ret += item.ToString() + "|";
            }
            return ret.TrimEnd(new char[] { '|' });
        }

        public SharedMemoryVariable GetShmVar(string variableID)
        {
            return this.ListOfShmVariables.Where((var) => var.VariableID == variableID).FirstOrDefault();
        }

        public bool IsFastSignal(string VarName)
        {
            SharedMemoryVariable variable = ListOfShmVariables.GetVariableByName(VarName);
            if( variable == null ) return false;
            return variable.IsFast;
        }

        public bool IsConnectable(string VarName)
        {
            return string.IsNullOrEmpty(GetSharedMemoryWriter(VarName));
        }

        public void AddNewShmVarWithDefaultValue()
        {
            ListOfShmVariables.AddNewShmVarWithDefaultValue();

            SharedMemoryConnection newConnection = new SharedMemoryConnection();
            newConnection.ConnectorPath = string.Empty;
            newConnection.Variable = ListOfShmVariables.Last();
            ListOfShmConnections.Add(newConnection);
        }

        public void RemoveShmVar(string VarName)
        {
            ListOfShmVariables.RemoveShmVar(VarName);
            ListOfShmConnections.RemoveEmptyConnection(VarName);
        }

        // <<<>>>
        //private string FindAPivFile()
        //{
            //DirectoryInfo wDir = new DirectoryInfo(projectPath);
            //FileInfo[] makFiles = wDir.GetFiles("*.mak", SearchOption.AllDirectories);

            //foreach (var makeFile in makFiles)
            //{
            //    string file = string.Empty;
            //    IniParser makParser = new IniParser(makeFile.FullName);
            //    file = makParser.GetSetting("PIV_FILES", "FILE0");
            //    if (file != null && file.Length > 0) //file can be null -> must be checked or be inside of try/catch!
            //    {
            //        if (file[0] == '\\')
            //        {
            //            //a leading backslash must be removed, otherwise Path.Combine() only returns the "file" string
            //            file = file.Substring(1);
            //        }
            //        return Path.Combine(projectPath, file);
            //    }
            //}
        //    return string.Empty;
        //}

        // <<<>>>
        //private void DeleteAllPivFileSectionsInMakFiles()
        //{
            //DirectoryInfo wDir = new DirectoryInfo(projectPath);
            //FileInfo[] makFiles = wDir.GetFiles("*.mak", SearchOption.AllDirectories);

            //foreach (var makeFile in makFiles)
            //{
            //    string file = string.Empty;
            //    IniParser makParser = new IniParser(makeFile.FullName);
            //    makParser.DeleteSection("PIV_FILES");
            //    makParser.SaveSettings();
            //}
        //}

        //piv shared memory variable importer, <<<>>>
        //public bool Import(string pivFile, ref SharedMemoryVariableList importedListOfVariables, ref SharedMemoryConnectionList importedListOfConnections, ref string msg)
        //{
            //try
            //{
            //    PIVSerialize pivSer = new PIVSerialize();
            //    if (pivSer.LoadFile(pivFile))
            //    {

            //        long nVarCount = pivSer.GetVariablesCount();
            //        for (int i = 1; i <= nVarCount; ++i)
            //        {
            //            string[] arrayOpc = new string[4];

            //            //get the name and data type
            //            string strNameAndType = pivSer.GetOpenPCSByName(i);

            //            //split the name and data type and comment
            //            arrayOpc = strNameAndType.Split('|');
            //            string strName = arrayOpc[0];
            //            string strType = arrayOpc[1];
            //            string strComment = pivSer.GetCommentInfo(strName);

            //            //get the connector path
            //            string strAll = pivSer.GetDataConsistencyInfo(strName);

            //            //get "Fast Mode"
            //            bool isFastMode = pivSer.IsFastSignal(strName);

            //            SharedMemoryVariable newVariable = new SharedMemoryVariable(strName, strType, strComment, isFastMode);
            //            importedListOfVariables.Add(newVariable);

            //            if (strAll.Length > 0)
            //            {
            //                int nCount = 0;
            //                int nStartIndex = 0;

            //                //split the connector path by "$"
            //                while (nStartIndex != -1)
            //                {
            //                    nStartIndex = strAll.IndexOf("$", nStartIndex + 1);
            //                    ++nCount;
            //                }
            //                string[] arrayPath = new string[nCount];
            //                arrayPath = strAll.Split('$');

            //                //parse all paths and add the variable to the list
            //                foreach (string tmpPath in arrayPath)
            //                {
            //                    SharedMemoryConnection newConnection = new SharedMemoryConnection();

            //                    int nTaskStart = tmpPath.IndexOf("Task:");
            //                    int nPathStart = tmpPath.IndexOf("ConnectorPath:");
            //                    int nBlockTypeStart = tmpPath.IndexOf("BlockType:");
            //                    int nAccessStart = tmpPath.IndexOf("Access:");
            //                    int nConsistentStart = tmpPath.IndexOf(" ", nAccessStart + 1);

            //                    if (nBlockTypeStart == -1)
            //                    {
            //                        nBlockTypeStart = nAccessStart;
            //                    }
            //                    if (nConsistentStart == -1)
            //                    {
            //                        nConsistentStart = tmpPath.Length;
            //                    }

            //                    newConnection.TaskName = tmpPath.Substring(nTaskStart + 5, nPathStart - nTaskStart - 6);
            //                    newConnection.ConnectorPath = tmpPath.Substring(nPathStart + 14, nBlockTypeStart - nPathStart - 15);
            //                    if (nBlockTypeStart < nAccessStart)
            //                    {
            //                        newConnection.BlockType = tmpPath.Substring(nBlockTypeStart + 10, nAccessStart - nBlockTypeStart - 11);
            //                    }
            //                    else
            //                    {
            //                        newConnection.BlockType = "";
            //                    }

            //                    string strAccess = tmpPath.Substring(nAccessStart + 7, nConsistentStart - nAccessStart - 7);
            //                    if (strAccess.Equals("READ", StringComparison.OrdinalIgnoreCase))
            //                    {
            //                        newConnection.IsReadingSignal = true;
            //                    }
            //                    else if (strAccess.Equals("WRITE", StringComparison.OrdinalIgnoreCase))
            //                    {
            //                        newConnection.IsReadingSignal = false;
            //                    }

            //                    newConnection.Variable = newVariable;
            //                    newConnection.Variable.ConnectionCounter++;
            //                    importedListOfConnections.Add(newConnection);
            //                }
            //            }
            //            else
            //            {
            //                SharedMemoryConnection newConnection = new SharedMemoryConnection();

            //                newConnection.ConnectorPath = "";
            //                newConnection.Variable = newVariable;
            //                importedListOfConnections.Add(newConnection);
            //            }
            //        }

            //        File.Move(pivFile, pivFile + ".bak");
            //        msg = "A shared memory variable database from a previous version of ViGET V2.0 was imported.\n\nFile " + Path.GetFileName(pivFile) + " was removed from all resources in the project and renamed to " + (Path.GetFileName(pivFile) + ".bak") + " as a backup.\n\nPlease use \"View/Shared Memory\" to show the new shared memory variables editor.";

            //        //in case piv file is imported successfully, delete all [PIV_FILES] sections in all mak-files
            //        DeleteAllPivFileSectionsInMakFiles();
            //        return true;
            //    }
            //    else
            //        return false;
            //}
            //catch (Exception)
        //    {
        //        return false;
        //    }
        //}

        public void UpdateRuntimeInformation(string blockName, string planName, string runtimeTask)
        {
            foreach (var item in ListOfShmConnections)
            {
                if (item.BlockName.Equals(blockName, StringComparison.OrdinalIgnoreCase) &&
                    item.PlanName.Equals(planName, StringComparison.OrdinalIgnoreCase))
                {
                    item.TaskName = runtimeTask;

                    // set dirty flag
                    Dirty = true;
                }
            }
        }

        public string SHMUsageValidityCheck()
        {
            string ret = "";
            foreach (var item in ListOfShmVariables)
            {
                //verify if there is variable written more than once (is already forbidden in CFC)
                var listOfWriter = ListOfShmConnections.Where(c => c.Variable.Name.Equals(item.Name, StringComparison.OrdinalIgnoreCase) &&
                                                                   c.IsReadingSignal == false).ToList();
                if (listOfWriter.Count > 1)
                    ret += ("Shared memory variable " + item.Name + " is written to more than once. Only one write access is allowed per variable in the entire application.\n");

                //verify if there is writer and reader from the same CPU (is already forbidden in CFC)
                var listOfReader = ListOfShmConnections.Where(c => c.Variable.Name.Equals(item.Name, StringComparison.OrdinalIgnoreCase) &&
                                                                   c.IsReadingSignal == true).ToList();
                foreach (var writer in listOfWriter)
                {
                    if (listOfReader.Where(c => c.CPUName.Length > 0 && c.CPUName.Equals(writer.CPUName, StringComparison.OrdinalIgnoreCase)).ToList().Count > 0)
                        ret += ("Shared memory variable " + writer.Variable.Name + " is read from and written to within CPU: " + writer.CPUName + ". One CPU may only read OR write a shared memory variable. Use a CPU-local connection instead.\n");
                }

                //interrupt read/write of a non-fast shared memory variable - error!
                var listOfConsistentInterruptConnections = ListOfShmConnections.Where(c => c.Variable.Name.Equals(item.Name, StringComparison.OrdinalIgnoreCase) && !item.IsFast && c.TaskName.StartsWith("I")).ToList();
                foreach (var conn in listOfConsistentInterruptConnections)
                {
                    ret += ("Shared memory variable " + item.Name + " (at CFC connector: " + conn.ConnectorPath + ") must be set to \"Fast\" mode. Using shared memory variables in consistent mode is not allowed in interrupt tasks.\n");
                }

            }
            return ret;
        }

        public void RemovePlan(string planName)
        {
            // <<<>>> TO DO: IMPLEMENT - REQUIRED???
        }

        public void RenamePlan(string oldName, string newName)
        {
            foreach (var conn in ListOfShmConnections)
            {
                if (conn.PlanName.Equals(oldName, StringComparison.OrdinalIgnoreCase))
                {
                    conn.RenamePlan(newName);

                    // set dirty flag
                    Dirty = true;
                }
            }
        }

        public void RenameBlock(string blockName, string planName, string newName)
        {
            foreach (var conn in ListOfShmConnections)
            {
                if (conn.PlanName.ToUpper() == planName.ToUpper() && conn.BlockName.ToUpper() == blockName.ToUpper())
                {
                    conn.RenameBlock(newName);

                    // set dirty flag
                    Dirty = true;
                }
            }
        }

        public void RenameCompoundBlock(string blockName, string planName, string newName)
        {
            foreach (var conn in ListOfShmConnections)
            {
                if (conn.PlanName.Equals(planName, StringComparison.OrdinalIgnoreCase))
                {
                    conn.ConnectorPath = conn.ConnectorPath.Replace(blockName, newName);

                    // set dirty flag
                    Dirty = true;
                }
            }
        }

        public void ChangeResource(string resource, string resourceType, string planName)
        {
            foreach (var conn in ListOfShmConnections)
            {
                if (conn.PlanName.Equals(planName, StringComparison.OrdinalIgnoreCase))
                {
                    conn.CPUName = resource.ToUpper();

                    // set dirty flag
                    Dirty = true;
                }
            }
        }

        public void RenameResource(string oldName, string newName)
        {
            foreach (var conn in ListOfShmConnections)
            {
                if (conn.CPUName.Equals(oldName, StringComparison.OrdinalIgnoreCase))
                {
                    //find first backslash and replace the text before it (which is the CPU name)
                    int index = conn.ConnectorPath.IndexOf("\\");
                    if (index > 0)
                    {
                        conn.ConnectorPath = newName.ToUpper() + conn.ConnectorPath.Substring(index);

	                    // set dirty flag
    	                Dirty = true;
                    }
                }
            }
        }

        public string GetSharedMemoryVariableDataType(string VarName)
        {
            foreach (var item in ListOfShmVariables)
            {
                if (item.Name.Equals(VarName, StringComparison.OrdinalIgnoreCase))
                {
                    return item.DataType;
                }
            }

            return string.Empty;
        }

        #region Methods to support DSP3000hw.dll
        public int GetVariablesCount()
        {
            return ListOfShmVariables.Count;
        }

        public string GetVariableName(int index)
        {
            if (index >= 0 && index < ListOfShmVariables.Count)
            {
                return ListOfShmVariables[index].Name;
            }

            return string.Empty;
        }

        public string GetVariableType(int index)
        {
            if (index >= 0 && index < ListOfShmVariables.Count)
            {
                return ListOfShmVariables[index].DataType;
            }

            return string.Empty;
        }

        public byte HasWriteAccess(string VarName)
        {
            return ListOfShmConnections.HasVariableWriteAccess(VarName);
        }

        public string GetVariableUsageString(string VarName)
        {
            return ListOfShmConnections.GetVariableUsageString(VarName);
        }
        #endregion
    }

    /// <summary>
    /// 共享内存变量列表。
    /// </summary>
    [ClassInterface(ClassInterfaceType.None)]
    public class SharedMemoryVariableList : ObservableCollection<SharedMemoryVariable>
    {
        static SharedMemoryVariableList list = null;

        private bool isValid = true;

        public bool IsValid
        {
            get { return isValid; }
            set { isValid = value; }
        }

        public SharedMemoryVariableList()
        {
            list = this;
        }

        private int counterForAssigningNewName = 1;
        private string getUniqueName()
        {
            while (this.Where(v => v.Name.Equals("var" + counterForAssigningNewName, StringComparison.OrdinalIgnoreCase)).ToList().Count > 0)
            {
                counterForAssigningNewName++;
            }
            return "var" + counterForAssigningNewName;
        }

        public void AddNewShmVarWithDefaultValue()
        {
            Add(new SharedMemoryVariable(getUniqueName(), Guid.NewGuid().ToString(), "BOOL", "", true));
        }

        public void RemoveShmVar(string varName)
        {
            //should be always one
            var itemToRemove = this.Where(v => v.Name.Equals(varName, StringComparison.OrdinalIgnoreCase)).ToList();
            foreach (var item in itemToRemove)
            {
                Remove(item);
            }
        }

        public SharedMemoryVariable GetVariableByName(string name)
        {
            foreach (var item in this)
            {
                if (item.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return item;
            }
            return null;
        }

        public bool NameExist(string name)
        {
            return this.Where(v => v.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).ToList().Count > 0;
        }

        static string[] IEC_KEYWORDS = {
            @"ABS",@"ACTION",@"AND",@"ANY",@"ANY_DATE",@"ANY_NUM",@"ARRAY",@"AT",@"ACOS",@"ADD",
            @"ANDN",@"ANY_BIT",@"ANY_INT",@"ANY_REAL",@"ASIN",@"ATAN",
            @"BOOL",@"BY",@"BUSY",@"BYTE",@"CAL",@"CALCN",@"CD",@"CLAIM",@"CONCAT",
            @"CONSTANT",@"CTD",@"CTUD",@"CV",@"CALC",@"CASE",@"CDT",@"CLK",@"CONFIGURATION",@"COS",@"CTU",@"CU",
            @"D",@"DATE_AND_TIME",@"DINT",@"DO",@"DT",@"DATE",@"DELETE",@"DIV",@"DS",@"DWORD",
            @"ELSE",@"ELSEIF",@"END_ACTION",@"END_CASE",@"END_CONFIGURATION",@"END_FOR",@"END_FUNTION",@"END_FUNCTION_BLOCK",
            @"END_IF",@"END_PROGRAM",@"END_REPEAT",@"END_RESOURCE",@"END_STEP",@"END_STRUCT",@"END_TRANSITION",@"END_TYPE",
            @"END_TYPE",@"END_VAR",@"END_WHILE",@"EN",@"ENO",@"EQ",@"ET",@"EXIT",@"EXP",@"EXPT",
            @"FALSE",@"F_EDGE",@"F_TRIG",@"FIND",@"FOR",@"FROM",@"FUNCTION",@"FUNCTION_BLOCK",@"GE",@"GT",
            @"IF",@"IN",@"INITIAL_STEP",@"INSERT",@"INT",@"INTERVAL",
            @"JMP",@"JMPC",@"JMPCN",@"L",@"LD",@"LDN",@"LE",@"LEFT",@"LEN",@"LIMIT",@"LINT",@"LN",@"LOG",@"LREAL",@"LT",@"LWORD",
            @"MAX",@"MID",@"MIN",@"MOD",@"MOVE",@"MUL",@"MUX",@"N",@"NE",@"NEG",@"NOT",
            @"OF",@"ON",@"OR",@"ORN",@"P",@"PRIORITY",@"PROGRAM",@"PT",@"PV",@"Q",@"Q1",@"QU",@"QD",
            @"R",@"R1",@"R_TRIG",@"READ_ONLY",@"READ_WRITE",@"REAL",@"RELEASE",@"REPEAT",@"REPLACE",@"RESOURCE",@"RET",@"RETAIN",
            @"RETC",@"RETCN",@"RETURN",@"RIGHT",@"ROL",@"ROR",@"RS",@"RTC",@"R_EDGE",
            @"S",@"S1",@"SD",@"SEL",@"SEMA",@"SHL",@"SHR",@"SIN",@"SINGLE",@"SINT",@"SL",@"SQRT",@"SR",@"ST",@"STEP",@"STN",@"STRING",
            @"STRUCT",@"SUB",@"TAN",@"TASK",@"THEN",@"TIME",@"TIME_OF_DAY",@"TO",@"TOD",@"TOF",@"TON",@"TP",@"TRANSITION",
            @"TRUE",@"TYPE",@"UDINT",@"UINT",@"ULINT",@"UNTIL",@"USINT",@"VAR",@"VAR_ACCESS",@"VAR_EXTERNAL",@"VAR_GLOBAL",
            @"VAR_INPUT",@"VAR_IN_OUT",@"VAR_OUTPUT",@"WHILE",@"WITH",@"WORD",@"XOR",@"XORN",
	        @"ELSIF",@"TRUE",@"END_FUNCTION",@"" };

        public static bool IsValidIECIdentifier(string name)
        {
            char c;

            if (name == "")
            {
                return false; //there must be a name
            }

            //check for a digit at the first position
            c = name.ElementAt(0);
            if (c >= '0' && c <= '9')
            {
                return false;
            }

            //check for multiple underscores in a row
            if (name.IndexOf("__") >= 0)
            {
                return false;
            }

            //check all the characters
            for (int i = 0; i < name.Length; i++)
            {
                c = name.ElementAt(i);

                if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_'))
                {
                    return false;
                }
            }

            //ALI 14.4.2005: 2005/266 a valid filename must end with a number or a character
            if (name.Last() == ' ')
            {
                return false; //no special characters
            }

            // check if filename is IEC keyword
            int nIECKeysIndex = 0;
            while ("" != IEC_KEYWORDS[nIECKeysIndex])
            {
                if (name.Equals(IEC_KEYWORDS[nIECKeysIndex], StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
                nIECKeysIndex++;
            }

            return true;
        }

        public static bool IsUniqueIdentifier(string name, SharedMemoryVariable currentVariable)
        {
            foreach (var item in list)
            {
                if (item != currentVariable)
                {
                    if (item.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }

    /// <summary>
    /// 共享内存变量列表管理器管理的共享内存变量信息。
    /// </summary>
    public class SharedMemoryVariable : INotifyPropertyChanged, IComparable
    {
        public SharedMemoryVariable(string name, string variableID, string dataType, string comment, bool isFast)
        {
            VariableID = variableID;
            Name = name;
            DataType = dataType;
            Comment = comment;
            IsFast = isFast;
        }

        public void Reset()
        {
            ConnectionCounter = 0;
        }

        #region <<<<<<<< Properties >>>>>>>>
        private string variableID = string.Empty;
        private string name = String.Empty;
        private string dataType = String.Empty;
        private string comment = String.Empty;
        private bool isFast = false;
        private bool isConnected = false;
        private bool isMarkedForConnecting = false;
        private int connectionCounter = 0;

        public string VariableID
        {
            get { return this.variableID; }
            protected set
            {
                if (value != this.variableID)
                {
                    this.variableID = value;
                    NotifyPropertyChanged("VariableID");
                }
            }
        }

        public string Name
        {
            get { return this.name; }
            set
            {
                if (value != this.name)
                {
                    this.name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }
        public string DataType
        {
            get { return this.dataType; }
            set
            {
                if (value != this.dataType)
                {
                    this.dataType = value;
                    NotifyPropertyChanged("DataType");
                }
            }
        }
        public string Comment
        {
            get { return this.comment; }
            set
            {
                if (value != this.comment)
                {
                    this.comment = value;
                    NotifyPropertyChanged("Comment");
                }
            }
        }
        public bool IsFast
        {
            get { return this.isFast; }
            set
            {
                if (value != this.isFast)
                {
                    this.isFast = value;
                    NotifyPropertyChanged("IsFast");
                }
            }
        }
        public bool IsConnected
        {
            get { return this.isConnected; }
            set
            {
                if (value != this.isConnected)
                {
                    this.isConnected = value;
                    NotifyPropertyChanged("IsConnected");
                }
            }
        }
        public bool IsMarkedForConnecting
        {
            get { return this.isMarkedForConnecting; }
            set
            {
                if (value != this.isMarkedForConnecting)
                {
                    this.isMarkedForConnecting = value;
                    NotifyPropertyChanged("IsMarkedForConnecting");
                }
            }
        }
        public int ConnectionCounter
        {
            get { return this.connectionCounter; }
            set
            {
                if (value != this.connectionCounter)
                {
                    this.connectionCounter = value;
                    IsConnected = value > 0;
                    NotifyPropertyChanged("ConnectionCounter");
                }
            }
        }
        #endregion

        public XmlNode GetXmlShmVariableNode(XmlDocument doc, SharedMemoryConnectionList connectionList)
        {
            XmlElement variable = doc.CreateElement("Variable");

            XmlElement name = doc.CreateElement("Name");
            name.InnerText = this.Name;
            variable.AppendChild(name);
            XmlElement variableID = doc.CreateElement("VariableID");
            variableID.InnerText = this.VariableID;
            variable.AppendChild(variableID);
            XmlElement dataType = doc.CreateElement("DataType");
            dataType.InnerText = this.DataType;
            variable.AppendChild(dataType);
            XmlElement comment = doc.CreateElement("Comment");
            comment.InnerText = this.Comment;
            variable.AppendChild(comment);
            XmlElement isFast = doc.CreateElement("IsFast");
            isFast.InnerText = this.IsFast.ToString();
            variable.AppendChild(isFast);

            XmlElement connections = doc.CreateElement("Connections");
            List<SharedMemoryConnection> listConns = new List<SharedMemoryConnection>();
            foreach (SharedMemoryConnection connection in connectionList)
            {
                if (connection.Variable == this && connection.ConnectorPath != string.Empty)
                {
                    listConns.Add(connection);
                }
            }

            listConns.Sort();

            foreach (SharedMemoryConnection connection in listConns)
            {
                connections.AppendChild(connection.GetXmlShmConnectionNode(doc));
            }

            variable.AppendChild(connections);

            return variable;
        }

        public override string ToString()
        {
            string isFast = this.IsFast ? "Yes" : "";
            return this.Name + "*" + this.DataType + "*" + this.Comment + "*" + isFast;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public int CompareTo(object obj)
        {
            SharedMemoryVariable v = obj as SharedMemoryVariable;

            if (v == null)
            {
                return 0;
            }

            //for sorting; sort by: (1) variable name
            return this.Name.CompareTo(v.Name);
        }
    }

    /// <summary>
    /// 共享内存变量连线列表。
    /// </summary>
    [ClassInterface(ClassInterfaceType.None)]
    public class SharedMemoryConnectionList : ObservableCollection<SharedMemoryConnection>
    {
        public void SortBy(Func<SharedMemoryConnection, string> keySelector)
        {
            List<SharedMemoryConnection> sortedList = this.OrderBy(keySelector).ToList();
            for (int i = 0; i < sortedList.Count(); i++)
            {
                this.Move(this.IndexOf(sortedList[i]), i);
            }
        }

        public bool IsVariableConnected(string varName)
        {
            return this.Where(c => c.Variable.Name.Equals(varName, StringComparison.OrdinalIgnoreCase)).ToList().Count > 0;
        }

        public void RemoveEmptyConnection(string varName)
        {
            //should always contain one element
            var connectionTeRemove = this.Where(c => c.Variable.Name.Equals(varName, StringComparison.OrdinalIgnoreCase)).ToList();
            foreach (var item in connectionTeRemove)
            {
                Remove(item);
            }
        }

        public byte HasVariableWriteAccess(string varName)
        {
            var connections = this.Where(c => c.Variable.Name.Equals(varName, StringComparison.OrdinalIgnoreCase)).ToList();

            if (connections.Count == 0)
            {
                //error
                return 0xFF;
            }

            foreach (var item in connections)
            {
                if (item.CPUName.Length > 0 && item.TaskName.Length > 0)
                {
                    if (item.IsReadingSignal == false)
                    {
                        //variable has write access
                        return 1;
                    }
                }
                //else: unconnected variable
            }

            //variable has no write access
            return 0;
        }

        public string GetVariableUsageString(string varName)
        {
            string result = string.Empty;

            var connections = this.Where(c => c.Variable.Name.Equals(varName, StringComparison.OrdinalIgnoreCase)).ToList();
            foreach (var item in connections)
            {
                if (item.CPUName.Length > 0 && item.TaskName.Length > 0)
                {
                    if (item.IsReadingSignal == false)
                    {
                        result += "W#" + item.CPUName + "\\" + item.TaskName + " ";
                    }
                    else if (item.IsReadingSignal == true)
                    {
                        result += "R#" + item.CPUName + "\\" + item.TaskName + " ";
                    }
                }
            }

            return result;
        }

        public void AddConnection(SharedMemoryConnection newConnection)
        {
            this.Add(newConnection);

            //in case of adding the first connection, the empty connection entry must be removed from the connection list
            if (newConnection.Variable.ConnectionCounter++ == 0)
            {
                //the list should only contain exactly one element
                var emptyConnection = this.Where(c => c.ConnectorPath == "" && c.Variable.Name.Equals(newConnection.Variable.Name, StringComparison.OrdinalIgnoreCase)).ToList();
                foreach (var item in emptyConnection)
                {
                    this.Remove(item);
                }
            }
        }

        public void RemoveConnection(SharedMemoryConnection connectionToRemove)
        {
            this.Remove(connectionToRemove);

            //in case of removing last connection, the empty connection entry must be added back to the connection list
            if (--connectionToRemove.Variable.ConnectionCounter == 0)
            {
                SharedMemoryConnection newConnection = new SharedMemoryConnection();
                newConnection.ConnectorPath = string.Empty;
                newConnection.Variable = connectionToRemove.Variable;

                this.Add(newConnection);
            }
        }
    }

    /// <summary>
    /// 共享内存变量列表管理器管理的共享内存变量连线信息。
    /// </summary>
    public class SharedMemoryConnection : INotifyPropertyChanged, IComparable
    {
        #region <<<<<<<< Properties >>>>>>>>
        private string connectorPath = string.Empty;
        private SharedMemoryVariable variable = null;
        private bool? isReadingSignal = null;
        private string cpuName = string.Empty;
        private string planName = string.Empty;
        private string taskName = string.Empty;
        private string targetConnector = string.Empty;
        private string blockType = string.Empty;
        public string ConnectorPath
        {
            get { return this.connectorPath; }
            set
            {
                if (value != this.connectorPath)
                {
                    this.connectorPath = value;
                    CPUName = RetrieveCPUName();
                    PlanName = RetrievePlanName();

                    TargetConnector = RetrieveTargetConnector();
                    NotifyPropertyChanged("ConnectorPath");
                }
            }
        }

        public SharedMemoryVariable Variable
        {
            get { return this.variable; }
            set
            {
                if (value != this.variable)
                {
                    this.variable = value;
                    NotifyPropertyChanged("Variable");
                }
            }
        }

        //true if is reading signal, false if is writing signal
        public bool? IsReadingSignal
        {
            get { return this.isReadingSignal; }
            set
            {
                if (value != this.isReadingSignal)
                {
                    this.isReadingSignal = value;
                    NotifyPropertyChanged("IsReadingSignal");
                }
            }
        }

        public string CPUName
        {
            get { return this.cpuName; }
            set
            {
                if (value != this.cpuName)
                {
                    this.cpuName = value;
                    NotifyPropertyChanged("CPUName");
                }
            }
        }

        public string PlanName
        {
            get { return this.planName; }
            set
            {
                if (value != this.planName)
                {
                    this.planName = value;
                    NotifyPropertyChanged("PlanName");
                }
            }
        }

        public string TaskName
        {
            get { return this.taskName; }
            set
            {
                if (value != this.taskName)
                {
                    this.taskName = value;
                    NotifyPropertyChanged("TaskName");
                }
            }
        }

        public string TargetConnector
        {
            get { return this.targetConnector; }
            set
            {
                if (value != this.targetConnector)
                {
                    this.targetConnector = value;
                    NotifyPropertyChanged("TargetConnector");
                }
            }
        }

        public string BlockType
        {
            get { return this.blockType; }
            set
            {
                if (value != this.blockType)
                {
                    this.blockType = value;
                    NotifyPropertyChanged("BlockType");
                }
            }
        }

        public string BlockName
        {
            get { return targetConnector.Split(new char[] { '\\' })[0]; }
        }

        #endregion

        public string RetrievePlanName()
        {
            return ViConnType.RetrievePlanName(this.connectorPath);
        }

        public string RetrieveCPUName()
        {
            return ViConnType.RetrieveCpuName(this.connectorPath);
        }

        public string RetrieveTargetConnector()
        {
            if (string.IsNullOrEmpty(this.connectorPath))
                return string.Empty;

            int endIndex = connectorPath.LastIndexOf('\\');
            if (endIndex > 0)
            {
                endIndex = connectorPath.LastIndexOf('\\', endIndex - 1);
                if (endIndex >= 0)
                    return this.connectorPath.Substring(endIndex + 1, connectorPath.Length - 1 - endIndex).Trim();
            }
            
            return string.Empty;
        }

        public XmlNode GetXmlShmConnectionNode(XmlDocument doc)
        {
            XmlElement connection = doc.CreateElement("Connection");

            XmlElement connectorPath = doc.CreateElement("ConnectorPath");
            connectorPath.InnerText = this.ConnectorPath;
            connection.AppendChild(connectorPath);
            XmlElement isReadingSignal = doc.CreateElement("IsReadingSignal");
            isReadingSignal.InnerText = this.IsReadingSignal.ToString();
            connection.AppendChild(isReadingSignal);
            XmlElement taskName = doc.CreateElement("TaskName");
            taskName.InnerText = this.TaskName;
            connection.AppendChild(taskName);
            XmlElement blockType = doc.CreateElement("BlockType");
            blockType.InnerText = this.BlockType;
            connection.AppendChild(blockType);

            return connection;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public void RenamePlan(string newName)
        {
            int iStart = connectorPath.IndexOf('\\');
            int iEnd = connectorPath.IndexOf('(', iStart + 1);

            ConnectorPath = connectorPath.Substring(0, iStart + 1) + newName + " " + connectorPath.Substring(iEnd);
        }

        public void RenameBlock(string newName)
        {
            int iStart = connectorPath.LastIndexOf('\\', connectorPath.LastIndexOf('\\') - 1);
            int iEnd = connectorPath.LastIndexOf('\\');

            ConnectorPath = connectorPath.Substring(0, iStart + 1) + newName + connectorPath.Substring(iEnd);
        }

        public int CompareTo(object obj)
        {
            SharedMemoryConnection c = obj as SharedMemoryConnection;

            if (c == null)
            {
                return 0;
            }

            //for sorting; sort by: (1) connector path
            return this.ConnectorPath.CompareTo(c.ConnectorPath);
        }
    }
}

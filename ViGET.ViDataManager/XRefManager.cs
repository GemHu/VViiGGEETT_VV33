/// <summary>
/// @file   ProjectManager.XRef.cs
///	@brief  ViGET 工程数据管理器。
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

namespace Dothan.Manager
{
    /// <summary>
    /// ViGET 工程 XRef 数据管理器。
    /// </summary>
    public class XRefManager : IXRef, IViGETManager
    {
        //public static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private IOAddressManager IOAddressMgr;
        private BlockListManager BlockListMgr;
        private SharedMemoryManager SharedMemoryMgr;
		private CompoundBlockManager CompoundBlockMgr; //KHo 20111226
        private BuildListManager BuildListMgr = null;
        private string XRefListPath = string.Empty;
        private string XShmListPath = string.Empty;

        /// <summary>
        /// XRef info is changed or not.
        /// </summary>
        public bool XRefDirty
        {
            get
            {
                if (IOAddressMgr != null && IOAddressMgr.Dirty)
                    return true;

                if (BlockListMgr != null && BlockListMgr.Dirty)
                    return true;

                if (SharedMemoryMgr != null && SharedMemoryMgr.Dirty)
                    return true;

                if (CompoundBlockMgr != null && CompoundBlockMgr.Dirty)
                    return true;

                if (BuildListMgr != null && BuildListMgr.Dirty)
                    return true;

                return false;
            }
            set
            {
                if (!value)
                {
                    if (IOAddressMgr != null)
                        IOAddressMgr.Dirty = false;

                    if (BlockListMgr != null)
                        BlockListMgr.Dirty = false;

                    if (SharedMemoryMgr != null)
                        SharedMemoryMgr.Dirty = false;

                    if (BuildListMgr != null)
                        BuildListMgr.Dirty = false;

                    if (CompoundBlockMgr != null)
                        CompoundBlockMgr.Dirty = false;
                }
            }
        }

        /// <summary>
        /// Init XRef info.
        /// </summary>
        /// <param name="projectPath">Project path(is not project var file)</param>
        /// <returns>Warning message string. Not used now.</returns>
        public string InitXRef(string projectPath)
        {
            if (string.IsNullOrEmpty(projectPath))
                return null;
            // remove ending path seperated char if has
            projectPath = projectPath.TrimEnd(new char[] { '\\' });

            BlockListMgr = new BlockListManager(projectPath);
            IOAddressMgr = new IOAddressManager(projectPath);
            SharedMemoryMgr = new SharedMemoryManager(projectPath);
			CompoundBlockMgr = new CompoundBlockManager(projectPath);//KHo 20111226            
            BuildListMgr = new BuildListManager(projectPath);

            XRefListPath = Path.Combine(projectPath, "xref.data");
            XShmListPath = Path.Combine(projectPath, "xshm.data");

            // if upgrade from xref.data to xref.data + xshm.data, we keep new files'
            // time is same as old xref.data's.
            bool keepTime = false; DateTime dtLastWriteTime = DateTime.Now;
            if (File.Exists(XRefListPath) && !File.Exists(XShmListPath))
            {
                dtLastWriteTime = File.GetLastWriteTime(XRefListPath);
                keepTime = true;
            }

            // load xref & xshm list info
            string msg = "";
            LoadXRefList(ref msg);

            // auto-save file if current is dirty
            if (this.XRefDirty)
            {
                SaveXRefList(false);

                // keep two files' time if needed
                if (keepTime)
                {
                    File.SetLastWriteTime(XRefListPath, dtLastWriteTime);
                    File.SetLastWriteTime(XShmListPath, dtLastWriteTime);
                }
            }

            return msg;
        }

        /// <summary>
        /// Reload XRef info.
        /// </summary>
        public void ReloadXRef()
        {
            string msg = "";
            LoadXRefList(ref msg);
        }

		public IIOAddressManager GetIOAddressManager()
		{
			return IOAddressMgr;
		}

        public IBlockListManager GetBlockListManager()
        {
            return BlockListMgr;
        }
        
        public ISharedMemoryManager GetSharedMemoryManager()
        {
            return SharedMemoryMgr;
        }

		public ICompoundBlockManager GetCompoundBlockManager()
		{
			return CompoundBlockMgr;
		}

        public IBuildListManager GetBuildListManager()
        {
            return BuildListMgr;
        }

        public void LoadXRefList(ref string msg)
        {
            try
            {
                XmlDocument docXRef = new XmlDocument();
                XmlDocument docXShm = new XmlDocument();
                if (File.Exists(XRefListPath))
                    docXRef.Load(XRefListPath);
                if (File.Exists(XShmListPath))
                    docXShm.Load(XShmListPath);

                // initialize blocklist
                BlockListMgr.Init(docXRef.SelectNodes("xref//Blocks//Block"));
                IOAddressMgr.Init(docXRef.SelectNodes("xref//IOConnections//Connection"));
                CompoundBlockMgr.Init(docXRef.SelectNodes("xref//CompoundBlocks//CompoundBlock"));
                BuildListMgr.Init(docXRef.SelectNodes("xref//BuildManager//Plan"));
                // force to save if file not exist
                if (!File.Exists(XRefListPath))
                    BlockListMgr.Dirty = true;

                // initialize shared memory list
                XmlNodeList ShmVariableList = docXShm.SelectNodes("xref//SharedMemory//Variable");
                SharedMemoryMgr.Init(ShmVariableList, ref msg);
                if (!string.IsNullOrEmpty(msg))
                {
                    // loaded from PIV file, force to save to new file
                    SharedMemoryMgr.Dirty = true;
                }
                else
                {
                    // check is old version xref.data or not. In new version, xref.data is seperated
                    // into xref.data & xshm.data two files.
                    if (ShmVariableList == null || ShmVariableList.Count <= 0)
                    {
                        XmlNodeList RefVariableList = docXRef.SelectNodes("xref//SharedMemory//Variable");
                        if (RefVariableList != null && RefVariableList.Count > 0)
                        {
                            SharedMemoryMgr.Init(RefVariableList, ref msg);
                            msg = "A shared memory variable database from xref.data was imported into xshm.data.";
                            // loaded from PIV file, force to save to new file
                            BlockListMgr.Dirty = true;
                            SharedMemoryMgr.Dirty = true;
                        }
                    }
                }
                // force to save if file not exist
                if (!File.Exists(XShmListPath))
                    SharedMemoryMgr.Dirty = true;
            }
            catch (Exception)
            {
            }
        }

        public void CleanXRefList()
        {
            BlockListMgr.CleanBlockList();
			CompoundBlockMgr.CleanBlockList();
            SharedMemoryMgr.CleanList();

            try
            {
                XmlDocument doc = new XmlDocument();
                if (File.Exists(XRefListPath))
                    doc.Load(XRefListPath);

                if (doc.DocumentElement == null || doc.DocumentElement.Name != "xref")
                {
                    doc.RemoveAll();
                    doc.AppendChild(doc.CreateElement("xref"));
                }

                XmlNode node = doc.SelectSingleNode("xref//Blocks");
                if (node == null)
                {
                    node = doc.CreateElement("Blocks");
                    doc.DocumentElement.AppendChild(node);
                }
                node.RemoveAll();

                node = doc.SelectSingleNode("xref//IOConnections");
                if (node == null)
                {
                    node = doc.CreateElement("IOConnections");
                    doc.DocumentElement.AppendChild(node);
                }
                node.RemoveAll();

				//KHo 20111226
				node = doc.SelectSingleNode("xref//CompoundBlocks");
				if (node == null)
				{
					node = doc.CreateElement("CompoundBlocks");
					doc.DocumentElement.AppendChild(node);
				}
                node.RemoveAll();

                //lsu 20120509
                node = doc.SelectSingleNode("xref//BuildManager");
                if (node == null)
                {
                    node = doc.CreateElement("BuildManager");
                    doc.DocumentElement.AppendChild(node);
                }
                node.RemoveAll();

                doc.Save(XRefListPath);

                // set dirty flag
                BlockListMgr.Dirty = true;
                IOAddressMgr.Dirty = true;
                CompoundBlockMgr.Dirty = true;
                BuildListMgr.Dirty = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                //logger.Error(e.Message + " at " + e.StackTrace);
            }
        }

        public void SaveXRefList(bool createBackup)
        {
            SaveXRefList(createBackup, false);
        }

        public void SaveXRefList(bool createBackup, bool force)
        {
            if (force || BlockListMgr.Dirty || IOAddressMgr.Dirty || CompoundBlockMgr.Dirty || BuildListMgr.Dirty)
            {
				string XRefTmpPath = Path.ChangeExtension(XRefListPath, "tmp");
				string XRefBakPath = Path.ChangeExtension(XRefListPath, "bak");

                try
                {
                    XmlDocument doc = new XmlDocument();
                    if (File.Exists(XRefListPath))
					{
                        if (createBackup)
                        {
                            //create a temporary copy of the existing file version
                            try { File.Delete(XRefTmpPath); }
                            catch (Exception) { }
                            File.Copy(XRefListPath, XRefTmpPath);
                        }

                        doc.Load(XRefListPath);
					}

                    if (doc.DocumentElement == null || doc.DocumentElement.Name != "xref")
                    {
                        doc.RemoveAll();
                        doc.AppendChild(doc.CreateElement("xref"));
                    }

                    //save block list
                    XmlNode node = doc.SelectSingleNode("xref//Blocks");
                    if (node == null)
                    {
                        node = doc.CreateElement("Blocks");
                        doc.DocumentElement.AppendChild(node);
                    }
                    node.RemoveAll();
					List<Block> listOfBlocks = BlockListMgr.GetBlockList2();
                    foreach (var item in listOfBlocks)
                    {
                        node.AppendChild(item.GetXmlBlockNode(doc));
                    }

                    //save I/O connections
                    node = doc.SelectSingleNode("xref//IOConnections");
                    if (node == null)
                    {
                        node = doc.CreateElement("IOConnections");
                        doc.DocumentElement.AppendChild(node);
                    }
                    node.RemoveAll();
                    List<IOConnection> listOfIOConnections = IOAddressMgr.GetIOConnectionList2();
                    foreach (var item in listOfIOConnections)
                    {
                        XmlNode newConnection = item.GetXmlConnectionNode(doc);
                        if (newConnection != null)
                            node.AppendChild(newConnection);
                    }

#if true
                    //shared memory variables is saved to xshm.data later
                    node = doc.SelectSingleNode("xref//SharedMemory");
                    if (node != null)
                    {
                        node.ParentNode.RemoveChild(node);
                    }
#else
                    //also save shared memory variables into xref.data to make ViGET happy.
                    node = doc.SelectSingleNode("xref//SharedMemory");
                    if (node == null)
                    {
                        node = doc.CreateElement("SharedMemory");
                        doc.DocumentElement.AppendChild(node);
                    }
                    node.RemoveAll();
                    SharedMemoryVariableList listOfShmVars = null;
                    SharedMemoryMgr.GetShmVariableList(ref listOfShmVars);
                    foreach (var item in listOfShmVars)
                    {
                        SharedMemoryConnectionList listOfConns = null;
                        SharedMemoryMgr.GetShmConnectionList(ref listOfConns);
                        XmlNode newVariable = item.GetXmlShmVariableNode(doc, listOfConns);
                        if (newVariable != null)
                            node.AppendChild(newVariable);
                    }
#endif

                    //save compound block list
                    node = doc.SelectSingleNode("xref//CompoundBlocks");
                    if (node == null)
                    {
                        node = doc.CreateElement("CompoundBlocks");
                        doc.DocumentElement.AppendChild(node);
                    }
                    node.RemoveAll();
                    List<CompoundBlock> compoundblocklist = CompoundBlockMgr.GetCompBlockList2();
                    foreach (var item in compoundblocklist)
                    {
                        node.AppendChild(item.GetXmlBlockNode(doc));
                    }

	                //lsu 20120509
    	            node = doc.SelectSingleNode("xref//BuildManager");
        	        if (node == null)
            	    {
                	    node = doc.CreateElement("BuildManager");
                    	doc.DocumentElement.AppendChild(node);
	                }
    	            node.RemoveAll();
                    foreach (var item in BuildListMgr.GetPlanList())
            	    {
                	    node.AppendChild(item.GetXmlBlockNode(doc));
                	}

                    doc.Save(XRefListPath);

                    if (createBackup)
                    {
                        //compare old and new file version; if different, keep the old version as xref.bak
                        if (HasDifferences(XRefListPath, XRefTmpPath))
                        {
                            try { File.Delete(XRefBakPath); }
                            catch (Exception) { }
                            File.Move(XRefTmpPath, XRefBakPath);
                        }
                        else
                        {
                            try { File.Delete(XRefTmpPath); }
                            catch (Exception) { }
                        }
                    }

                    // reset dirty flag
                    BlockListMgr.Dirty = false;
                    IOAddressMgr.Dirty = false;
                    CompoundBlockMgr.Dirty = false;
                    BuildListMgr.Dirty = false;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    //logger.Error(e.Message + " at " + e.StackTrace);
                }
            }

            if (force || SharedMemoryMgr.Dirty)
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    if (File.Exists(XShmListPath))
                        doc.Load(XShmListPath);

                    if (doc.DocumentElement == null || doc.DocumentElement.Name != "xref")
                    {
                        doc.RemoveAll();
                        doc.AppendChild(doc.CreateElement("xref"));
                    }

                    //save shared memory variables
                    XmlNode node = doc.SelectSingleNode("xref//SharedMemory");
                    if (node == null)
                    {
                        node = doc.CreateElement("SharedMemory");
                        doc.DocumentElement.AppendChild(node);
                    }
                    node.RemoveAll();
                    SharedMemoryVariableList listOfShmVars = null;
                    SharedMemoryMgr.GetShmVariableList(ref listOfShmVars);
                    foreach (var item in listOfShmVars)
                    {
                        SharedMemoryConnectionList listOfConns = null;
                        SharedMemoryMgr.GetShmConnectionList(ref listOfConns);
                        XmlNode newVariable = item.GetXmlShmVariableNode(doc, listOfConns);
                        if (newVariable != null)
                            node.AppendChild(newVariable);
                    }

                    doc.Save(XShmListPath);

                    // reset dirty flag
                    SharedMemoryMgr.Dirty = false;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    //logger.Error(e.Message + " at " + e.StackTrace);
                }
            }
        }

        /// <summary>
        /// Set new hardware config file. It should be called if project is opened or hardware config file is changed.
        /// </summary>
        /// <param name="fileName">Hardware config file full path name.</param>
        /// <returns>Success or not.</returns>
        public bool SetHWConfigFile(string fileName)
        {
            if (IOAddressMgr == null) return false;
            return IOAddressMgr.SetHWConfigFile(fileName);
        }

        private bool HasDifferences(string filePath1, string filePath2)
        {
            byte[] hash1;
            byte[] hash2;

            //compute file hashes of both files
            using (var stream = File.OpenRead(filePath1))
            {
                hash1 = System.Security.Cryptography.MD5.Create().ComputeHash(stream); //returns a byte[16]
            }
            using (var stream = File.OpenRead(filePath2))
            {
                hash2 = System.Security.Cryptography.MD5.Create().ComputeHash(stream); //returns a byte[16]
            }

            //compare hashes
            for (int i = 0; i < hash1.GetLength(0); i++)
            {
                if (hash1[i] != hash2[i])
                {
                    return true;
                }
            }

            return false;
        }

        public void UpdateRuntimeInformation(string blockName, string planName, string runtimeTask, string runtimeGroup, string runSeqNum)
        {
            BlockListMgr.UpdateRuntimeInformation(blockName, planName, runtimeTask, runtimeGroup, runSeqNum);
            SharedMemoryMgr.UpdateRuntimeInformation(blockName, planName, runtimeTask);
        }

        public void RemovePlan(string planName)
        {
            BlockListMgr.RemoveBlocks(planName);
            IOAddressMgr.RemoveIOConnectionsInPlan(planName);
            SharedMemoryMgr.RemoveShmConnections(planName);
            CompoundBlockMgr.RemoveBlocks(planName);
            BuildListMgr.Unregister(planName);
        }

        public void RenamePlan(string oldName, string newName)
        {
            BlockListMgr.RenamePlan(oldName, newName);
            SharedMemoryMgr.RenamePlan(oldName, newName);
            CompoundBlockMgr.RenamePlan(oldName, newName);
            BuildListMgr.RenamePlan(oldName, newName);
        }

        public void RenameCompoundBlock(string blockName, string planName, string newName)
        {
            //TO DO: IMPLEMENT - CURRENTLY SOLVED DIFFERENTLY IN OTHER METHODS OF SHMMGR. AND CBMGR. - MISSING IN BLOCKLISTMGR.???
        }

        public void ChangeResource(string resource, string resourceType, string planName)
        {
            BlockListMgr.ChangeResource(resource, resourceType, planName);
            SharedMemoryMgr.ChangeResource(resource, resourceType, planName);
            CompoundBlockMgr.ChangeResource(resource, resourceType, planName);
            BuildListMgr.ChangeResource(resource, resourceType, planName);
        }

        public void RenameResource(string oldName, string newName)
        {
            BlockListMgr.RenameResource(oldName, newName);
            SharedMemoryMgr.RenameResource(oldName, newName);
            CompoundBlockMgr.RenameResource(oldName, newName);
            BuildListMgr.RenameResource(oldName, newName);
        }
	}
}

/// <summary>
/// @file   BlockListManager.cs
///	@brief  ViGET 工程的已使用的功能块列表管理器。
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
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;
//using log4net;
using System.IO;
using System.ComponentModel;

namespace Dothan.Manager
{
    /// <summary>
    /// ViGET 工程的已使用的功能块列表管理器，每个工程有一个这样的对象。
    /// </summary>
    public class BlockListManager : IBlockListManager, IViGETManager
    {
        /// <summary>
        /// Object info is changed or not.
        /// </summary>
        public bool Dirty { get; set; }

        //public static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public BlockListManager(string projectPath)
        {
            XRefListPath = Path.Combine(projectPath, "xref.data");

            blockList.CollectionChanged += blockList_CollectionChanged;

            // reset dirty flag
            Dirty = false;
        }

        void blockList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Dirty = true;
        }

        //the block list in memory which is used in CFC or other components
        private BlockList blockList = new BlockList();

        private string XRefListPath;

        public void AddBlock(
            string blockName,
            string blockType,
            string planName,
            string cpu,
            string sourceLibrary,
            string sourceLibraryVersion,
            string runtimeTask,
            string runtimeGroup,
            string runSeqNum)
        {
            blockList.Add(new Block(
                blockName,
                blockType,
                planName,
                cpu,
                sourceLibrary,
                sourceLibraryVersion,
                runtimeTask,
                runtimeGroup,
                runSeqNum
                ));
        }

        public void UpdateRuntimeInformation(string blockName, string planName, string runtimeTask, string runtimeGroup, string runSeqNum)
        {
            Block b = GetBlock(blockName, planName);
            if (b != null)
            {
                b.RuntimeTask = runtimeTask;
                b.RuntimeGroup = runtimeGroup;
                b.RunSeqNum = runSeqNum;

                // set dirty flag
                Dirty = true;
            }
        }

        private Block GetBlock(string blockName, string planName)
        {
            foreach (var block in blockList)
            {
                if (block.PlanName.Equals(planName, StringComparison.OrdinalIgnoreCase) &&
					block.BlockName.Equals(blockName, StringComparison.OrdinalIgnoreCase))
                    return block;
            }
            return null;
        }

        public void RemoveBlock(string blockName, string planName)
        {
            foreach (var item in blockList)
            {
                if (item.BlockName.Equals(blockName, StringComparison.OrdinalIgnoreCase) &&
					item.PlanName.Equals(planName, StringComparison.OrdinalIgnoreCase))
                {
                    blockList.Remove(item);
                    break;
                }
            }
        }

        public void RenameBlock(string blockName, string planName, string newName)
        {
            foreach (var item in blockList)
            {
                if (item.BlockName.Equals(blockName, StringComparison.OrdinalIgnoreCase) &&
					item.PlanName.Equals(planName, StringComparison.OrdinalIgnoreCase))
                {
                    item.BlockName = newName;

                    // set dirty flag
                    Dirty = true;

                    break;
                }
            }
        }

        public void RemoveBlocks(string planName)
        {
            var itemsToRemove = blockList.Where(b => b.PlanName.Equals(planName, StringComparison.OrdinalIgnoreCase)).ToList();
            foreach (var itemToRemove in itemsToRemove)
            {
                blockList.Remove(itemToRemove);
            }
        }

        public bool IsAnyBlockFromLibraryUsed(string libraryName)
        {
            foreach (var item in blockList)
            {
                if (item.SourceLibraryName.Equals(libraryName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

		public bool IsAnyBlockFromLibraryUsed(string libraryName, string resourceName)
		{
			var itemsToDetect = blockList.Where(b => b.SourceLibraryName.Equals(libraryName, StringComparison.OrdinalIgnoreCase)).ToList();
			foreach (var item in itemsToDetect)
			{
				if (item.CPU.Equals(resourceName, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
			return false;
		}

		public void GetUsedBlockList(string libraryName, string resourceName, ref BlockList List)
		{
			var itemsToDetect = blockList.Where(b => b.SourceLibraryName.Equals(libraryName, StringComparison.OrdinalIgnoreCase)).ToList();
			foreach (var item in itemsToDetect)
			{
				if (item.CPU.Equals(resourceName, StringComparison.OrdinalIgnoreCase))
				{
					List.Add(item); 
				}
			}
		}

        public void ReplaceLibrary(string oldName, string oldVersion, string newName, string newVersion, string resourceName)
        {
            var itemsToModify = blockList.Where(b => b.SourceLibraryName.Equals(oldName, StringComparison.OrdinalIgnoreCase) &&
													 b.SourceLibraryVersion.Equals(oldVersion, StringComparison.OrdinalIgnoreCase)).ToList();
            foreach (var item in itemsToModify)
            {
                if (item.CPU.Equals(resourceName, StringComparison.OrdinalIgnoreCase))
                {
                    item.SourceLibraryName = newName;
                    item.SourceLibraryVersion = newVersion;

                    // set dirty flag
                    Dirty = true;
                }
            }
        }

        public void Init(XmlNodeList blockListXml)
        {
            //clean the list
            blockList.Clear();

            foreach (XmlNode item in blockListXml)
            {
                blockList.Add(
                    new Block(
                            item.SelectSingleNode("BlockName").InnerText,
                            item.SelectSingleNode("BlockType").InnerText,
                            item.SelectSingleNode("PlanName").InnerText,
                            item.SelectSingleNode("CPU").InnerText,
                            item.SelectSingleNode("SourceLibrary").InnerText,
                            item.SelectSingleNode("SourceLibraryVersion").InnerText,
                            item.SelectSingleNode("RuntimeTask").InnerText,
                            item.SelectSingleNode("RuntimeGroup").InnerText,
                            item.SelectSingleNode("RunSeqNum").InnerText
                            )
                        );
            }

            // reset dirty flag
            Dirty = false;
        }

        public void CleanBlockList()
        {
            //clean the list
            blockList.Clear();
        }

        //KHo 20110607
        public void GetBlockList(ref BlockList list)
        {
            list = blockList;
        }

        public List<Block> GetBlockList2()
        {
            List<Block> list = new List<Block>();

            foreach (Block b in blockList)
            {
                list.Add(b);
            }

            list.Sort();

            return list;
        }

        //lsu 20120215
        public void RenamePlan(string oldName, string newName)
        {
            var itemsToRename = blockList.Where(b => b.PlanName.Equals(oldName, StringComparison.OrdinalIgnoreCase)).ToList();
            foreach (var itemToRename in itemsToRename)
            {
                itemToRename.PlanName = newName;

                // set dirty flag
                Dirty = true;
            }
        }

        public void RenameCompoundBlock(string blockName, string planName, string newName)
        {
            //TO DO: IMPLEMENT - REQUIRED???
        }

        public void ChangeResource(string resource, string resourceType, string planName)
        {
            var itemsToRename = blockList.Where(b => b.PlanName.ToUpper().Equals(planName.ToUpper())).ToList();
            foreach (var itemToRename in itemsToRename)
            {
                itemToRename.CPU = resource;

                // set dirty flag
                Dirty = true;
            }
        }

        public void RenameResource(string oldName, string newName)
        {
            var itemsToRename = blockList.Where(b => b.CPU.ToUpper().Equals(oldName.ToUpper())).ToList();
            foreach (var itemToRename in itemsToRename)
            {
                itemToRename.CPU = newName;

                // set dirty flag
                Dirty = true;
            }
        }
    }

    /// <summary>
    /// 功能块列表。支持根据指定排序函数排序的功能。
    /// </summary>
    [ClassInterface(ClassInterfaceType.None)]
    public class BlockList : ObservableCollection<Block>
    {
        public void SortBy(Func<Block, string> keySelector)
        {
            List<Block> sortedList = this.OrderBy(keySelector).ToList();
            for (int i = 0; i < sortedList.Count(); i++)
            {
                this.Move(this.IndexOf(sortedList[i]), i);
            }
        }
    }

    /// <summary>
    /// 功能块列表管理器管理的功能块信息。
    /// </summary>
    public class Block : DependencyObject, INotifyPropertyChanged, IComparable
    {
        public Block()
        {
        }

        public Block(string blockName, string blockType, string planName, string cpu, string sourceLibrary, string sourceLibraryVersion, string runtimeTask, string runtimeGroup, string runSeqNum)
        {
            this.BlockName = blockName;
            this.BlockType = blockType;
            this.PlanName = planName;
            this.CPU = cpu;
            this.SourceLibraryName = sourceLibrary;
            this.SourceLibraryVersion = sourceLibraryVersion;
            this.RuntimeTask = runtimeTask;
            this.RuntimeGroup = runtimeGroup;
            this.RunSeqNum = runSeqNum;
        }

        public XmlNode GetXmlBlockNode(XmlDocument doc)
        {
            XmlElement Block = doc.CreateElement("Block");

            XmlElement BlockName = doc.CreateElement("BlockName");
            BlockName.InnerText = this.BlockName;
            Block.AppendChild(BlockName);

            XmlElement BlockType = doc.CreateElement("BlockType");
            BlockType.InnerText = this.BlockType;
            Block.AppendChild(BlockType);

            XmlElement PlanName = doc.CreateElement("PlanName");
            PlanName.InnerText = this.PlanName;
            Block.AppendChild(PlanName);

            XmlElement CPU = doc.CreateElement("CPU");
            CPU.InnerText = this.CPU;
            Block.AppendChild(CPU);

            XmlElement SourceLibrary = doc.CreateElement("SourceLibrary");
            SourceLibrary.InnerText = this.SourceLibraryName;
            Block.AppendChild(SourceLibrary);

            XmlElement SourceLibraryVersion = doc.CreateElement("SourceLibraryVersion");
            SourceLibraryVersion.InnerText = this.SourceLibraryVersion;
            Block.AppendChild(SourceLibraryVersion);

            XmlElement RuntimeTask = doc.CreateElement("RuntimeTask");
            RuntimeTask.InnerText = this.RuntimeTask;
            Block.AppendChild(RuntimeTask);

            XmlElement RuntimeGroup = doc.CreateElement("RuntimeGroup");
            RuntimeGroup.InnerText = this.RuntimeGroup;
            Block.AppendChild(RuntimeGroup);

            XmlElement RunSeqNum = doc.CreateElement("RunSeqNum");
            RunSeqNum.InnerText = this.RunSeqNum;
            Block.AppendChild(RunSeqNum);

            return Block;
        }

        private string blockName = string.Empty;
        private string planName = string.Empty;
        private string cpu = string.Empty;
        private string blockType = string.Empty;
        private string sourceLibrary = string.Empty;
        private string sourceLibraryVersion = string.Empty;
        private string runtimeTask = string.Empty;
        private string runtimeGroup = string.Empty;
        private string runSeqNum = string.Empty;

        public string BlockName
        {
            get { return this.blockName; }
            set
            {
                if (value != this.blockName)
                {
                    this.blockName = value;
                    NotifyPropertyChanged("BlockName");
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

        public string CPU
        {
            get { return this.cpu; }
            set
            {
                if (value != this.cpu)
                {
                    this.cpu = value;
                    NotifyPropertyChanged("CPU");
                }
            }
        }

        public string SourceLibraryName
        {
            get { return this.sourceLibrary; }
            set
            {
                if (value != this.sourceLibrary)
                {
                    this.sourceLibrary = value;
                    NotifyPropertyChanged("SourceLibraryName");
                    NotifyPropertyChanged("SourceLibraryInfo");
                }
            }
        }

        public string SourceLibraryVersion
        {
            get { return this.sourceLibraryVersion; }
            set
            {
                if (value != this.sourceLibraryVersion)
                {
                    this.sourceLibraryVersion = value;
                    NotifyPropertyChanged("SourceLibraryVersion");
                    NotifyPropertyChanged("SourceLibraryInfo");
                }
            }
        }

        public string SourceLibraryInfo
        {
            get { return this.sourceLibrary + " (" + this.sourceLibraryVersion + ")"; }
        }

        public string RuntimeTask
        {
            get { return this.runtimeTask; }
            set
            {
                if (value != this.runtimeTask)
                {
                    this.runtimeTask = value;
                    NotifyPropertyChanged("RuntimeTask");
                }
            }
        }

        public string RuntimeGroup
        {
            get { return this.runtimeGroup; }
            set
            {
                if (value != this.runtimeGroup)
                {
                    this.runtimeGroup = value;
                    NotifyPropertyChanged("RuntimeGroup");
                }
            }
        }

        public string RunSeqNum
        {
            get { return this.runSeqNum; }
            set
            {
                if (value != this.runSeqNum)
                {
                    this.runSeqNum = value;
                    NotifyPropertyChanged("RunSeqNum");
                }
            }
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
            Block b = obj as Block;

            if (b == null)
            {
                return 0;
            }

            //for sorting; sort by: (1) CPU, (2) plan name, (3) block name
            if (b.CPU.Equals(this.CPU, StringComparison.OrdinalIgnoreCase))
            {
                if (b.PlanName.Equals(this.PlanName, StringComparison.OrdinalIgnoreCase))
                {
                    return this.BlockName.CompareTo(b.BlockName);
                }
                else
                {
                    return this.PlanName.CompareTo(b.PlanName);
                }
            }
            else
            {
                return this.CPU.CompareTo(b.CPU);
            }
        }
    }
}

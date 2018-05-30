/// <summary>
/// @file   CompoundBlockManager.cs
///	@brief  ViGET 工程的组合功能块列表管理器。
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
using System.Xml;
using System.Runtime.InteropServices;
using System.Globalization;
using System.IO;

namespace Dothan.Manager
{
    /// <summary>
    /// ViGET 工程的组合功能块列表管理器，每个工程有一个这样的对象。
    /// </summary>
    public class CompoundBlockManager : ICompoundBlockManager, IViGETManager
    {
        /// <summary>
        /// Object info is changed or not.
        /// </summary>
        public bool Dirty { get; set; }

        public CompoundBlockManager(string projectPath)
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
        private CompoundBlockList blockList = new CompoundBlockList();

        private string XRefListPath;

        public void AddBlock(string Name, string PlanName, string ChartPath, string CPU, string Prototype, string CPUType, long globalId)
        {
            blockList.Add(new CompoundBlock(Name, PlanName, ChartPath, CPU, Prototype, CPUType, globalId));
        }

        public CompoundBlock GetBlock(string Name, string PlanName)
        {
            foreach (var block in blockList)
            {
                if (block.Name.Equals(Name, StringComparison.OrdinalIgnoreCase) &&
                    block.PlanName.Equals(PlanName, StringComparison.OrdinalIgnoreCase))
                    return block;
            }
            return null;
        }

        public void RemoveBlock(string Name, string PlanName)
        {
            foreach (var item in blockList)
            {
                if (item.Name.Equals(Name, StringComparison.OrdinalIgnoreCase) &&
                    item.PlanName.Equals(PlanName, StringComparison.OrdinalIgnoreCase))
                {
                    blockList.Remove(item);
                    break;
                }
            }
        }

        public void RenameBlock(string Name, string PlanName, string newName)
        {
            foreach (var item in blockList)
            {
                if (item.Name.Equals(Name, StringComparison.OrdinalIgnoreCase) &&
                    item.PlanName.Equals(PlanName, StringComparison.OrdinalIgnoreCase))
                {
                    item.Name = newName;

                    // set dirty flag
                    Dirty = true;

                    break;
                }
            }
        }

        public void RemoveBlocks(string PlanName)
        {
            var itemsToRemove = blockList.Where(b => b.PlanName.Equals(PlanName, StringComparison.OrdinalIgnoreCase)).ToList();
            foreach (var itemToRemove in itemsToRemove)
            {
                blockList.Remove(itemToRemove);
            }
        }

        public void Init(XmlNodeList blockListXml)
        {
            //clean the list
            blockList.Clear();

            foreach (XmlNode item in blockListXml)
            {
                long id = 0;
                long.TryParse(item.SelectSingleNode("GlobalID").InnerText, NumberStyles.Number, CultureInfo.InvariantCulture, out id);
                blockList.Add(new CompoundBlock(
                            item.SelectSingleNode("Name").InnerText,
                            item.SelectSingleNode("PlanName").InnerText,
                            item.SelectSingleNode("ChartPath").InnerText,
                            item.SelectSingleNode("CPU").InnerText,
                            item.SelectSingleNode("Prototype").InnerText,
                            item.SelectSingleNode("CPUType").InnerText,
                            id)
                        );
            }

            // reset dirty flag
            Dirty = false;
        }

        public void RemovePlan(string planName)
        {
            //TO DO: IMPLEMENT - REQUIRED???
        }

        public void RenamePlan(string oldName, string newName)
        {
            var itemsToRename = blockList.Where(b => b.PlanName.Equals(oldName, StringComparison.OrdinalIgnoreCase)).ToList();
            foreach (var item in itemsToRename)
            {
                item.PlanName = newName;
            }

            // set dirty flag
            Dirty = true;
        }

        public void RenameCompoundBlock(string blockName, string planName, string newName)
        {
            //TO DO: IMPLEMENT - REQUIRED???
        }

        public void ChangeResource(string resource, string resourceType, string PlanName)
        {
            var itemsToRename = blockList.Where(b => b.PlanName.Equals(PlanName, StringComparison.OrdinalIgnoreCase)).ToList();
            foreach (var item in itemsToRename)
            {
                item.CPU = resource;
                item.CPUType = resourceType;
            }

            // set dirty flag
            Dirty = true;
        }

        public void RenameResource(string oldName, string newName)
        {
            var itemsToRename = blockList.Where(b => b.CPU.Equals(oldName)).ToList();
            foreach (var item in itemsToRename)
            {
                item.CPU = newName;

                // set dirty flag
                Dirty = true;
            }
        }

        public void CleanBlockList()
        {
            //clean the list
            blockList.Clear();
        }

        public void GetCompBlockList(ref CompoundBlockList list)
        {
            list = blockList;
        }

        public List<CompoundBlock> GetCompBlockList2()
        {
            List<CompoundBlock> list = new List<CompoundBlock>();

            foreach (CompoundBlock b in blockList)
            {
                list.Add(b);
            }

            list.Sort();

            return list;
        }
    }

    /// <summary>
    /// 组合功能块列表。支持根据指定的排序函数排序的功能。
    /// </summary>
    [ClassInterface(ClassInterfaceType.None)]
    public class CompoundBlockList : ObservableCollection<CompoundBlock>
    {
        public void SortBy(Func<CompoundBlock, string> keySelector)
        {
            List<CompoundBlock> sortedList = this.OrderBy(keySelector).ToList();
            for (int i = 0; i < sortedList.Count(); i++)
            {
                this.Move(this.IndexOf(sortedList[i]), i);
            }
        }
    }

    /// <summary>
    /// 组合功能块列表管理器管理的组合功能块信息。
    /// </summary>
    public class CompoundBlock : IComparable
    {
        public CompoundBlock()
        {
        }

        public CompoundBlock(string sName, string sPlanName, string sChartPath, string sCPU, string sPrototype, string sCPUType, long globalId)
        {
            this.Name = sName;
            this.PlanName = sPlanName;
            this.ChartPath = sChartPath;
            this.CPU = sCPU;
            this.Prototype = sPrototype;
            this.CPUType = sCPUType;
            this.GlobalID = globalId;
        }

        public XmlNode GetXmlBlockNode(XmlDocument doc)
        {
            XmlElement Block = doc.CreateElement("CompoundBlock");

            XmlElement Name = doc.CreateElement("Name");
            Name.InnerText = this.Name;
            Block.AppendChild(Name);

            XmlElement PlanName = doc.CreateElement("PlanName");
            PlanName.InnerText = this.PlanName;
            Block.AppendChild(PlanName);

            XmlElement ChartPath = doc.CreateElement("ChartPath");
            ChartPath.InnerText = this.ChartPath;
            Block.AppendChild(ChartPath);

            XmlElement CPU = doc.CreateElement("CPU");
            CPU.InnerText = this.CPU;
            Block.AppendChild(CPU);

            XmlElement Prototype = doc.CreateElement("Prototype");
            Prototype.InnerText = this.Prototype;
            Block.AppendChild(Prototype);

            XmlElement CPUType = doc.CreateElement("CPUType");
            CPUType.InnerText = this.CPUType;
            Block.AppendChild(CPUType);

            XmlElement GlobalID = doc.CreateElement("GlobalID");
            GlobalID.InnerText = this.GlobalID.ToString();
            Block.AppendChild(GlobalID);

            return Block;
        }

        public string Name { get; set; }
        public string PlanName { get; set; }
        public string ChartPath { get; set; }
        public string CPU { get; set; }
        public string Prototype { get; set; }
        public string CPUType { get; set; }
        public long GlobalID { get; set; }

        public int CompareTo(object obj)
        {
            CompoundBlock b = obj as CompoundBlock;

            if (b == null)
            {
                return 0;
            }

            //for sorting; sort by: (1) CPU, (2) plan name, (3) compound block name
            if (b.CPU.Equals(this.CPU, StringComparison.OrdinalIgnoreCase))
            {
                if (b.PlanName.Equals(this.PlanName, StringComparison.OrdinalIgnoreCase))
                {
                    return this.Name.CompareTo(b.Name);
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

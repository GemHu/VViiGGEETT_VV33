/// <summary>
/// @file   IOAddressManager.cs
///	@brief  ViGET 工程的硬件配置器中的 IO Address 管脚列表管理器。
/// @author	DothanTech 刘伟宏
/// 
/// Copyright(C) 2011~2014, DothanTech. All rights reserved.
/// </summary>

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Windows;
using System.Windows.Threading;
using System.ComponentModel;
using System.Runtime.InteropServices;

using Dothan.Helpers;

namespace Dothan.Manager
{
    /// <summary>
    /// ViGET 工程的硬件配置器中的 IO Address 管脚列表管理器，每个工程有一个这样的对象。
    /// </summary>
    public class IOAddressManager : DependencyObject, IIOAddressManager, IViGETManager, INotifyPropertyChanged
    {
        /// <summary>
        /// Object info is changed or not.
        /// </summary>
        public bool Dirty { get; set; }

        //public static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private string ProjectPath;

        private IOConnectionList listOfIOConnections = null;

        private IOAddressList listOfAvailableIOAddress = null;

        private HWConfigurationProxy proxyOfHWConfig = null;

        void value_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Dirty = true;
        }

        // persistent list of existing IO connection
        public IOConnectionList ListOfIOConnections
        {
            get { return this.listOfIOConnections; }
            set
            {
                if (value != this.listOfIOConnections)
                {
                    // spy CollectionChanged event
                    if (this.listOfIOConnections != null)
                        this.listOfIOConnections.CollectionChanged -= value_CollectionChanged;
                    if (value != null)
                        value.CollectionChanged += value_CollectionChanged;
                    // set dirty flag
                    Dirty = true;

                    this.listOfIOConnections = value;
                    NotifyPropertyChanged("ListOfIOConnections");
                }
            }
        }

        public void GetIOConnectionList(ref IOConnectionList list)
        {
            list = ListOfIOConnections;
        }

        internal List<IOConnection> GetIOConnectionList2()
        {
            List<IOConnection> list = new List<IOConnection>();

            foreach (IOConnection c in listOfIOConnections)
            {
                list.Add(c);
            }

            list.Sort();

            return list;
        }

        public IOAddressList ListOfAvailableIOAddress
        {
            get { return this.listOfAvailableIOAddress; }
            set
            {
                if (value != this.listOfAvailableIOAddress)
                {
                    // spy CollectionChanged event
                    if (this.listOfAvailableIOAddress != null)
                        this.listOfAvailableIOAddress.CollectionChanged -= value_CollectionChanged;
                    if (value != null)
                        value.CollectionChanged += value_CollectionChanged;

                    // set dirty flag
                    Dirty = true;

                    this.listOfAvailableIOAddress = value;
                    NotifyPropertyChanged("ListOfAvailableIOAddress");
                }
            }
        }

        public IOAddressManager(string projectPath)
        {
            this.ProjectPath = projectPath;

            ListOfAvailableIOAddress = new IOAddressList();
            ListOfIOConnections = new IOConnectionList();

            proxyOfHWConfig = new HWConfigurationProxy(ProjectPath);

            #region Get Hardware Config File of Project

            string hwConfigFile = null;

            // Search in project file
            try
            {
                string[] files = System.IO.Directory.GetFiles(projectPath, @"*.VAR");
                if (files != null && files.Count() > 0)
                {
                    IniFile iniFile = new IniFile(files[0]);
                    hwConfigFile = iniFile.GetValueS("STATION", "FILE0");
                    if (!string.IsNullOrEmpty(hwConfigFile))
                    {
                        hwConfigFile = projectPath.PathCombine(hwConfigFile + ".hwconfig");
                    }
                }
            }
            catch (Exception)
            {
            }

            // Search .hwconfig file
            if (string.IsNullOrEmpty(hwConfigFile))
            {
                try
                {
                    string[] files = System.IO.Directory.GetFiles(projectPath, @"*.hwconfig");
                    if (files != null && files.Count() > 0)
                    {
                        hwConfigFile = files[0];
                    }
                }
                catch (Exception)
                {
                }
            }

            #endregion

            this.SetHWConfigFile(hwConfigFile);

            // reset dirty flag
            Dirty = false;
        }

        public void Init(XmlNodeList IOConnectionList)
        {
            ListOfIOConnections.Clear();

            foreach (XmlNode item in IOConnectionList)
            {
                string uniqueID = item.SelectSingleNode("UniqueID").InnerText;
                string connectorPath = item.SelectSingleNode("ConnectorPath").InnerText;

                IOAddress address = GetAddressItem(uniqueID);
                if (address != null)//in case of inconsistency e.g. connection with invalide address stored in xref.data
                    ListOfIOConnections.Add(new IOConnection(address, connectorPath));
            }

            // reset dirty flag
            Dirty = false;
        }

        public bool IsAddressUsed(string uniqueID)
        {
            foreach (var connection in ListOfIOConnections)
            {
                if (connection.Address.UniqueID == uniqueID)
                    return true;
            }

            return false;
        }

        public void AddIOConnection(string uniqueID, string connectorPath)
        {
            RemoveIOConnection(uniqueID, connectorPath);

            IOAddress item = ListOfAvailableIOAddress.GetIOAddress(uniqueID);
            if (item != null)
                ListOfIOConnections.Add(new IOConnection(item, connectorPath));
        }

        public void RemoveIOConnection(string uniqueID, string connectorPath)
        {
            //lsu 20120410: comparing TargetConnector instead of ConnectorPath. 
            //              After allowing moving block over page border, the ConnectorPath of margin connector may change (page number)
            int iStart = connectorPath.LastIndexOf('\\', connectorPath.LastIndexOf('\\') - 1);
            string targetConnector = connectorPath.Substring(iStart + 1, connectorPath.Length - 1 - iStart).Trim();

            //lsu 20120524: uniqueness can be assured with plan name
            iStart = connectorPath.IndexOf('\\');
            int iEnd = connectorPath.IndexOf('(', iStart + 1);
            string planName = connectorPath.Substring(iStart + 1, iEnd - iStart - 2).Trim();

            var itemsToRemove = ListOfIOConnections.Where(a =>
                a.ConnectorPath.Equals(connectorPath, StringComparison.OrdinalIgnoreCase)).ToList();
            foreach (var item in itemsToRemove)
            {
                ListOfIOConnections.Remove(item);
            }
        }

        public IOConnection GetIOConnection(string uniqueID, string connectorPath)
        {
            return this.listOfIOConnections.Where(o => o.Address.UniqueID.Equals(uniqueID, StringComparison.OrdinalIgnoreCase) && o.ConnectorPath.Equals(connectorPath, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
        }

        public List<IOConnection> GetIOConnections(string uniqueID)
        {
            return this.listOfIOConnections.Where(o => o.Address.UniqueID.Equals(uniqueID, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public bool SetHWConfigFile(string fileName)
        {
            if (proxyOfHWConfig == null) return false;
            if (string.IsNullOrEmpty(fileName)) return false;

            try
            {
                IOAddressList newIOAddressList = proxyOfHWConfig.ParseHWConfigFile(fileName);
                Dictionary<string, IOAddress> newIOAddressDic = newIOAddressList.ToDictionary();
                IOAddress[] listOfAvailableIOAddress = ListOfAvailableIOAddress.ToArray<IOAddress>();

                foreach (var ioAddress in listOfAvailableIOAddress)
                {
                    if (newIOAddressDic.ContainsKey(ioAddress.UniqueID))
                    { // still exist
                        IOAddress newAddress = newIOAddressDic[ioAddress.UniqueID];

                        newIOAddressDic.Remove(ioAddress.UniqueID);

                        if (ListOfAvailableIOAddress.InsertNewIOAddress(newAddress))
                        {
                            if (IOAddressChanged != null && IsAddressUsed(ioAddress.UniqueID))
                                IOAddressChanged(newAddress.ToString() + " is updated.");
                        }
                    }
                    else
                    { // removed
                        ListOfAvailableIOAddress.Remove(ioAddress);

                        if (IOAddressChanged != null && IsAddressUsed(ioAddress.UniqueID))
                            IOAddressChanged(ioAddress.ToString() + " is not valid any more.");

                        // remove connection(s) of this IOAddress(by UniqueID)
                        var connections = ListOfIOConnections.Where(a =>
                            a.Address.UniqueID == ioAddress.UniqueID);
                        foreach (var conn in connections)
                            ListOfIOConnections.Remove(conn);
                    }
                }

                //add new IO addresses into the list
                foreach (var rest in newIOAddressDic.Values)
                {
                    ListOfAvailableIOAddress.InsertNewIOAddress(rest);
                }

                return true;
            }
            catch (Exception ee)
            {
                Trace.WriteLine("###[" + ee.Message + "]; Exception : " + ee.Source);
                Trace.WriteLine("###" + ee.StackTrace);
            }

            return false;
        }

        public List<IOAddress> GetAddressList(string cpu, string blockType)
        {
            //string ret = string.Empty;
            List<IOAddress> ret = new List<IOAddress>();
            foreach (IOAddress item in ListOfAvailableIOAddress)
            {
                //determine which I/O address can be applied to current connector according to the CPU name and function block type
                if (item.AllowedFunctionBlockTypes.Contains(blockType) &&
                    proxyOfHWConfig.IsCPUUsed(cpu) &&
                    item.AllowedVMESlots.Contains(proxyOfHWConfig.GetSlotForCPU(cpu)) &&
                    item.SymbolicName != string.Empty)
                {
                    ret.Add(item);
                }
            }
            return ret;
        }

        public IOAddress GetAddress(string uniqueID)
        {
            return GetAddressItem(uniqueID);
        }

        private IOAddress GetAddressItem(string uniqueID)
        {
            return ListOfAvailableIOAddress.GetIOAddress(uniqueID);
        }

        public void GetIOAddressList(ref IOAddressList list)
        {
            list = ListOfAvailableIOAddress;
        }

        public delegate void IOAddressChangedHandler(string text);
        public event IOAddressChangedHandler IOAddressChanged;

        public void RemoveInvalidIOConnections(string planName, string usedIOAddressesInPlan)
        {
            var itemsToCheck = ListOfIOConnections.Where(b =>
                b.ConnectorPath.Length > 0 && b.GetPlanName().Equals(planName, StringComparison.OrdinalIgnoreCase)).ToList();
            string[] arrayUsedIOAddressesInPlan = usedIOAddressesInPlan.Split(new char[] { '|' });
            foreach (var item in itemsToCheck)
            {
                if (!arrayUsedIOAddressesInPlan.Contains<string>(item.Address.UniqueID))
                {
                    item.ConnectorPath = string.Empty;

                    // set dirty flag
                    Dirty = true;
                }
            }
        }

        public void RemoveIOConnectionsInPlan(string planName)
        {
            //IO address currently related to plan "planName": ConnectorPath removed
            var itemsToRemove = ListOfIOConnections.Where(b =>
                b.GetPlanName().Equals(planName, StringComparison.OrdinalIgnoreCase)).ToList();
            foreach (var itemToRemove in itemsToRemove)
            {
                ListOfIOConnections.Remove(itemToRemove);
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
    }

    /// <summary>
    /// IO Address 管脚列表。支持根据指定的排序函数排序的功能。
    /// </summary>
    [ClassInterface(ClassInterfaceType.None)]
    public class IOAddressList : ObservableCollection<IOAddress>
    {
        public void SortBy(Func<IOAddress, string> keySelector)
        {
            List<IOAddress> sortedList = this.OrderBy(keySelector).ToList();
            for (int i = 0; i < sortedList.Count(); i++)
            {
                this.Move(this.IndexOf(sortedList[i]), i);
            }
        }

        public Dictionary<string, IOAddress> ToDictionary()
        {
            Dictionary<string, IOAddress> dic = new Dictionary<string, IOAddress>();
            foreach (var item in this)
            {
                dic[item.UniqueID] = item;
            }
            return dic;
        }

        public IOAddress GetIOAddress(string uniqueID)
        {
            foreach (var item in this)
            {
                if (item.UniqueID == uniqueID)
                    return item;
            }
            return null;
        }

        //return true if an item with uniqueID exists and is deleted
        //return false no item with uniqueID is found
        public bool RemoveIOAddress(string uniqueID)
        {
            var toRemove = GetIOAddress(uniqueID);
            if (toRemove != null)
            {
                Remove(toRemove);
                return true;
            }
            return false;
        }

        //return true if an item with uniqueID exists and is updated (if changed)
        //return false no item with uniqueID is found, or the new item is the same as old item. the new item is simply added into the list
        public bool InsertNewIOAddress(IOAddress newAddress)
        {
            var existingIOAddress = GetIOAddress(newAddress.UniqueID);
            if (existingIOAddress != null)
            {
                // check if the address contents have changed
                if (!newAddress.ToString().Equals(existingIOAddress.ToString()) ||
                    !newAddress.AllowedCPUTypes.SequenceEqual(existingIOAddress.AllowedCPUTypes) ||
                    !newAddress.AllowedFunctionBlockTypes.SequenceEqual(existingIOAddress.AllowedFunctionBlockTypes) ||
                    !newAddress.AllowedVMESlots.SequenceEqual(existingIOAddress.AllowedVMESlots))
                {
                    SetItem(IndexOf(existingIOAddress), newAddress);
                    return true;
                }
                else
                    return false;
            }

            Add(newAddress);
            return false;
        }
    }

    /// <summary>
    /// IO Address 管脚列表管理器管理的 IO Address 管脚信息。
    /// </summary>
    public class IOAddress : INotifyPropertyChanged
    {
        private string uniqueID = string.Empty;
        private string vMEAddress = string.Empty;
        private string providerName = string.Empty;
        private string symbolicName = string.Empty;
        private string displayName = string.Empty;

        public string UniqueID
        {
            get { return this.uniqueID; }
            set
            {
                if (value != this.uniqueID)
                {
                    this.uniqueID = value;
                    NotifyPropertyChanged("UniqueID");
                }
            }
        }

        public string VMEAddress
        {
            get { return this.vMEAddress; }
            set
            {
                if (value != this.vMEAddress)
                {
                    this.vMEAddress = value;
                    NotifyPropertyChanged("VMEAddress");
                }
            }
        }

        public string SymbolicName
        {
            get { return this.symbolicName; }
            set
            {
                if (value != this.symbolicName)
                {
                    this.symbolicName = value;
                    NotifyPropertyChanged("SymbolicName");
                }
            }
        }

        public string DisplayName
        {
            get { return this.displayName; }
            set
            {
                if (value != this.displayName)
                {
                    this.displayName = value;
                    NotifyPropertyChanged("DisplayName");
                }
            }
        }

        public string ProviderName
        {
            get { return this.providerName; }
            set
            {
                if (value != this.providerName)
                {
                    this.providerName = value;
                    NotifyPropertyChanged("ProviderName");
                }
            }
        }
        public List<string> AllowedCPUTypes { get; set; }
        public List<string> AllowedFunctionBlockTypes { get; set; }
        public List<string> AllowedVMESlots { get; set; }

        public override string ToString()
        {
            return this.UniqueID + "*" + this.VMEAddress + "*" /*+ this.AddressType + "*"*/ + this.SymbolicName + "*" + this.DisplayName + "*" + this.ProviderName;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }

    /// <summary>
    /// IO Address 管脚连线列表。支持根据指定的排序函数排序的功能。
    /// </summary>
    [ClassInterface(ClassInterfaceType.None)]
    public class IOConnectionList : ObservableCollection<IOConnection>
    {
        public void SortBy(Func<IOConnection, string> keySelector)
        {
            List<IOConnection> sortedList = this.OrderBy(keySelector).ToList();
            for (int i = 0; i < sortedList.Count(); i++)
            {
                this.Move(this.IndexOf(sortedList[i]), i);
            }
        }
    }

    /// <summary>
    /// IO Address 管脚列表管理器管理的 IO Address 管脚连线信息。
    /// </summary>
    public class IOConnection : INotifyPropertyChanged, IComparable
    {
        public IOConnection(IOAddress address, string connectorPath)
        {
            Address = address;
            ConnectorPath = connectorPath;
        }

        public XmlNode GetXmlConnectionNode(XmlDocument doc)
        {
            if (this.ConnectorPath.Length == 0)
                return null;

            XmlElement Connection = doc.CreateElement("Connection");

            XmlElement UniqueID = doc.CreateElement("UniqueID");
            UniqueID.InnerText = this.Address.UniqueID;
            Connection.AppendChild(UniqueID);

            XmlElement ConnectorPath = doc.CreateElement("ConnectorPath");
            ConnectorPath.InnerText = this.ConnectorPath;
            Connection.AppendChild(ConnectorPath);

            return Connection;
        }

        private IOAddress address = null;

        private string connectorPath = string.Empty;

        public IOAddress Address
        {
            get { return this.address; }
            set
            {
                if (value != this.address)
                {
                    this.address = value;
                    NotifyPropertyChanged("Address");
                }
            }
        }

        public string ConnectorPath
        {
            get { return this.connectorPath; }
            set
            {
                if (value != this.connectorPath)
                {
                    this.connectorPath = value;
                    NotifyPropertyChanged("ConnectorPath");
                }
            }
        }

        public string GetPlanName()
        {
            if (ConnectorPath.Length == 0)
                return string.Empty;

            int iStart = ConnectorPath.IndexOf('\\');
            int iEnd = ConnectorPath.IndexOf('(', iStart + 1);
            return ConnectorPath.Substring(iStart + 1, iEnd - iStart - 2).Trim();
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
            IOConnection c = obj as IOConnection;

            if (c == null)
            {
                return 0;
            }

            //for sorting; sort by: (1) symbolic name
            return this.Address.SymbolicName.CompareTo(c.Address.SymbolicName);
        }
    }

    /// <summary>
    /// 从硬件配置器文件中解析出 IO Address 的代理类。
    /// </summary>
    class HWConfigurationProxy
    {
        private Dictionary<string, string> mapOfSlotToCPU = new Dictionary<string, string>();

        public HWConfigurationProxy(string ProjectPath)
        {
        }

        public IOAddressList ParseHWConfigFile(string fileName)
        {
            //parse hwconfig and generate a new IO address list
            IOAddressList newIOAddressList = new IOAddressList();

            if (string.IsNullOrEmpty(fileName))
                return newIOAddressList;
            string HWConfigurationPath = fileName;

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(HWConfigurationPath);
                XmlNode root = doc.DocumentElement;
                XmlNodeList IOAddressList = root.SelectNodes("..//hwModule//properties//moduleSpecificProperties//property//address");

                foreach (XmlNode node in IOAddressList)
                {
                    IOAddress newAddress = new IOAddress();

                    newAddress.SymbolicName = node.ParentNode.SelectSingleNode("value").InnerText;
                    newAddress.DisplayName = node.ParentNode.SelectSingleNode("displayName").InnerText;
                    //obsolete
                    //newAddress.AddressType = node.Attributes["dataType"].Value;
                    newAddress.VMEAddress = node.SelectSingleNode("vmeBusAddress").InnerText;
                    newAddress.UniqueID = node.SelectSingleNode("uniqueAddressID").InnerText;
                    newAddress.AllowedCPUTypes = node.SelectSingleNode("allowedCPUTypes").InnerText.Split(new char[] { ';' }).ToList();
                    newAddress.AllowedFunctionBlockTypes = node.SelectSingleNode("allowedFunctionBlockTypes").InnerText.Split(new char[] { ';' }).ToList();
                    newAddress.AllowedVMESlots = node.SelectSingleNode("allowedVMESlots").InnerText.Split(new char[] { ';' }).ToList();

                    //find in "commonProperties" a property named "Name" and take as the ProviderName
                    XmlNodeList commonProperties = node.ParentNode.ParentNode.ParentNode.SelectNodes("commonProperties//property");
                    foreach (XmlNode commonProperty in commonProperties)
                    {
                        if (commonProperty.Attributes["name"].Value == "Name")
                        {
                            newAddress.ProviderName = commonProperty.SelectSingleNode("value").InnerText;
                            break;
                        }
                    }

                    newIOAddressList.Add(newAddress);
                }

                //mapping cpu name to slot number
                mapOfSlotToCPU.Clear();
                XmlNodeList moduleList = root.SelectNodes("..//hwModule");
                foreach (XmlNode module in moduleList)
                {
                    XmlNodeList commonPropertyList = module.SelectNodes("properties//commonProperties//property");
                    string CPUName = string.Empty;
                    string slotNumber = string.Empty;
                    foreach (XmlNode commonProperty in commonPropertyList)
                    {
                        if (commonProperty.Attributes["name"].Value.Equals("Name"))
                        {
                            CPUName = commonProperty.SelectSingleNode("value").InnerText;
                        }
                        if (commonProperty.Attributes["name"].Value.Equals("Slot"))
                        {
                            slotNumber = commonProperty.SelectSingleNode("value").InnerText;
                        }
                    }
                    if (!slotNumber.Equals(string.Empty) && !CPUName.Equals(string.Empty) && !mapOfSlotToCPU.ContainsKey(CPUName))
                        mapOfSlotToCPU.Add(CPUName, slotNumber);
                }

                return newIOAddressList;
            }
            catch (Exception)
            {
                return newIOAddressList;
            }
        }

        public bool IsCPUUsed(string CPU)
        {
            return mapOfSlotToCPU.ContainsKey(CPU);
        }

        public string GetSlotForCPU(string CPU)
        {
            return mapOfSlotToCPU[CPU];
        }
    }
}

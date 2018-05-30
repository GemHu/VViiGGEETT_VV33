using System.Collections.Generic;

namespace Dothan.Manager
{
    public interface IXRef
    {
        IIOAddressManager GetIOAddressManager();
        IBlockListManager GetBlockListManager();
        ISharedMemoryManager GetSharedMemoryManager();
        ICompoundBlockManager GetCompoundBlockManager();
        IBuildListManager GetBuildListManager();

        string InitXRef(string projectPath);
        void ReloadXRef();
        void SaveXRefList(bool createBackup);
        void CleanXRefList();

        void RemovePlan(string planName);
        void RenamePlan(string oldName, string newName);
        void RenameCompoundBlock(string blockName, string planName, string newName);
        void ChangeResource(string resource, string resourceType, string planName);
        void RenameResource(string oldName, string newName);

        void UpdateRuntimeInformation(string blockName, string planName, string runtimeTask, string runtimeGroup, string runSeqNum);

        /// <summary>
        /// Set hardware configuration file name.
        /// </summary>
        /// <param name="fileName">Hardware config file's full path name.</param>
        /// <returns>Success or not. </returns>
        bool SetHWConfigFile(string fileName);
    }

    public interface IIOAddressManager
    {
        event IOAddressManager.IOAddressChangedHandler IOAddressChanged;

        void GetIOAddressList(ref IOAddressList IOAddrList);
        /// <summary>
        ///		Request for a set of I/O addresses for a given CPU and block type.
        /// </summary>
        /// <param name="cpu">CPU name which want to use I/O addresses.</param>
        /// <param name="blockType">Function block type which want to connect I/O addresses.</param>
        /// <returns>
        ///		Returns the list of I/O address data sets as a string. The data sets are separated by a '|'. 
        ///		The values of a data set are separated by a '*' and contained in the following order: uniqueID*VMEbusAddr*dataType*symbolicName*displayName*IOboard. 
        /// </returns>
        List<IOAddress> GetAddressList(string cpu, string blockType);
        IOAddress GetAddress(string uniqueID);
        bool IsAddressUsed(string uniqueID);
        void AddIOConnection(string uniqueID, string connectorPath);
        void RemoveIOConnection(string uniqueID, string connectorPath);
        void RemoveInvalidIOConnections(string planName, string usedIOConnectionsInPlan);
        void RemoveIOConnectionsInPlan(string planName);
        void GetIOConnectionList(ref IOConnectionList IOAddrList);
        IOConnection GetIOConnection(string uniqueID, string connectorPath);
        List<IOConnection> GetIOConnections(string uniqueID);
    }

    public interface IBlockListManager
    {
        void AddBlock(string blockName,
                        string blockType,
                        string planName,
                        string cpu,
                        string sourceLibrary,
                        string sourceLibraryVersion,
                        string runtimeTask,
                        string runtimeGroup,
                        string runSeqNum);
        void RemoveBlock(string blockName, string planName);
        void RenameBlock(string blockName, string planName, string newName);
        bool IsAnyBlockFromLibraryUsed(string libraryName);

        bool IsAnyBlockFromLibraryUsed(string libraryName, string resourceName);
        void GetUsedBlockList(string libraryName, string resourceName, ref BlockList List);
        void ReplaceLibrary(string oldName, string oldVersion, string newName, string newVersion, string resourceName);

        void GetBlockList(ref BlockList blockList);
        void RemoveBlocks(string planName);

        void RenamePlan(string oldName, string newName);
        void RenameCompoundBlock(string blockName, string planName, string newName);
        void ChangeResource(string resource, string resourceType, string planName);
        void RenameResource(string oldName, string newName);
    }

    public interface ISharedMemoryManager
    {
        void AddShmConnection(string VarName, string ConnectorPath, int IsReadingSignal, string TaskName, string BlockType);
        void RemoveShmConnection(string VarName, string ConnectorPath);
        void GetShmConnectionList(ref SharedMemoryConnectionList list);
        void GetShmVariableList(ref SharedMemoryVariableList list);
        SharedMemoryVariable GetShmVar(string variableID);
        void RemoveShmConnections(string planName);

        void AddNewShmVarWithDefaultValue();
        void RemoveShmVar(string VarName);

        bool IsAnyWriterInCPU(string sSharedMemoryVariable, string sCPUName);
        bool IsAnyReaderInCPU(string sSharedMemoryVariable, string sCPUName);
        string GetSharedMemoryWriter(string sSharedMemoryVariable);
        string GetSharedMemoryReader(string sSharedMemoryVariable);

        string GetSharedMemoryVariableList();
        bool IsFastSignal(string VarName);
        bool IsConnectable(string VarName);

        int GetVariablesCount();
        string GetVariableName(int index);
        string GetVariableType(int index);
        byte HasWriteAccess(string VarName);
        string GetVariableUsageString(string VarName);

        string SHMUsageValidityCheck();

        void RemovePlan(string planName);
        void RenamePlan(string oldName, string newName);
        void RenameBlock(string blockName, string planName, string newName);
        void ChangeResource(string resource, string resourceType, string planName);
        void RenameResource(string oldName, string newName);

        bool IsVariablesListValid();

        string GetSharedMemoryVariableDataType(string VarName);
    }

    public interface ICompoundBlockManager
    {
        void AddBlock(string Name, string PlanName, string ChartPath, string CPU, string Prototype, string CPUType, long globalId);
        CompoundBlock GetBlock(string Name, string PlanName);
        void RemoveBlock(string Name, string PlanName);
        void RenameBlock(string Name, string PlanName, string newName);
        void RemoveBlocks(string PlanName);

        void RemovePlan(string planName);
        void RenamePlan(string oldName, string newName);
        void RenameCompoundBlock(string blockName, string planName, string newName);
        void ChangeResource(string resource, string resourceType, string planName);
        void RenameResource(string oldName, string newName);

        void GetCompBlockList(ref CompoundBlockList list);
    }

    public interface IBuildListManager
    {
        void Register(string planPath, string resourceName);
        void Unregister(string planName);

        void RenamePlan(string oldName, string newName);
        void ChangeResource(string resource, string resourceType, string planName);
        void RenameResource(string oldName, string newName);

        string GetListOfNewPOEs(string resourceName);
    }
}

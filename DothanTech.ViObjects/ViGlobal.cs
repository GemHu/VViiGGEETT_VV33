/// <summary>
/// @file   ViGlobal.cs
///	@brief  ViGET 全局参数定义和设置。
/// @author	DothanTech 刘伟宏
/// 
/// Copyright(C) 2011~2014, DothanTech. All rights reserved.
/// </summary>

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Diagnostics;
using System.Collections;
using System.Collections.ObjectModel;

using Dothan.Helpers;

namespace Dothan.ViObject
{
    /// <summary>
    /// ViGET 全局参数定义和设置。
    /// </summary>
    public static class ViGlobal
    {
        #region Static Init

        static ViGlobal()
        {
            InitCache();
        }

        /// <summary>
        /// 初始化全局缓存。
        /// </summary>
        public static void InitCache()
        {
            ViGlobal._strProjODKPath = string.Empty;
            ViGlobal._strViGETPath = string.Empty;
            ViGlobal._strViGETV20Path = string.Empty;
            ViGlobal._strViGETV20ProgramDataPath = string.Empty;
            ViGlobal._strPackagesToLoad = string.Empty;
            //
            ViGlobal._hardwareTypes = null;
            ViGlobal._pouSources = null;
        }

        #endregion

        #region Global Registry
        /// <summary>
        /// 全局的配置信息操作对象。
        /// </summary>
        public static RegFile GlobalReg = new RegFile(Registry.CurrentUser, @"Software\DothanTech\ViGET\2.0");
        #endregion

        #region Project ODK Path
        // C:\Documents and Settings\All Users\Application Data\XJGC\ViGET\Openpcs.520\MODULES\XJ_CP3000.ini
        // [Module] PceDeviceName=IEC_IEC_NORM
        /// <summary>
        /// ViGET V2.0 全局文件所在的目录（就是常说的 OpenPCS.520 目录）
        /// </summary>
        public static string ProjODKPath
        {
            get
            {
                if (string.IsNullOrEmpty(_strProjODKPath))
                {
                    // Get IniPath from Visual CFC
                    RegFile reg = new RegFile(@"HKEY_LOCAL_MACHINE\SOFTWARE\DothanTech\ViGET");
                    string version = reg.GetValue("", "CurrentVersion", "2.0");
                    _strProjODKPath = reg.GetValue(version, "IniPath", "");

                    // Get IniPath from OpenPCS
                    if (string.IsNullOrEmpty(_strProjODKPath))
                    {
                        reg = new RegFile(@"HKEY_LOCAL_MACHINE\SOFTWARE\infoteam Software GmbH\OpenPCS");
                        version = reg.GetValue("CurrentVersion", "", "6.2.0");
                        _strProjODKPath = reg.GetValue(version + @"\IniPath", "", "");
                    }

                    // Get IniPath from Guess
                    if (string.IsNullOrEmpty(_strProjODKPath))
                    {
                        // For Win7 and newer version, ODK path is in ProgramData folder.
                        if ((Environment.OSVersion.Platform == PlatformID.Win32NT) &&
                            ((Environment.OSVersion.Version.Major == 6) && (Environment.OSVersion.Version.Minor >= 1) ||
                             (Environment.OSVersion.Version.Major > 6)))
                        {
                            _strProjODKPath = Environment.GetEnvironmentVariable(@"ProgramData")
                                        + @"\XJGC\ViGET\Openpcs.520\";
                        }
                        else
                        {
                            _strProjODKPath = Environment.GetEnvironmentVariable(@"ALLUSERSPROFILE")
                                        + @"\Application Data\XJGC\ViGET\Openpcs.520\";
                        }
                    }

                    Trace.WriteLine("strProjODKPath: " + _strProjODKPath);
                }
                return _strProjODKPath;
            }
            set
            {
                _strProjODKPath = value;
            }
        }
        static private string _strProjODKPath = "";
        #endregion

        #region ViGET template path
        /// <summary>
        /// ViGET 全局 Templates 目录。
        /// </summary>
        public static string TemplatePath
        {
            get { return ProjODKPath + @"TEMPLATES\"; }
        }
        #endregion

        #region ViGET lib path
        /// <summary>
        /// ViGET 全局库所在目录。
        /// </summary>
        public static string LibraryPath
        {
            get { return ProjODKPath + @"lib\"; }
        }
        #endregion

        #region ViGET program path
        /// <summary>
        /// ViGET 应用程序安装的目录。
        /// </summary>
        public static string ViGETPath
        {
            get
            {
                if (string.IsNullOrEmpty(_strViGETPath))
                {
                    RegFile reg = new RegFile(@"HKEY_LOCAL_MACHINE\SOFTWARE\infoteam Software GmbH\OpenPCS");
                    string version = reg.GetValue("CurrentVersion", "", "6.2.0");
                    _strViGETPath = reg.GetValue(version + @"\OpenPCSDir", "", "");
                    _strViGETPath = FileName.GetSafePath(_strViGETPath);
                }
                return _strViGETPath;
            }
        }
        static private string _strViGETPath = "";
        #endregion

        #region Online connection database file
        /// <summary>
        /// Online Server 使用的 Connection Database 文件名称。
        /// </summary>
        public static string OnlineConnectionDatabase
        {
            get { return ProjODKPath + @"onlsvr.con"; }
        }
        #endregion

        #region Show Ladder or not
        /// <summary>
        /// 是否显示 Ladder 类型的功能块？当前设置为不显示。
        /// </summary>
        public static bool ShowLadder
        {
            get
            {
                RegFile reg = new RegFile(@"HKEY_LOCAL_MACHINE\SOFTWARE\DothanTech\ViGET");
                string version = reg.GetValue("", "CurrentVersion", "2.0");
                int BLCodeFilter = reg.GetValue(version + @"\Settings\CatalogOptions", @"BLCodeFilter", -1);

                if (BLCodeFilter < 0)
                {
                    reg = new RegFile(@"HKEY_LOCAL_MACHINE\SOFTWARE\infoteam Software GmbH\OpenPCS");
                    version = reg.GetValue("CurrentVersion", "", "6.2.0");
                    BLCodeFilter = reg.GetValue(version + @"\Settings\CatalogOptions", @"BLCodeFilter", 1);
                }

                return (BLCodeFilter >= 0 && BLCodeFilter != 1);
            }
        }
        #endregion

        #region ViGET V2.0 program path
        // ViGETV20 PATH Equivalent to 
        // ViGet PATH HKEY_LOCAL_MACHINE\\SOFTWARE\\infoteam Software GmbH\\OpenPCS\\" + strOpenPCSVersion + "\\OpenPCSDir
        /// <summary>
        /// ViGET V2.0 的程序安装目录。
        /// </summary>
        public static string ViGETV20Path
        {
            get
            {
                if (string.IsNullOrEmpty(_strViGETV20Path))
                {
                    // Get IniPath from Visual CFC
                    RegFile reg = new RegFile(@"HKEY_LOCAL_MACHINE\SOFTWARE\DothanTech\ViGET");
                    string version = reg.GetValue("", "CurrentVersion", "2.0");
                    _strViGETV20Path = reg.GetValue(version, "OpenPCSDir", "");

                    // Get IniPath from OpenPCS
                    if (string.IsNullOrEmpty(_strViGETV20Path))
                        _strViGETV20Path = ViGETPath;
                    else
                        _strViGETV20Path = FileName.GetSafePath(_strViGETV20Path);

                    Trace.WriteLine("_strViGETV20Path: " + _strViGETV20Path);
                }

                return _strViGETV20Path;
            }
        }
        static private string _strViGETV20Path = "";
        #endregion

        #region ViGET V2.0 program data path
        // ViGETV20 ProgramData PATH Equivalent to 
        // ViGet PATH HKEY_LOCAL_MACHINE\\SOFTWARE\\infoteam Software GmbH\\OpenPCS\\" + strOpenPCSVersion + "\\ProgramDataDir
        /// <summary>
        /// ViGET V2.0 程序数据目录。
        /// </summary>
        public static string ViGETV20ProgramDataPath
        {
            get
            {
                if (string.IsNullOrEmpty(_strViGETV20ProgramDataPath))
                {
                    // Get IniPath from Visual CFC
                    RegFile reg = new RegFile(@"HKEY_LOCAL_MACHINE\SOFTWARE\DothanTech\ViGET");
                    string version = reg.GetValue("", "CurrentVersion", "2.0");
                    _strViGETV20ProgramDataPath = reg.GetValue(version, "ProgramDataDir", "");

                    // Get IniPath from OpenPCS
                    if (string.IsNullOrEmpty(_strViGETV20ProgramDataPath))
                    {
                        reg = new RegFile(@"HKEY_LOCAL_MACHINE\SOFTWARE\infoteam Software GmbH\OpenPCS");
                        version = reg.GetValue("CurrentVersion", "", "6.2.0");
                        _strViGETV20ProgramDataPath = reg.GetValue(version + @"\ProgramDataDir", "", "");
                    }

                    // make sure is safe path
                    _strViGETV20ProgramDataPath = FileName.GetSafePath(_strViGETV20ProgramDataPath);

                    Trace.WriteLine("_strViGETV20ProgramDataPath: " + _strViGETV20ProgramDataPath);
                }

                return _strViGETV20ProgramDataPath;
            }
        }
        private static string _strViGETV20ProgramDataPath = "";
        #endregion

        #region ViGET V2.0 packages to be loaded path
        /// <summary>
        /// ViGET V2.0 PackagesToLoad 目录。
        /// </summary>
        public static string PackagesToLoad
        {
            get
            {
                if (string.IsNullOrEmpty(_strPackagesToLoad))
                {
                    string strPath = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
                    _strPackagesToLoad = FileName.GetSafePath(strPath) + @"PackagesToLoad\";
                }
                return _strPackagesToLoad;
            }
        }
        static private string _strPackagesToLoad = "";
        #endregion

        #region Get ViGET file's full path
        /// <summary>
        /// 得到 ViGET 的模块文件全路径名称。会先尝试 ViGET 目录，然后尝试 ViGET V2.0 的PackagesToLoad 目录。
        /// </summary>
        /// <param name="shortName">ViGET 的模块短路径名称。</param>
        /// <returns>ViGET 的模块长路径名称。</returns>
        public static string GetViGETFile(string shortName)
        {
            // try packages to load folder
            string strPath = PackagesToLoad;
            if (!string.IsNullOrEmpty(strPath) &&
                File.Exists(strPath + shortName))
                return strPath + shortName;

            // failed
            return "";
        }
        #endregion

        #region ViGET Hardware Types

        /// <summary>
        /// ViGET 的硬件类型信息字典。
        /// </summary>
        public static Dictionary<string, ViHardwareType> HardwareTypes
        {
            get
            {
                if (_hardwareTypes == null)
                {
                    _hardwareTypes = new Dictionary<string, ViHardwareType>();

                    IniFile iniFile = new IniFile(ProjODKPath + @"MODULES\MODULES.INI");
                    string strSection = "Modules";
                    for (int index = 1; ; ++index)
                    {
                        string module = iniFile.GetValueS(strSection, "Module" + index.ToString());
                        if (string.IsNullOrEmpty(module)) break;
                        string modIni = iniFile.GetValueS(strSection, module);
                        if (string.IsNullOrEmpty(modIni)) break;

                        ViHardwareType hardwareType = new ViHardwareType()
                        {
                            HardwareName = module,
                            IniFileName = FileName.GetAbsoluteFileName(ProjODKPath + @"MODULES\", modIni),
                            DisplayName = iniFile.GetValue(strSection, "DisplayName" + index.ToString(), module),
                            UserFBPathName = iniFile.GetValue(strSection, "UserFBPathName" + index.ToString(), module),
                            DetailPathName = FileName.GetSafePath(FileName.GetAbsoluteFileName(ProjODKPath + @"MODULES\",
                                             iniFile.GetValue(strSection, "DetailPathName" + index.ToString(), module))),
                        };
                        _hardwareTypes[module.ToUpper()] = hardwareType;
                    }
                }

                return _hardwareTypes;
            }
        }
        private static Dictionary<string, ViHardwareType> _hardwareTypes;

        /// <summary>
        /// 得到指定名称的硬件类型信息。
        /// </summary>
        /// <param name="name">硬件类型名称</param>
        /// <returns>硬件类型名称</returns>
        public static ViHardwareType GetHardwareType(string name)
        {
            name = name.ToUpper();
            if (HardwareTypes.ContainsKey(name))
                return HardwareTypes[name];
            return null;
        }

        #endregion

        #region Global Pou Source

        /// <summary>
        /// 得到指定硬件类型的公共 PouSource 列表。
        /// </summary>
        /// <param name="_hardwareType">硬件类型名称</param>
        /// <returns>公共 PouSource 列表</returns>
        public static ArrayList GetPouSources(string _hardwareType)
        {
            ViHardwareType hardwareType = GetHardwareType(_hardwareType);
            if (hardwareType == null) return null;

            ArrayList alPouSources = new ArrayList();
            AppendPouSource(alPouSources, hardwareType, "GLOBWRAP.INC",
                (sourceFile) => new ViIncPouSource(sourceFile, hardwareType.IniFileName, ViPouAttributes.LDWrapper | ViPouAttributes.Protected));
            AppendPouSource(alPouSources, hardwareType, "GLOBPROT.INC",
                (sourceFile) => new ViIncPouSource(sourceFile, hardwareType.IniFileName, ViPouAttributes.Manufacturer | ViPouAttributes.Public));
            AppendPouSource(alPouSources, hardwareType, ProjODKPath + (ShowLadder ? "IEC_STANDARD_FUN_CFCFBD.INC" : "IEC_STANDARD_FUN_CFCFBD_BL.INC"),
                (sourceFile) => new ViIncPouSource(sourceFile, hardwareType.IniFileName, ViPouAttributes.CFCFBDStandardFuns | ViPouAttributes.Public));

            return alPouSources;
        }
        private static Dictionary<string, ViPouSource> _pouSources;

        private static void AppendPouSource(ArrayList alPouSources, ViHardwareType hardwareType, string sourceFile, Func<string, ViPouSource> factory)
        {
            sourceFile = GetPouSrcFileName(hardwareType, sourceFile);
            if (string.IsNullOrEmpty(sourceFile)) return;

            // 自动创建 PouSource 字典对象
            if (_pouSources == null)
                _pouSources = new Dictionary<string, ViPouSource>();

            // 可能会在 UI/Build 线程中调用，因此互斥访问
            lock (_pouSources)
            {
                // 先在字典中查找，如果找不到，则自动创建
                string sourceKey = (hardwareType.HardwareName + ':' + sourceFile).ToUpper();
                if (_pouSources.ContainsKey(sourceKey))
                {
                    alPouSources.Add(_pouSources[sourceKey]);
                }
                else
                {
                    ViPouSource pouSource = factory(sourceFile);
                    if (pouSource != null)
                    {
                        _pouSources[sourceKey] = pouSource;
                        alPouSources.Add(pouSource);
                    }
                }
            }
        }

        private static string GetPouSrcFileName(ViHardwareType hardwareType, string sourceFile)
        {
            if (FileName.GetFilePathType(sourceFile) != FileName.FilePathType.Absolute)
            {
                string newFile = FileName.GetAbsoluteFileName(hardwareType.DetailPathName, sourceFile);
                if (!File.Exists(newFile))
                    newFile = FileName.GetAbsoluteFileName(ProjODKPath + @"System\", sourceFile);
                //
                sourceFile = newFile;
            }
            if (!File.Exists(sourceFile))
                return null;

            return sourceFile;
        }

        #endregion

        #region ViGET project related path treatment

        /// <summary>
        /// UserFBs 目录的路径名称。
        /// </summary>
        public const string UserFBsPath = "UserFBs";
        /// <summary>
        /// UserFBs 目录的显示名称。
        /// </summary>
        public const string UserFBsShown = "UserFBs";

        /// <summary>
        /// 得到工程的 $ENV$ 目录全路径。
        /// </summary>
        public static string GetENVPath(string projectPath)
        {
            return projectPath + @"$ENV$\";
        }

        /// <summary>
        /// 得到工程的 $GEN$ 目录全路径。
        /// </summary>
        public static string GetGENPath(string projectPath)
        {
            return projectPath + @"$GEN$\";
        }

        /// <summary>
        /// 得到工程的 CPU 的 $ENV$ 目录全路径。
        /// </summary>
        public static string GetCPUENVPath(string projectPath, string cpuName)
        {
            return GetENVPath(projectPath) + cpuName + @"\";
        }

        /// <summary>
        /// 得到工程的 CPU 的 $GEN$ 目录全路径。
        /// </summary>
        public static string GetCPUGENPath(string projectPath, string cpuName)
        {
            return GetGENPath(projectPath) + cpuName + @"\";
        }

        /// <summary>
        /// 得到工程的 UserFBs 目录全路径。
        /// </summary>
        public static string GetUserFBsPath(string projectPath)
        {
            return projectPath + UserFBsPath + @"\";
        }

        /// <summary>
        /// 得到工程的 UserFBs 下指定硬件类型的目录全路径。
        /// </summary>
        public static string GetHardwareFBsPath(string projectPath, string hardwareName)
        {
            return GetUserFBsPath(projectPath) + hardwareName + @"\";
        }

        /// <summary>
        /// 得到功能块路径中包含的硬件类型名称。
        /// </summary>
        public static string GetPathHardwareName(string projectPath, string fbFileName)
        {
            fbFileName = FileName.GetAbsoluteFileName(projectPath, fbFileName);
            string userFBsPath = GetUserFBsPath(projectPath);
            if (!fbFileName.StartsWith(userFBsPath))
                return null;

            fbFileName = fbFileName.Substring(userFBsPath.Length);

            int pos = fbFileName.IndexOf('\\');
            if (pos <= 0) return null;

            return fbFileName.Substring(0, pos);
        }

        #endregion
    }

    /// <summary>
    /// 硬件类型信息，在 OpenPCS.520\MODULES 目录下。
    /// </summary>
    public class ViHardwareType
    {
        public string HardwareName;         ///< 硬件类型名称
        public string DisplayName;          ///< 硬件类型的显示名称
        public string UserFBPathName;       ///< UserFB 存放的目录
        public string IniFileName;          ///< 硬件 INI 配置文件名称
        public string DetailPathName;       ///< 硬件类型详细信息的目录
    }
}

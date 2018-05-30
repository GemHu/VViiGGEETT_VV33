using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Microsoft.Win32;
using Dothan.Helpers;
using Dothan.ViObject;

namespace DothanTech.ViGET.Manager
{
    public class ViProjDataModule
    {
        public ViProjDataModule(string moduleName, string oemDeviceName, string fbPathName, string fbDispName)
        {
            this.ModuleName = moduleName;
            this.OemDeviceName = oemDeviceName;
            this.FBPathName = fbPathName;
            this.FBDispName = fbDispName;
        }

        public string ModuleName { get; protected set; }
        public string OemDeviceName { get; protected set; }
        public string FBPathName { get; protected set; }
        public string FBDispName { get; protected set; }
    }

    public static class ViProjDataConfig
    {
        public const string strFuncBlockPathName = "UserFBs";
        public const string strFuncBlockDispName = "UserFBs";
        public const string strUnlinkedCFCPath = "UnlinkedItems";
        public const string strUserFuncs = "UserFunctions";

        public const string strXJ_CP3000Hardware = "XJ_CP3000";
        public const string strFB_CP3000PathName = "XJ_CP3000";
        public const string strFB_CP3000DispName = "XJ_CP3000";

        public const string strXJ_MS3000Hardware = "XJ_MS3000";
        public const string strFB_MS3000PathName = "XJ_MS3000";
        public const string strFB_MS3000DispName = "XJ_MS3000";

        public const string strSmartSIMHardware = "SmartSIM";
        public const string strSmartSIMPathName = "SmartSIM";
        public const string strSmartSIMDispName = "SmartSIM";

        public static ViProjDataModule GetHardwareModule(string hardware)
        {
            loadModulesInfo();

            if (!string.IsNullOrEmpty(hardware))
            {
                hardware = hardware.ToUpper();
                if (modules.ContainsKey(hardware))
                    return modules[hardware];
            }
            return null;
        }

        public static string GetHardwarePathName(string hardware)
        {
            ViProjDataModule module = GetHardwareModule(hardware);
            if (module == null)
                return "";
            return module.FBPathName;
        }

        public static bool IsFBPathNameValid(string fbPathName)
        {
            loadModulesInfo();

            foreach (ViProjDataModule module in modules.Values)
            {
                if (module.FBPathName.Equals(fbPathName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        public static string GetHardwareDispName(string hardware)
        {
            ViProjDataModule module = GetHardwareModule(hardware);
            if (module == null)
                return "";
            return module.FBDispName;
        }

        private static void loadModulesInfo()
        {
            if (modules == null)
            {
                modules = new Dictionary<string, ViProjDataModule>();

                string strProjODKPath = ViGlobal.ProjODKPath;
                IniFile iniModules = new IniFile(strProjODKPath + @"MODULES\MODULES.INI");
                for (int index = 1; ; ++index)
                {
                    string internalName = iniModules.GetValueS("Modules", "Module" + index.ToString());
                    if (string.IsNullOrEmpty(internalName))
                        break;

                    string iniFileName = iniModules.GetValueS("Modules", internalName);
                    IniFile iniHardware = new IniFile(strProjODKPath + @"MODULES\" + iniFileName);

                    string oemDeviceName = iniHardware.GetValueS("Module", "OemDeviceName");
                    if (string.IsNullOrEmpty(oemDeviceName))
                        continue;
                    String fbPathName = iniHardware.GetValue("Module", "FBPathName", internalName);
                    String fbDispName = iniHardware.GetValue("Module", "FBDispName", internalName);

                    modules[internalName.ToUpper()] = new ViProjDataModule(internalName, oemDeviceName, fbPathName, fbDispName);
                }
            }
        }

        public static Dictionary<string, ViProjDataModule> Modules
        {
            get
            {
                loadModulesInfo();

                return modules;
            }
        }

        private static Dictionary<string, ViProjDataModule> modules;
    }
}

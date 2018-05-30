using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace Dothan.Helpers
{
    public class CfcUpgradeHelper
    {
        private const string Ext_CFC = ".CFC";
        private const string Ext_XCFC = ".XCFC";

        /// <summary>
        /// 检查并升级CFC文件；
        /// </summary>
        /// <param name="cfcFile">可以为CFC文件，或者XCFC文件。</param>
        /// <param name="strCommand">生及命令，需要调用者提供。</param>
        public static bool CheckAndUpgradeCfcFile(string cfcFile, string projectPath, string makFile, string strCommand)
        {
            if (string.IsNullOrEmpty(cfcFile))
                return false;
            if (!cfcFile.EndsWith(Ext_CFC, StringComparison.OrdinalIgnoreCase) && !cfcFile.EndsWith(Ext_XCFC, StringComparison.OrdinalIgnoreCase))
                return false;

            return UpgradeCfcFile(cfcFile.Substring(0, cfcFile.LastIndexOf('.')), projectPath, makFile, strCommand);
        }

        private static bool UpgradeCfcFile(string cfcPathWithoutExtention, string projectPath, string makFile, string strCommand)
        {
            string cfcFile = cfcPathWithoutExtention + ".CFC";
            string xcfcFile = cfcPathWithoutExtention + ".XCFC";

            if (!File.Exists(cfcFile) || File.Exists(xcfcFile))
                return false;

            try
            {
                if (string.IsNullOrEmpty(strCommand))
                {
                    Trace.WriteLine("Can't find CFCUpgrade tool \"CFCUpgrade.exe\". Please re-install the program!");
                    return false;
                }

                // command-line parameter ready to compile
                // string strArgument = "-f \"" + cfcFile + "\" -v \"" + strVarFile + "\" -n";
                string strArgument = "-f \"" + cfcFile + "\"";
                if (!string.IsNullOrEmpty(makFile))
                    strArgument += (" -m \"" + makFile + "\"");
                if (!string.IsNullOrEmpty(projectPath))
                    strArgument += (" -d \"" + projectPath + "\"");

                ProcessStartInfo startInfo = new ProcessStartInfo(strCommand, strArgument);
                startInfo.CreateNoWindow = true;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
                startInfo.WorkingDirectory = FileName.GetFilePath(strCommand);
                startInfo.ErrorDialog = true;
                startInfo.UseShellExecute = false;
                Process itmakeProcess = new Process();
                itmakeProcess.StartInfo = startInfo;
                if (!itmakeProcess.Start())
                {
                    Trace.WriteLine(string.Format("Execute {0} failed!", strCommand));
                    return false;
                }

                // display the information outputed by ITMake 
                string info = GetLogInfo(itmakeProcess.StandardOutput);
                if (!string.IsNullOrEmpty(info))
                    Trace.WriteLine(info);
                info = GetLogInfo(itmakeProcess.StandardError);
                if (!string.IsNullOrEmpty(info))
                {
                    Trace.WriteLine("#############  ERROR  ##############");
                    Trace.WriteLine(info);
                    Trace.WriteLine("####################################");
                }
                itmakeProcess.Close();

                return true;
            }
            catch (Exception e)
            {
                Trace.WriteLine("### [" + e.Source + "] Exception: " + e.Message);
                Trace.WriteLine("### " + e.StackTrace);

                return false;
            }
        }

        private static string GetLogInfo(StreamReader sr)
        {
            try
            {
                return sr.ReadToEnd();
            }
            catch (Exception e)
            {
                Trace.WriteLine("### [" + e.Source + "] Exception: " + e.Message);
                Trace.WriteLine("### " + e.StackTrace);

                return string.Empty;
            }
            finally
            {
                sr.Close();
            }
        }
    }
}

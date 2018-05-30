/// <summary>
/// @file   ProjectManager.cs
///	@brief  ViGET 工程数据管理器中与编译相关的数据与方法。
/// @author	DothanTech 胡殿兴
/// 
/// Copyright(C) 2011~2014, DothanTech. All rights reserved.
/// </summary>

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dothan.Helpers;
using Dothan.ViObject;
using System.Diagnostics;
using System.Collections;
using System.Runtime.InteropServices;
using DothanTech.ViGET.ViService;

namespace DothanTech.ViGET.Manager
{
    public partial class ProjectManager : IViBuildGroup
    {
        #region Log Error and Worning

        const string PoeFileExt = ".POE";
        const string CFCFileExt = ".XCFC";

        public void LogWarning(string fName, string strMessage)
        {
            try
            {
                // if it is a POE file, turn it to CFC
                if (Path.GetExtension(fName).Equals(PoeFileExt, StringComparison.OrdinalIgnoreCase))
                {
                    fName = Path.GetFileNameWithoutExtension(fName) + CFCFileExt;

                    if (System.IO.File.Exists(fName))
                    {
                    }
                    else
                    {
                        // the file not found, ignore this error
                        // TODO: Check PIV ST etc maybe in the future.
                        return;
                    }
                }

                // add cfc navigate information
                if (!strMessage.Contains("(*"))
                {
                    strMessage = BuildCfcErrorMessage(strMessage);
                }

                // send to error list window
                // 可借助于事件或者命令来实现；
                //if (this.theProjectNode != null)
                //{
                //    this.theProjectNode.LogWarning(fName, strMessage);
                //}

            }
            catch (Exception ee)
            {
                Trace.WriteLine("### [" + ee.Source + "] Exception: " + ee.Message);
                Trace.WriteLine("### " + ee.StackTrace);
            }
        }

        public void LogError(string fName, string strMessage)
        {
            try
            {
                // if it is a POE file, turn it to CFC
                if (Path.GetExtension(fName).Equals(PoeFileExt, StringComparison.OrdinalIgnoreCase))
                {
                    //fName = Path.GetFileNameWithoutExtension(fName) + CFCFileExt;
                    fName = Path.ChangeExtension(fName, CFCFileExt);

                    if (System.IO.File.Exists(fName))
                    {
                    }
                    else
                    {
                        // the file not found, ignore this error
                        // TODO: Check PIV ST etc maybe in the future.
                        return;
                    }
                }

                // add cfc navigate information
                if (!strMessage.Contains("(*"))
                {
                    strMessage = BuildCfcErrorMessage(strMessage);
                }

                // send to error list window
                // 可借助于事件或者命令来实现；
                //if (this.theProjectNode != null)
                //{
                //    this.theProjectNode.LogError(fName, strMessage);
                //}
            }
            catch (Exception ee)
            {
                Trace.WriteLine("### [" + ee.Source + "] Exception: " + ee.Message);
                Trace.WriteLine("### " + ee.StackTrace);
            }
        }

        [DllImport("DZCfcExt.dll", EntryPoint = "DZCEBuildErrorMessage", CharSet = CharSet.Ansi)]
        internal static extern uint BuildCfcErrorMessage([MarshalAs(UnmanagedType.LPStr)] string csErrMsg, StringBuilder buf, uint nMaxCount);
        // Build abundant message according to CFC built output message
        public static string BuildCfcErrorMessage(string csErrMsg)
        {
            StringBuilder buf = new StringBuilder(2048);
            if (BuildCfcErrorMessage(csErrMsg, buf, 2048) <= 0)
                return csErrMsg;
            return buf.ToString();
        }

        #endregion

        public void LoopBuildItem(Action<IViBuildItem> action)
        {
            foreach (var item in this.Children)
            {
                if (item is IViBuildItem)
                {
                    action(item as IViBuildItem);
                }
                else if (item is IViBuildGroup)
                {
                    (item as IViBuildGroup).LoopBuildItem(action);
                }
            }
        }
    }
}

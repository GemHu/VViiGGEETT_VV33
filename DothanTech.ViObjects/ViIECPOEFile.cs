/// <summary>
/// @file   ViIECPOEFile.cs
///	@brief  ViGET IEC POE 文件相关的操作。
/// @author	DothanTech 刘伟宏
/// 
/// Copyright(C) 2011~2014, DothanTech. All rights reserved.
/// </summary>

using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;

namespace Dothan.ViObject
{
    /// <summary>
    /// ViGET IEC POE 文件相关的操作。
    /// </summary>
    public static class ViIECPOEFile
    {
        /// <summary>
        /// 得到指定 POE 文件的功能块描述信息。
        /// </summary>
        /// <param name="fileName">POE 文件名称</param>
        /// <returns>功能块描述信息，null 表示失败。</returns>
        public static ViPOEBlockType GetBlockType(string fileName)
        {
            try
            {
                using (StreamReader reader = new StreamReader(fileName, Encoding.Default))
                    return GetBlockType(fileName, reader);
            }
            catch (Exception ee)
            {
                Trace.WriteLine("### [" + ee.Source + "] Exception: " + ee.Message);
                Trace.WriteLine("### " + ee.StackTrace);
            }

            return null;
        }

        /// <summary>
        /// 得到指定 POE 文件的功能块描述信息。
        /// </summary>
        /// <param name="fileName">POE 文件名称</param>
        /// <param name="reader">文本文件读对象</param>
        /// <returns>功能块描述信息，null 表示失败。</returns>
        public static ViPOEBlockType GetBlockType(string fileName, TextReader reader)
        {
            ViPOEBlockType blockType = null;
            ViConnType connType = null;
            int lineIndex = 0;
            // 0x00: 没有读取到功能块开始
            // 0x01: 读取到功能块开始
            // 0x21: 开始 INPUT
            // 0x22: 开始 OUTPUT
            // 0x23: 开始 IN_OUT
            int stage = 0;

            while (true)
            {
                string line = reader.ReadLine();
                if (line == null) break;
                ++lineIndex;

                line = line.Trim();
                if (string.IsNullOrEmpty(line)) continue;

                if (stage == 0x00)
                {
                    if (line.StartsWith("FUNCTION_BLOCK ", StringComparison.OrdinalIgnoreCase))
                    {
                        connType = ViConnType.Parse(line.Substring(15), ViTypeCreation.None);
                        if (connType != null && connType.Type == null)
                        {
                            blockType = new ViPOEBlockType(connType.Name, null);
                            blockType.Comment = connType.Comment;
                            //
                            stage = 0x01;
                        }
                        else
                        {
                            Trace.WriteLine(string.Format("{0}({1}): Invalid FUNCTION_BLOCK declaration!", fileName, lineIndex));
                        }
                    }
                    //else if (line.StartsWith("PROGRAM ", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    connType = ViConnType.Parse(line.Substring(8), ViTypeCreation.None);
                    //    if (connType != null && connType.Type == null)
                    //    {
                    //        blockType = new ViPOEBlockType(connType.Name, null);
                    //        blockType.Comment = connType.Comment;
                    //        //
                    //        stage = 0x01;
                    //    }
                    //    else
                    //    {
                    //        Trace.WriteLine(string.Format("{0}({1}): Invalid PROGRAM declaration!", fileName, lineIndex));
                    //    }
                    //}
                    else if (line.StartsWith("FUNCTION ", StringComparison.OrdinalIgnoreCase))
                    {
                        connType = ViConnType.Parse(line.Substring(9), ViTypeCreation.Create);
                        if (connType != null && connType.Type != null)
                        {
                            blockType = new ViPOEBlockType(connType.Name, connType.Type);
                            blockType.Comment = connType.Comment;
                            //
                            stage = 0x01;
                        }
                        else
                        {
                            Trace.WriteLine(string.Format("{0}({1}): Invalid FUNCTION declaration!", fileName, lineIndex));
                        }
                    }
                }
                else if (stage >= 0x01)
                {
                    if (stage == 0x01 && line.EndsWith("*)") &&
                        line.StartsWith("(* task usage:", StringComparison.OrdinalIgnoreCase))
                    {
                        blockType.ModesDecl = line.Substring(14, line.Length - 16);
                    }
                    else if (line.StartsWith(ViTextFile.CommentBegin))
                    {
                        // 注释行
                    }
                    else if (line.StartsWith("VAR_INPUT", StringComparison.OrdinalIgnoreCase))
                    {
                        stage = 0x21;
                    }
                    else if (line.StartsWith("VAR_OUTPUT", StringComparison.OrdinalIgnoreCase))
                    {
                        stage = 0x22;
                    }
                    else if (line.StartsWith("VAR_IN_OUT", StringComparison.OrdinalIgnoreCase))
                    {
                        stage = 0x23;
                    }
                    else if (line.StartsWith("END_VAR", StringComparison.OrdinalIgnoreCase))
                    {
                        stage = 0x01;
                    }
                    else if (line.StartsWith("LD ", StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }
                    else if (line.StartsWith("END_FUNCTION", StringComparison.OrdinalIgnoreCase))
                    {
                        if (blockType != null)
                        {
                            // 功能块后缀描述字符串
                            blockType.SetSuffixDecl(line);
                        }
                        break;
                    }
                    else if (stage >= 0x21)
                    {
                        connType = ViConnType.Parse(line, ViTypeCreation.Create);
                        if (connType != null && connType.Type != null)
                        {
                            switch ((stage & 0x0F))
                            {
                                case 0x01:  // INPUT
                                    connType.SetParent(blockType);
                                    blockType.InputConnectors.Add(connType);
                                    break;
                                case 0x02:  // OUTPUT
                                    connType.SetParent(blockType);
                                    blockType.OutputConnectors.Add(connType);
                                    break;
                                case 0x03:  // IN_OUT
                                    connType.SetParent(blockType);
                                    blockType.InputConnectors.Insert(blockType.InOutConnectorCount, connType);
                                    blockType.OutputConnectors.Insert(blockType.InOutConnectorCount, connType);
                                    blockType.InOutConnectorCount = blockType.InOutConnectorCount + 1;
                                    break;
                            }
                        }
                        else
                        {
                            Trace.WriteLine(string.Format("{0}({1}): Invalid connection declaration!", fileName, lineIndex));
                        }
                    }
                }
            }

            return blockType;
        }

        /// <summary>
        /// 修改 POE 文件内部的类型名称。
        /// </summary>
        /// <param name="fileName">POE 文件名称</param>
        /// <returns>成功与否？</returns>
        public static bool ChangeName(string fileName)
        {
            return ChangeName(fileName, Path.GetFileNameWithoutExtension(fileName));
        }

        /// <summary>
        /// 修改 POE 文件内部的类型名称。
        /// </summary>
        /// <param name="fileName">POE 文件名称</param>
        /// <param name="typeName">POE 类型名称</param>
        /// <returns>成功与否？</returns>
        public static bool ChangeName(string fileName, string typeName)
        {
            try
            {
                ArrayList alLines = new ArrayList();

                // 读取文件所有行
                using (StreamReader reader = new StreamReader(fileName, Encoding.Default))
                {
                    ViConnType connType = null;
                    string oldName = null;
                    string line, trim, type;
                    int lineIndex = 0;
                    while (true)
                    {
                        line = reader.ReadLine();
                        if (line == null) break;
                        ++lineIndex;

                        if (oldName == null)
                        {
                            trim = line.Trim();
                            if (trim.StartsWith("PROGRAM ", StringComparison.OrdinalIgnoreCase))
                            {
                                connType = ViConnType.Parse(trim.Substring(8), ViTypeCreation.None);
                                type = "PROGRAM";
                            }
                            else if (trim.StartsWith("FUNCTION_BLOCK ", StringComparison.OrdinalIgnoreCase))
                            {
                                connType = ViConnType.Parse(trim.Substring(15), ViTypeCreation.None);
                                type = "FUNCTION_BLOCK";
                            }
                            else if (trim.StartsWith("FUNCTION ", StringComparison.OrdinalIgnoreCase))
                            {
                                connType = ViConnType.Parse(trim.Substring(9), ViTypeCreation.Create);
                                type = "FUNCTION";
                            }
                            else
                            {
                                alLines.Add(line);
                                continue;
                            }

                            if (connType == null)
                                return false;
                            oldName = connType.Name;
                            // 名称一样，不需要修改
                            if (oldName == typeName)
                                return true;

                            line = type + ' ' + typeName;
                            if (connType.Type != null)
                                line += " : " + connType.Type.Name;
                            alLines.Add(line);
                        }
                        else
                        {
                            alLines.Add(line);
                        }
                    }

                    // 没有读取到类型名称？
                    if (oldName == null)
                        return false;
                }

                // 写入所有行
                using (StreamWriter writer = ViTextFile.CreateWriter(fileName))
                {
                    foreach (string line in alLines)
                        writer.WriteLine(line);
                }

                // 大功告成
                return true;
            }
            catch (Exception ee)
            {
                Trace.WriteLine("### [" + ee.Source + "] Exception: " + ee.Message);
                Trace.WriteLine("### " + ee.StackTrace);
            }

            return false;
        }
    }
}

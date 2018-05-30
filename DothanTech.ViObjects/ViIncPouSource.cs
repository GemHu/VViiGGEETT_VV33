/// <summary>
/// @file   ViIncPouSource.cs
///	@brief  ViGET INC 类型文本文件的功能块数据来源。
/// @author	DothanTech 刘伟宏
/// 
/// Copyright(C) 2011~2014, DothanTech. All rights reserved.
/// </summary>

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.ObjectModel;

using Dothan.Helpers;

namespace Dothan.ViObject
{
    /// <summary>
    /// ViGET INC 类型文本文件的功能块数据来源。
    /// </summary>
    public class ViIncPouSource : ViPouSource
    {
        #region Life cycle

        /// <summary>
        /// 构建对象。
        /// </summary>
        /// <param name="sourceFile">功能块数据来源对应的文件。</param>
        /// <param name="infoFile">功能块信息文件，主要用于获取功能块的特性，如 Init / System / Normal Mode。</param>
        /// <param name="pouAttributes">数据来源中的功能块的特性。</param>
        public ViIncPouSource(string sourceFile, string infoFile, ViPouAttributes pouAttributes)
            : base(sourceFile, pouAttributes)
        {
            this.InfoFile = infoFile;
        }

        /// <summary>
        /// 功能块信息文件，主要用于获取功能块的特性，如 Init / System / Normal Mode。
        /// </summary>
        public string InfoFile { get; protected set; }

        #endregion

        /// <summary>
        /// 更新功能块数组列表。
        /// </summary>
        public override void UpdatePOUs()
        {
            this.DeleteAll();

            try
            {
                ViFirmBlockType blockType = null;
                ViConnType connType = null;

                if (File.Exists(this.SourceFile))
                {
                    int stage = 0x00;   // 0x00: 未开始
                    // 0x10: 开始 Proto Type
                    // 0x20: 开始 Function Block
                    // 0x3x: 开始 Function
                    // 0x21: 开始 INPUT
                    // 0x22: 开始 OUTPUT
                    // 0x23: 开始 IN_OUT
                    using (StreamReader reader = new StreamReader(this.SourceFile, Encoding.Default))
                    {
                        int lineIndex = 0;
                        while (true)
                        {
                            string line = reader.ReadLine();
                            if (line == null) break;
                            ++lineIndex;

                            line = line.Trim();
                            if (string.IsNullOrEmpty(line)) continue;

                            // 注释？
                            if (line.StartsWith(ViTextFile.CommentBegin)) continue;

                            if (stage == 0x00)
                            {
                                if (line.StartsWith("GLOBAL_PROTOTYP_BEGIN", StringComparison.OrdinalIgnoreCase))
                                    stage = 0x10;
                            }
                            else if (line.StartsWith("GLOBAL_PROTOTYP_END", StringComparison.OrdinalIgnoreCase))
                            {
                                stage = 0x00;
                            }
                            else if (stage == 0x10)
                            {
                                if (line.StartsWith("FUNCTION_BLOCK ", StringComparison.OrdinalIgnoreCase))
                                {
                                    connType = ViConnType.Parse(line.Substring(15), ViTypeCreation.CreateGlobal);
                                    if (connType != null && connType.Type == null)
                                    {
                                        stage = 0x20;
                                        //
                                        blockType = new ViFirmBlockType(connType.Name, null);
                                        blockType.Comment = connType.Comment;
                                    }
                                    else
                                    {
                                        Trace.WriteLine(string.Format("{0}({1}): Invalid FUNCTION_BLOCK declaration!", this.SourceFile, lineIndex));
                                    }
                                }
                                else if (line.StartsWith("FUNCTION ", StringComparison.OrdinalIgnoreCase))
                                {
                                    connType = ViConnType.Parse(line.Substring(9), ViTypeCreation.CreateGlobal);
                                    if (connType != null && connType.Type != null)
                                    {
                                        stage = 0x30;
                                        //
                                        blockType = new ViFirmBlockType(connType.Name, connType.Type);
                                        blockType.Comment = connType.Comment;
                                    }
                                    else
                                    {
                                        Trace.WriteLine(string.Format("{0}({1}): Invalid FUNCTION declaration!", this.SourceFile, lineIndex));
                                    }
                                }
                            }
                            else if (stage >= 0x20)
                            {
                                if (line.StartsWith("VAR_INPUT", StringComparison.OrdinalIgnoreCase))
                                {
                                    stage = (stage & 0xF0) | 0x01;
                                }
                                else if (line.StartsWith("VAR_OUTPUT", StringComparison.OrdinalIgnoreCase))
                                {
                                    stage = (stage & 0xF0) | 0x02;
                                }
                                else if (line.StartsWith("VAR_IN_OUT", StringComparison.OrdinalIgnoreCase))
                                {
                                    stage = (stage & 0xF0) | 0x03;
                                }
                                else if (line.StartsWith("END_VAR", StringComparison.OrdinalIgnoreCase))
                                {
                                    stage = (stage & 0xF0) | 0x00;
                                }
                                else if (line.StartsWith("END_FUNCTION", StringComparison.OrdinalIgnoreCase))
                                {
                                    if (blockType != null)
                                    {
                                        // 功能块后缀描述字符串
                                        blockType.SetSuffixDecl(line);

                                        if (ChildByName(blockType.Name) == null)
                                        {
                                            this.AddChild(blockType);
                                            //Trace.WriteLine("Block Type: \n" + blockType.ToString());
                                        }
                                        else
                                        {
                                            Trace.WriteLine(string.Format("{0}({1}): Duplicate POU name [{2}]!", this.SourceFile, lineIndex, blockType.Name));
                                        }
                                    }
                                    stage = 0x10;
                                }
                                else if ((stage & 0x0F) != 0x00)
                                {
                                    connType = ViConnType.Parse(line, ViTypeCreation.CreateGlobal);
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
                                        Trace.WriteLine(string.Format("{0}({1}): Invalid connection declaration!", this.SourceFile, lineIndex));
                                    }
                                }
                            }
                        }
                    }
                }

                // 从对应配置文件中读取功能块参数/属性信息
                if (File.Exists(this.InfoFile))
                {
                    IniFile iniFile = new IniFile(this.InfoFile);
                    blockType = null;
                    iniFile.LoopSections(
                        (section) =>
                        {
                            blockType = GetPOU(section);
                            return (blockType != null);
                        },
                        (key, value) =>
                        {
                            if (key.Equals("InitMode", StringComparison.OrdinalIgnoreCase) && value.ToBool())
                            {
                                blockType.Modes |= ViFirmBlockMode.Init;
                            }
                            else if (key.Equals("SystemMode", StringComparison.OrdinalIgnoreCase) && value.ToBool())
                            {
                                blockType.Modes |= ViFirmBlockMode.System;
                            }
                            else if (key.Equals("NormalMode", StringComparison.OrdinalIgnoreCase) && value.ToBool())
                            {
                                blockType.Modes |= ViFirmBlockMode.Normal;
                            }
                            else if (key.Equals("LibraryType", StringComparison.OrdinalIgnoreCase) ||
                                key.Equals("Category", StringComparison.OrdinalIgnoreCase))
                            {
                                blockType.Category = value;
                            }
                            else if (key.Equals("FWLibraryName", StringComparison.OrdinalIgnoreCase))
                            {
                                blockType.FWLibraryName = value;
                            }
                            else if (key.Equals("FWLibraryVersion", StringComparison.OrdinalIgnoreCase))
                            {
                                blockType.FWLibraryVersion = value;
                            }
                            else if (key.Equals("BlockComment", StringComparison.OrdinalIgnoreCase))
                            {
                                blockType.Comment = value;
                            }
                            return true;
                        });
                }
            }
            catch (Exception ee)
            {
                Trace.WriteLine("### [" + ee.Source + "] Exception: " + ee.Message);
                Trace.WriteLine("### " + ee.StackTrace);
            }
        }
    }
}

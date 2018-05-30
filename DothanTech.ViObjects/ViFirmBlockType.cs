/// <summary>
/// @file   ViFirmBlockType.cs
///	@brief  ViGET 编辑器中的固件功能块类型。
/// @author	DothanTech 刘伟宏
/// 
/// Copyright(C) 2011~2014, DothanTech. All rights reserved.
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.ObjectModel;

using Dothan.Helpers;

namespace Dothan.ViObject
{
    /// <summary>
    /// ViGET 编辑器中的固件功能块的运行模式。
    /// </summary>
    [Flags]
    public enum ViFirmBlockMode
    {
        None = 0x00,
        Init = 0x01,
        System = 0x02,
        Normal = 0x04,
    }

    /// <summary>
    /// ViGET 编辑器中的固件功能块的后缀描述信息的类型。
    /// </summary>
    [Flags]
    public enum ViFirmBlockSuffixType
    {
        None = 0x00,                ///< 没有后缀描述信息
        Modes = 0x01,               ///< 正常的 Modes 描述信息，#Modes:
        SModes = 0x02,              ///< 短的 Modes 描述信息，#
        FWLibrary = 0x04,           ///< 正常的 FWLibrary 描述信息，#FwLib:
        Comment = 0x08,             ///< 备注后缀，@
    }

    /// <summary>
    /// ViGET 编辑器中的固件功能块类型。
    /// </summary>
    public class ViFirmBlockType : ViType, IViSerializeBlur
    {
        #region Life cycle

        /// <summary>
        /// 构建对象。
        /// </summary>
        /// <param name="name">功能块名称</param>
        /// <param name="dataType">功能块返回的类型，为 null 表示是 FUNCTION BLOCK。</param>
        public ViFirmBlockType(string name, ViDataType dataType)
            : base(name, dataType)
        {
            this.InputConnectors = new ViObservableCollection<ViConnType>();
            this.OutputConnectors = new ViObservableCollection<ViConnType>();
        }

        /// <summary>
        /// 释放与其它对象之间的弱引用。
        /// </summary>
        public override void Dispose()
        {
            if (this.InputConnectors != null)
            {
                foreach (ViConnType connType in this.InputConnectors)
                    connType.Dispose();
                this.InputConnectors.Clear();
            }
            if (this.OutputConnectors != null)
            {
                foreach (ViConnType connType in this.OutputConnectors)
                    connType.Dispose();
                this.OutputConnectors.Clear();
            }

            base.Dispose();
        }

        #endregion

        #region Type

        /// <summary>
        /// 得到功能块返回数据类型。
        /// </summary>
        public new ViDataType Type
        {
            get { return base.Type as ViDataType; }
            set { base.Type = value; }
        }

        /// <summary>
        /// 返回是否是 FUNCTION BLOCK？
        /// </summary>
        public bool IsFunctionBlock
        {
            get
            {
                return (base.Type == null);
            }
        }

        #endregion

        #region D Category

        public static readonly DependencyProperty CategoryProperty =
            DependencyProperty.Register("Category", typeof(string), typeof(ViFirmBlockType),
                                        new FrameworkPropertyMetadata(string.Empty));

        /// <summary>
        /// 对象所属分组。
        /// </summary>
        public string Category
        {
            get
            {
                return (string)GetValue(CategoryProperty);
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    ClearValue(CategoryProperty);
                else
                    SetValue(CategoryProperty, value);
            }
        }

        #endregion

        #region Modes

        /// <summary>
        /// ViGET 编辑器中的固件功能块的运行模式。
        /// </summary>
        public ViFirmBlockMode Modes { get; set; }

        /// <summary>
        /// 字符串类型的 Modes 描述信息。
        /// </summary>
        public string ModesDecl
        {
            get
            {
                string decl = string.Empty;
                if (((uint)(this.Modes & ViFirmBlockMode.Init)) != 0)
                    decl += 'I';
                if (((uint)(this.Modes & ViFirmBlockMode.System)) != 0)
                    decl += 'S';
                if (((uint)(this.Modes & ViFirmBlockMode.Normal)) != 0)
                    decl += 'N';
                //
                return string.IsNullOrEmpty(decl) ? "N" : decl;
            }
            set
            {
                this.Modes = ViFirmBlockMode.None;

                if (string.IsNullOrEmpty(value))
                    return;

                for (int i = 0; i < value.Length; ++i)
                {
                    switch (value[i])
                    {
                        case 'I':
                        case 'i':
                            // 如果是类似 I1 之内的描述，表示是 I1 中断中运行，不是 Init Mode
                            if (i + 1 >= value.Length || !char.IsDigit(value[i + 1]))
                                this.Modes |= ViFirmBlockMode.Init;
                            break;
                        case 'S':
                        case 's':
                            this.Modes |= ViFirmBlockMode.System;
                            break;
                        case 'N':
                        case 'n':
                            this.Modes |= ViFirmBlockMode.Normal;
                            break;
                    }
                }
            }
        }

        #endregion

        #region D FWLibraryName

        public static readonly DependencyProperty FWLibraryNameProperty =
            DependencyProperty.Register("FWLibraryName", typeof(string), typeof(ViFirmBlockType),
                                        new FrameworkPropertyMetadata(string.Empty));

        /// <summary>
        /// 对象所属库文件名称。
        /// </summary>
        public string FWLibraryName
        {
            get
            {
                return (string)GetValue(FWLibraryNameProperty);
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    ClearValue(FWLibraryNameProperty);
                else
                    SetValue(FWLibraryNameProperty, value);
            }
        }

        public static readonly DependencyProperty FWLibraryVersionProperty =
            DependencyProperty.Register("FWLibraryVersion", typeof(string), typeof(ViFirmBlockType),
                                        new FrameworkPropertyMetadata(string.Empty));

        /// <summary>
        /// 对象所属库文件版本号。
        /// </summary>
        public string FWLibraryVersion
        {
            get
            {
                return (string)GetValue(FWLibraryVersionProperty);
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    ClearValue(FWLibraryVersionProperty);
                else
                    SetValue(FWLibraryVersionProperty, value);
            }
        }

        /// <summary>
        /// 对象所属库文件描述信息。
        /// </summary>
        public string FWLibraryDecl
        {
            get
            {
                if (string.IsNullOrEmpty(this.FWLibraryName))
                    return string.Empty;

                if (string.IsNullOrEmpty(this.FWLibraryVersion))
                    return this.FWLibraryName;

                return string.Format("{0} ({1})", this.FWLibraryName, this.FWLibraryVersion);
            }
            set
            {
                int pos = value.IndexOf('(');
                if (pos < 0)
                {
                    this.FWLibraryName = value.Trim();
                    this.FWLibraryVersion = string.Empty;
                }
                else
                {
                    this.FWLibraryName = value.Substring(0, pos).Trim();
                    //
                    string fwLibraryVersion = value.Substring(pos + 1).Trim();
                    if (fwLibraryVersion.EndsWith(")"))
                        fwLibraryVersion = fwLibraryVersion.Remove(fwLibraryVersion.Length - 1).Trim();
                    this.FWLibraryVersion = fwLibraryVersion;
                }
            }
        }

        #endregion

        #region SuffixDecl

        /// <summary>
        /// 得到功能块的后缀描述字符串。
        /// </summary>
        /// <param name="type">后缀描述字符串类型</param>
        /// <returns>功能块的后缀描述字符串。</returns>
        public string GetSuffixDecl(ViFirmBlockSuffixType type)
        {
            string decl = string.Empty;

            if (((uint)(type & ViFirmBlockSuffixType.Modes)) != 0)
                decl += "#Modes:" + this.ModesDecl;
            else if (((uint)(type & ViFirmBlockSuffixType.SModes)) != 0)
                decl += "#" + this.ModesDecl;

            if (((uint)(type & ViFirmBlockSuffixType.FWLibrary)) != 0)
            {
                string fwLibraryDecl = this.FWLibraryDecl;
                if (!string.IsNullOrEmpty(fwLibraryDecl))
                    decl += "#FwLib:" + fwLibraryDecl;
            }

            if (((uint)(type & ViFirmBlockSuffixType.Comment)) != 0)
            {
                string comment = this.Comment;
                if (!string.IsNullOrEmpty(comment))
                    decl += "@" + comment;
            }

            return decl;
        }

        /// <summary>
        /// 解析功能块后缀描述字符串的信息。
        /// </summary>
        /// <param name="decl">功能块后缀描述字符串。</param>
        public void SetSuffixDecl(string decl)
        {
            if (decl == null) return;

            int start = decl.IndexOfAny("#@");
            if (start < 0) return;

            while (start >= 0)
            {
                if (decl[start] == '@')
                {
                    this.Comment = decl.Substring(start + 1).Trim();
                    break;
                }
                else
                {
                    int next = decl.IndexOfAny("#@", start + 1), comma;
                    string part = (next < 0 ? decl.Substring(start + 1) :
                        decl.Substring(start + 1, next - start - 1)).Trim(), key;
                    //
                    comma = part.IndexOf(':');
                    if (comma < 0)
                    {
                        this.ModesDecl = part;
                    }
                    else
                    {
                        key = part.Substring(0, comma).TrimEnd();
                        if (key.Equals("Modes", StringComparison.OrdinalIgnoreCase))
                            this.ModesDecl = part.Substring(comma + 1).TrimStart();
                        else if (key.Equals("FwLib", StringComparison.OrdinalIgnoreCase))
                            this.FWLibraryDecl = part.Substring(comma + 1).TrimStart();
                    }

                    // 下一个
                    start = next;
                }
            }
        }

        #endregion

        #region Prototype

        /// <summary>
        /// 功能块原型描述字符串。
        /// </summary>
        public string Prototype
        {
            get
            {
                return this.ToString(false, null, null, ViFirmBlockSuffixType.Modes);
            }
        }

        #endregion

        #region ToString

        /// <summary>
        /// 重载转化成字符串的函数，调试时可以很方便地显示对象信息。
        /// </summary>
        public override string ToString()
        {
            return this.ToString(true);
        }

        /// <summary>
        /// 重载转化成字符串的函数，调试时可以很方便地显示对象信息。
        /// </summary>
        /// <param name="comment">是否包含注释信息？</param>
        public virtual string ToString(bool comment)
        {
            return this.ToString(comment, string.Empty, ViTextFile.Indent,
                (comment ? ViFirmBlockSuffixType.Comment : ViFirmBlockSuffixType.None));
        }

        /// <summary>
        /// 将对象转化成字符串显示。
        /// </summary>
        /// <param name="comment">是否包含注释信息？</param>
        /// <param name="leading">每行的前缀</param>
        /// <param name="tabbing">每个缩进的字符串</param>
        /// <param name="suffixType">功能块后缀描述类型</param>
        /// <returns>对象的字符串显示。</returns>
        public virtual string ToString(bool comment, string leading, string tabbing, ViFirmBlockSuffixType suffixType)
        {
            if (leading == null) leading = string.Empty;
            if (tabbing == null) tabbing = string.Empty;

            string str = leading, prefix;
            if (this.IsFunctionBlock)
            {
                str += string.Format("{0} {1}\n", "FUNCTION_BLOCK", this.Name);
            }
            else
            {
                str += string.Format("{0} {1} : {2}\n", "FUNCTION", this.Name, this.Type.Name);
            }

            // Task Usage
            if (comment)
            {
                str += string.Format("\n{0}(* task usage: {1} *)\n\n", leading, this.ModesDecl);
            }

            // INPUT
            if (this.InOutConnectorCount < this.InputConnectors.Count)
            {
                str += leading + tabbing + "VAR_INPUT\n";
                prefix = leading + tabbing + tabbing;
                for (int i = this.InOutConnectorCount; i < this.InputConnectors.Count; ++i)
                {
                    ViConnType connType = this.InputConnectors[i];
                    str += string.Format("{0}{1}\n", prefix, connType.ToString(comment));
                }
                str += leading + tabbing + "END_VAR\n";
            }

            // OUTPUT
            if (this.InOutConnectorCount < this.OutputConnectors.Count)
            {
                str += leading + tabbing + "VAR_OUTPUT\n";
                prefix = leading + tabbing + tabbing;
                for (int i = this.InOutConnectorCount; i < this.OutputConnectors.Count; ++i)
                {
                    ViConnType connType = this.OutputConnectors[i];
                    str += string.Format("{0}{1}\n", prefix, connType.ToString(comment));
                }
                str += leading + tabbing + "END_VAR\n";
            }

            // IN_OUT
            if (this.InOutConnectorCount > 0)
            {
                str += leading + tabbing + "VAR_IN_OUT\n";
                prefix = leading + tabbing + tabbing;
                for (int i = 0; i < this.InOutConnectorCount; ++i)
                {
                    ViConnType connType = this.InputConnectors[i];
                    str += string.Format("{0}{1}\n", prefix, connType.ToString(comment));
                }
                str += leading + tabbing + "END_VAR\n";
            }

            if (this.IsFunctionBlock)
            {
                str += string.Format("{0}{1}", leading, "END_FUNCTION_BLOCK");
            }
            else
            {
                str += string.Format("{0}{1}", leading, "END_FUNCTION");
            }

            // Suffix Decl
            str += string.Format("{0}{1}", this.GetSuffixDecl(suffixType), "\n");

            return str;
        }

        #endregion

        #region Connector

        /// <summary>
        /// 输入管脚集合。
        /// </summary>
        public ViObservableCollection<ViConnType> InputConnectors { get; protected set; }

        /// <summary>
        /// 输出管脚集合。
        /// </summary>
        public ViObservableCollection<ViConnType> OutputConnectors { get; protected set; }

        /// <summary>
        /// 具有 IN/OUT 两个特性的管脚的个数。这些管脚需要被存放在 Input/Output 管脚数组的头部。
        /// </summary>
        public int InOutConnectorCount { get; internal set; }

        #endregion

        #region Source

        /// <summary>
        /// 生成该功能块的数据源。
        /// </summary>
        public ViPouSource PouSource
        {
            get
            {
                return this.Parent as ViPouSource;
            }
            internal set
            {
                this.Parent = value;
            }
        }

        /// <summary>
        /// POU（功能块）特性。
        /// </summary>
        public ViPouAttributes PouAttributes
        {
            get
            {
                return (this.PouSource == null ? ViPouAttributes.None : this.PouSource.PouAttributes)
                     | (this.IsFunctionBlock ? ViPouAttributes.FunctionBlock : ViPouAttributes.Function);
            }
        }

        #endregion

        #region 序列化

        public const string TAG = "firmBlockType";

        public bool SerializeTo(ViBlur blur, System.Xml.XmlWriter writer)
        {
            writer.WriteStartElement(ViFirmBlockType.TAG);
            {
                writer.WriteAttributeString(blur, "name", this.Name);
                writer.WriteAttributeString("category", this.Category);
                writer.WriteAttributeString("mode", this.ModesDecl);

                //dataType
                if (this.Type != null)
                {
                    writer.WriteStartElement("dataType");
                    {
                        writer.WriteAttributeString("typeName", this.Type.Name);
                        writer.WriteAttributeString("typeShortName", this.Type.ShortName);
                    }
                    writer.WriteEndElement();
                }

                //InputConnectors
                writer.WriteStartElement("inputVars");
                if (this.InputConnectors != null && this.InputConnectors.Count > 0)
                {
                    foreach (ViConnType item in this.InputConnectors)
                    {
                        item.SerializeTo(blur, writer);
                    }
                }
                writer.WriteEndElement();

                //OutputConnectors
                writer.WriteStartElement("outputVars");
                if (this.OutputConnectors != null && this.OutputConnectors.Count > 0)
                {
                    foreach (ViConnType item in this.OutputConnectors)
                    {
                        item.SerializeTo(blur, writer);
                    }
                }
                writer.WriteEndElement();

                //InOutputConnectorCount
                if (this.InOutConnectorCount > 0)
                {
                    writer.WriteStartElement("inOutVars");
                    {
                        writer.WriteAttributeString("count", this.InOutConnectorCount.ToString());
                    }
                    writer.WriteEndElement();
                }
            }
            writer.WriteEndElement();

            return true;
        }

        public bool DeserializeFrom(ViBlur blur, System.Xml.XmlReader reader)
        {
            this.Name = reader.GetAttribute(blur, "name");
            string category = reader.GetAttribute("category");
            if (!string.IsNullOrEmpty(category))
                this.Category = category;
            string mode = reader.GetAttribute("mode");
            if (!string.IsNullOrEmpty(mode))
                this.ModesDecl = mode;

            if (reader.IsEmptyElement)
                return true;

            int stage = 0x00;
            // 0x00: 未开始
            // 0x01: 开始 INPUT
            // 0x02: 开始 OUTPUT
            // 0x03: 开始 IN_OUT

            while (!reader.EOF)
            {
                if (!reader.Read()) break;
                if (reader.NodeType == System.Xml.XmlNodeType.Whitespace) continue;

                if (reader.NodeType == System.Xml.XmlNodeType.Element)
                {
                    if (string.Compare(reader.Name, "dataType", true) == 0)
                    {
                        string typeName = reader.GetAttribute("typeName");
                        string typeShortName = reader.GetAttribute("typeShortName");
                        ViDataType dataType = new ViDataType(typeName, typeShortName);
                        this.Type = dataType;
                    }
                    else if (string.Compare(reader.Name, "inputVars", true) == 0)
                    {
                        this.InputConnectors.Clear();
                        stage = 0x01;
                    }
                    else if (string.Compare(reader.Name, "outputVars", true) == 0)
                    {
                        this.OutputConnectors.Clear();
                        stage = 0x02;
                    }
                    else if (string.Compare(reader.Name, "inOutVars", true) == 0)
                    {
                        string count = reader.GetAttribute("count");
                        if (!string.IsNullOrEmpty(count))
                            this.InOutConnectorCount = int.Parse(count);
                    }
                    else if (string.Compare(reader.Name, ViConnType.TAG, true) == 0)
                    {
                        ViConnType connType = new ViConnType(null, null);
                        if (!connType.DeserializeFrom(blur, reader))
                            return false;

                        if (stage == 0x01)
                        {
                            this.InputConnectors.Add(connType);
                        }
                        else if (stage == 0x02)
                        {
                            this.OutputConnectors.Add(connType);
                        }
                    }
                }
                else if (reader.NodeType == System.Xml.XmlNodeType.EndElement)
                {
                    if (string.Compare(reader.Name, ViFirmBlockType.TAG, true) == 0)
                        return true;
                }
            }

            return false;
        }

        #endregion
    }
}

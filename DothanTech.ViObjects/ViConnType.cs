/// <summary>
/// @file   ViConnType.cs
///	@brief  ViGET 编辑器中功能块管脚类型。
/// @author	DothanTech 刘伟宏
/// 
/// Copyright(C) 2011~2014, DothanTech. All rights reserved.
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections;
using Dothan.Helpers;

namespace Dothan.ViObject
{
    /// <summary>
    /// ViGET 编辑器中功能块管脚的特性。
    /// </summary>
    [Flags]
    public enum ViConnAttributes
    {
        None = 0x0000,
        F_EDGE = 0x0001,
        R_EDGE = 0x0002,
    }

    /// <summary>
    /// ViGET 编辑器中功能块管脚类型。
    /// </summary>
    public class ViConnType : ViType, IViSerializeBlur
    {
        #region Life cycle

        /// <summary>
        /// 构建对象。
        /// </summary>
        /// <param name="name">管脚名称</param>
        /// <param name="dataType">管脚数据类型</param>
        public ViConnType(string name, ViDataType dataType)
            : base(name, dataType)
        {
        }

        #endregion

        #region Type

        /// <summary>
        /// 得到管脚数据类型。
        /// </summary>
        /// <returns>管脚数据类型。</returns>
        public new ViDataType GetType()
        {
            return this.Type;
        }

        /// <summary>
        /// 得到管脚数据类型。
        /// </summary>
        public new ViDataType Type
        {
            get
            {
                return base.Type as ViDataType;
            }
            set
            {
                base.Type = value;
            }
        }

        #endregion

        #region Attributes

        /// <summary>
        /// ViGET 编辑器中功能块管脚的特性。
        /// </summary>
        public ViConnAttributes Attributes = ViConnAttributes.None;

        #endregion

        #region D DefaultValue

        public static readonly DependencyProperty DefaultValueProperty =
            DependencyProperty.Register("DefaultValue", typeof(string), typeof(ViConnType),
                                        new FrameworkPropertyMetadata(string.Empty));

        /// <summary>
        /// 对象备注信息。
        /// </summary>
        public string DefaultValue
        {
            get
            {
                return (string)GetValue(DefaultValueProperty);
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    ClearValue(DefaultValueProperty);
                else
                    SetValue(DefaultValueProperty, value);
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
        public virtual string ToString(bool comment)
        {
            // 名称
            string str = this.Name;

            if (this.Type != null)
            {
                // 类型
                str += " : " + this.Type.Name;

                // 特性
                if (this.Attributes == ViConnAttributes.F_EDGE)
                    str += " F_EDGE";
                if (this.Attributes == ViConnAttributes.R_EDGE)
                    str += " R_EDGE";

                // 缺省值
                if (!string.IsNullOrEmpty(this.DefaultValue))
                {
                    str += " := " + this.DefaultValue;
                }
            }

            // 变量定义结束符
            str += ';';

            // 备注
            if (comment)
            {
                string commentDecl = this.GetCommentDecl();
                if (!string.IsNullOrEmpty(commentDecl))
                    str += string.Format(" {0}{1}{2}", ViTextFile.CommentBegin, commentDecl, ViTextFile.CommentEnd);
            }

            return str;
        }

        /// <summary>
        /// 得到备注的描述字符串（不包括前后的 (* *)）。
        /// </summary>
        /// <returns>备注的描述字符串。</returns>
        public virtual string GetCommentDecl()
        {
            return this.Comment;
        }

        #endregion

        #region Parse

        /// <summary>
        /// 将文本行描述的变量定义，解析为管脚类型对象。
        /// </summary>
        /// <param name="decl">管脚的文本行描述</param>
        /// <param name="typeCreation">自动创建管脚数据类型的方式</param>
        /// <returns>管脚类型对象，null 表示解析失败</returns>
        public static ViConnType Parse(string decl, ViTypeCreation typeCreation)
        {
            return Parse(decl, typeCreation, (name, dataType) => new ViConnType(name, dataType));
        }

        /// <summary>
        /// 将文本行描述的变量定义，解析为管脚类型对象。
        /// </summary>
        /// <param name="decl">管脚的文本行描述</param>
        /// <param name="typeCreation">自动创建管脚数据类型的方式</param>
        /// <param name="creator">ViConnType 的创建函数</param>
        /// <returns>管脚类型对象，null 表示解析失败</returns>
        public static ViConnType Parse(string decl, ViTypeCreation typeCreation, Func<string, ViDataType, ViConnType> creator)
        {
            string name = null, type = null, comment = null; string[] addiInfo = null;

            if (!SeperateNameType(decl.Trim(), ref name, ref type, ref addiInfo, ref comment))
                return null;

            ViDataType dataType = null;
            if (!string.IsNullOrEmpty(type))
            {
                dataType = ViDataType.GetDataType(type, typeCreation);
                if (dataType == null)
                    return null;
            }

            ViConnType connType = creator(name, dataType);
            if (connType == null)
                return null;

            // 备注
            connType.Comment = comment;

            if (addiInfo != null)
            {
                // 特性
                foreach (string info in addiInfo)
                {
                    if (info.Equals("F_EDGE", StringComparison.OrdinalIgnoreCase))
                        connType.Attributes |= ViConnAttributes.F_EDGE;
                    else if (info.Equals("R_EDGE", StringComparison.OrdinalIgnoreCase))
                        connType.Attributes |= ViConnAttributes.R_EDGE;
                }

                // 缺省值
                if (addiInfo.Count() >= 2 && addiInfo[addiInfo.Count() - 2] == ":=")
                {
                    connType.DefaultValue = addiInfo[addiInfo.Count() - 1];
                }
            }

            return connType;
        }

        /// <summary>
        /// 将名称类型描述拆分出：名称、类型、附加信息。
        ///   名称 : 类型 {附加信息} ; 注释
        /// </summary>
        /// <param name="decl">名称类型描述</param>
        /// <param name="name">返回拆分出的名称</param>
        /// <param name="type">返回拆分出的类型</param>
        /// <param name="addiInfo">返回拆分出的附加信息</param>
        /// <returns>是否成功拆分？</returns>
        public static bool SeperateNameType(string decl, ref string name, ref string type, ref string[] addiInfo, ref string comment)
        {
            decl = decl.Trim();
            if (string.IsNullOrEmpty(decl))
                return false;

            // 分号之后是行结束或者注释
            int start = 0, pos, pos2;
            while (true)
            {
                pos = decl.IndexOf(';', start);
                if (pos < 0) break;

                // 单引号用来定义字符串，需要过滤掉字符串中的分号
                pos2 = decl.IndexOf('\'', start);
                if (pos2 < 0) break;

                // 引号在分号后面，就不管了
                if (pos2 > pos) break;

                start = decl.IndexOf('\'', pos2 + 1) + 1;
                if (start <= 0)
                {
                    pos = -1;
                    break;
                }
            }
            if (pos >= 0)
            {
                comment = decl.Substring(pos + 1).Trim();
                decl = decl.Substring(0, pos).Trim();

                if (comment.Length >= 4 &&
                    comment.StartsWith(ViTextFile.CommentBegin) &&
                    comment.EndsWith(ViTextFile.CommentEnd))
                {
                    comment = comment.Substring(2, comment.Length - 4);
                }
                else
                {
                    comment = null;
                }
            }
            else
            {
                comment = null;
            }

            // 以冒号分隔名称和类型
            pos = decl.IndexOf(':');
            if (pos < 0)
            {
                name = decl;
                type = string.Empty;
                addiInfo = null;
            }
            else
            {
                name = decl.Substring(0, pos).Trim();
                //
                ArrayList parts = decl.Substring(pos + 1).Trim().SplitComma();
                if (parts == null || parts.Count <= 0) return false;

                // 类型名称
                type = parts[0] as string;

                // 附加信息
                if (parts.Count > 1)
                {
                    parts.RemoveAt(0);
                    addiInfo = (string[])parts.ToArray(typeof(string));
                }
                else
                {
                    addiInfo = null;
                }
            }

            return (!string.IsNullOrEmpty(name));
        }

        #endregion

        #region Connectable

        public static readonly DependencyProperty ConnectableProperty =
            DependencyProperty.Register("Connectable", typeof(bool), typeof(ViConnType),
                                        new FrameworkPropertyMetadata(true));

        /// <summary>
        /// 表示对象是否可被连接。
        /// </summary>
        public bool Connectable
        {
            get
            {
                return (bool)GetValue(ConnectableProperty);
            }
            set
            {
                SetValue(ConnectableProperty, value);
            }
        }

        #endregion

        #region Fast

        public static readonly DependencyProperty FastProperty =
            DependencyProperty.Register("Fast", typeof(bool), typeof(ViConnType),
                                        new FrameworkPropertyMetadata(false));

        /// <summary>
        /// 表示对象是否连接速度。
        /// </summary>
        public bool Fast
        {
            get
            {
                return (bool)GetValue(FastProperty);
            }
            set
            {
                SetValue(FastProperty, value);
            }
        }

        #endregion

        #region 管教路径解析函数

        /// <summary>
        /// 获取给定路径的 CPU 名称。
        /// </summary>
        public static string RetrieveCpuName(string path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;

            int firstSeparate = path.IndexOf('\\');
            int secondSeparate = path.IndexOf('\\', firstSeparate + 1);
            bool isNewVersion = path[secondSeparate - 1] == ')';

            if (isNewVersion)
            {
                int iEnd = path.IndexOf('\\');
                return path.Substring(0, iEnd).Trim();
            }

            return string.Empty;
        }

        /// <summary>
        /// 获取指定路径的 Plan（Program）名称。
        /// </summary>
        public static string RetrievePlanName(string path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;

            int firstSeparate = path.IndexOf(ViNamedObject.PathSeperator);
            int secondSeparate = path.IndexOf(ViNamedObject.PathSeperator, firstSeparate + 1);
            if (firstSeparate < 0 || secondSeparate <= 0)
                return null;

            bool isNewVersion = path[secondSeparate - 1] == ')';

            if (isNewVersion)
            {
                int iEnd = path.IndexOf('(', firstSeparate + 1);
                return path.Substring(firstSeparate + 1, iEnd - firstSeparate - 2).Trim();
            }

            return path.Substring(0, firstSeparate).Trim();
        }

        /// <summary>
        /// 获取给定路径中中功能块名称。
        /// </summary>
        public static string RetrieveBlockName(string path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;

            int endIndex = path.LastIndexOf(ViNamedObject.PathSeperator);
            string pathWidthoutConn = path.Substring(0, endIndex);
            int startIndex = pathWidthoutConn.LastIndexOf(ViNamedObject.PathSeperator);
            string sBlockName = pathWidthoutConn.Substring(startIndex + 1);
            return sBlockName.Trim();
        }

        /// <summary>
        /// 获取给定路径中的管脚名称。
        /// </summary>
        public static string RetrieveConnectorName(string path)
        {
            if (path.Length == 0)
                return string.Empty;

            int endIndex = path.LastIndexOf(ViNamedObject.PathSeperator);
            if (endIndex >= 0)
            {
                string connectorName = path.Substring(endIndex + 1);
                int commentSeparator = connectorName.IndexOf('*');
                if (commentSeparator > -1)
                    connectorName = connectorName.Substring(0, commentSeparator);

                return connectorName.Trim();
            }

            return string.Empty;
        }

        /// <summary>
        /// 从管脚路径中获取管脚所在的页码信息；
        /// </summary>
        /// <param name="path">待解析管脚路径</param>
        /// <param name="pagePath">是否需要返回页面全路径？true：返回Page全路径；false：只返回page的页码；</param>
        /// <returns>页码名称或者路径</returns>
        public static string RetrievePageName(string path, bool pagePath = false)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;

            string tempPath = path;
            // 去掉管脚名称；
            int endIndex = tempPath.LastIndexOf(ViNamedObject.PathSeperator);
            tempPath = tempPath.Substring(0, endIndex);
            // 去掉功能块名称；
            endIndex = tempPath.LastIndexOf(ViNamedObject.PathSeperator);
            tempPath = tempPath.Substring(0, endIndex);

            if (pagePath)
                return tempPath;

            int startIndex = tempPath.LastIndexOf('(');
            endIndex = tempPath.LastIndexOf(')');

            return tempPath.Substring(startIndex + 1, endIndex - startIndex - 1).Trim();
        }

        #endregion

        #region 序列化

        public const string TAG = "connType";

        public bool SerializeTo(ViBlur blur, System.Xml.XmlWriter writer)
        {
            if (this.Type != null)
            {
                writer.WriteStartElement(ViConnType.TAG);
                {
                    writer.WriteAttributeString(blur, "name", this.Name);
                    writer.WriteAttributeString("connectable", this.Connectable.ToString());
                    writer.WriteAttributeString("fast", this.Fast.ToString());
                    writer.WriteAttributeString("attributes", this.Attributes.ToString());
                    writer.WriteAttributeString("defaultValue", this.DefaultValue);

                    //DataType
                    if (this.Type != null)
                    {
                        writer.WriteStartElement("dataType");
                        {
                            writer.WriteString(this.Type.Name);
                        }
                        writer.WriteEndElement();
                    }
                }
                writer.WriteEndElement();
            }

            return true;
        }

        public bool DeserializeFrom(ViBlur blur, System.Xml.XmlReader reader)
        {
            string name = reader.GetAttribute(blur, "name");
            string connectable = reader.GetAttribute("connectable");
            string fast = reader.GetAttribute("fast");
            string attributes = reader.GetAttribute("attributes");
            string defaultValue = reader.GetAttribute("defaultValue");
            string connected = reader.GetAttribute("connected");

            //Name
            if (!string.IsNullOrEmpty(name))
                this.Name = name;
            //Connectable
            if (!string.IsNullOrEmpty(connectable))
                this.Connectable = bool.Parse(connectable);
            //Fast
            if (!string.IsNullOrEmpty(fast))
                this.Fast = bool.Parse(fast);
            //Attributes
            if (!string.IsNullOrEmpty(attributes))
            {
                if (string.Compare(attributes, ViConnAttributes.F_EDGE.ToString(), true) == 0)
                    this.Attributes = ViConnAttributes.F_EDGE;
                else if (string.Compare(attributes, ViConnAttributes.R_EDGE.ToString(), true) == 0)
                    this.Attributes = ViConnAttributes.R_EDGE;
                else
                    this.Attributes = ViConnAttributes.None;
            }
            if (!string.IsNullOrEmpty(defaultValue))
                this.DefaultValue = defaultValue;

            while (!reader.EOF)
            {
                if (!reader.Read()) break;
                if (reader.NodeType == System.Xml.XmlNodeType.Whitespace) continue;

                if (reader.NodeType == System.Xml.XmlNodeType.Element)
                {
                    if (string.Compare(reader.Name, "dataType", true) == 0)
                    {
                        string typeName = reader.GetAttribute("typeName");
                        //读取Value
                        if (!reader.IsEmptyElement && string.IsNullOrEmpty(typeName))
                        {
                            if (!reader.Read()) break;
                            typeName = reader.Value;
                        }
                        ViDataType dataType = ViDataType.GetDataType(typeName);
                        this.Type = dataType;
                    }
                }
                else if (reader.NodeType == System.Xml.XmlNodeType.EndElement)
                {
                    if (string.Compare(reader.Name, ViConnType.TAG, true) == 0)
                        return true;
                }
            }

            return false;
        }

        #endregion
    }
}

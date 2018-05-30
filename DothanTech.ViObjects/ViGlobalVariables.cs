/// <summary>
/// @file   ViGlobalVariables.cs
///	@brief  ViGET Global Variables 类型文件的管理。
/// @author	DothanTech 刘伟宏
/// 
/// Copyright(C) 2011~2014, DothanTech. All rights reserved.
/// </summary>

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Diagnostics;
using System.Collections.ObjectModel;

using Dothan.Helpers;

namespace Dothan.ViObject
{
    /// <summary>
    /// ViGET Global Variables 类型文件的管理。
    /// </summary>
    public class ViGlobalVariables : MappedTreeObject
    {
        #region Life cycle

        /// <summary>
        /// 构建对象。
        /// </summary>
        public ViGlobalVariables()
        {
        }

        /// <summary>
        /// 构建指定名称的对象。
        /// </summary>
        /// <param name="name">对象名称</param>
        public ViGlobalVariables(string name)
            : base(name)
        {
        }

        #endregion

        /// <summary>
        /// 文件头部的行。
        /// </summary>
        protected string HeadLines = string.Empty;

        #region Find

        public ViGlobalVariable GetGlobalVariable(string uuid)
        {
            if (string.IsNullOrEmpty(uuid))
                return null;
            if (!ViGlobalVariable.CheckUUID(uuid))
                return null;

            if (this.Children.Count > 0)
                return this.Children.Where((o) => ((ViGlobalVariable)o).UUID == uuid).FirstOrDefault() as ViGlobalVariable;

            return null;
        }

        #endregion

        #region Remove

        /// <summary>
        /// 删除指定的全局变量。
        /// </summary>
        public virtual bool RemoveGlobalVariable(ViGlobalVariable var)
        {
            if (var == null)
                return false;

            return this.Children.Remove(var);
        }

        /// <summary>
        /// 删除指定 UUID 所对应的全局变量。
        /// </summary>
        public virtual bool RemoveGlobalVariable(string uuid)
        {
            return this.RemoveGlobalVariable(this.GetGlobalVariable(uuid));
        }

        #endregion

        #region Load

        /// <summary>
        /// 从指定文件中读取信息。
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <returns>成功/失败？</returns>
        public virtual bool Load(string fileName)
        {
            try
            {
                using (StreamReader reader = new StreamReader(fileName, Encoding.Default))
                    return this.Load(reader);
            }
            catch (Exception ee)
            {
                Trace.WriteLine("### [" + ee.Source + "] Exception: " + ee.Message);
                Trace.WriteLine("### " + ee.StackTrace);
            }
            return false;
        }

        /// <summary>
        /// 从指定文件中读取信息。
        /// </summary>
        /// <param name="reader">文件对象</param>
        /// <returns>成功/失败？</returns>
        public virtual bool Load(TextReader reader)
        {
            // 先清空内容
            this.Name = null;
            HeadLines = string.Empty;
            this.DeleteAll();

            // 0: 文件头部
            // 1: 读取到 PROGRAM
            // 2: 读取到 VAR_GLOBAL
            // 3: 文件尾部
            int stage = 0;
            while (true)
            {
                string line = reader.ReadLine(), trim;
                if (line == null) break;

                trim = line.Trim();
                switch (stage)
                {
                    case 0:
                        if (trim.StartsWith("PROGRAM", StringComparison.OrdinalIgnoreCase))
                        {
                            stage = 1;
                            if (trim.Length > "PROGRAM".Length)
                                this.Name = trim.Substring(7).Trim();
                        }
                        else
                        {
                            HeadLines += line + '\n';
                        }
                        break;
                    case 1:
                        if (trim.StartsWith("VAR_GLOBAL", StringComparison.OrdinalIgnoreCase))
                        {
                            stage = 2;
                        }
                        break;
                    case 2:
                        if (trim.StartsWith("END_VAR", StringComparison.OrdinalIgnoreCase))
                        {
                            stage = 3;
                        }
                        else if (!string.IsNullOrEmpty(trim))
                        {
                            this.OnVariableLine(trim);
                        }
                        break;
                }
            }

            return (!string.IsNullOrEmpty(this.Name));
        }

        /// <summary>
        /// 读取一行变量描述信息。
        /// </summary>
        /// <param name="line">变量文本行描述信息</param>
        /// <returns>成功/失败？</returns>
        protected virtual bool OnVariableLine(string line)
        {
            if (line.StartsWith(ViTextFile.CommentBegin))
                return true;

            ViGlobalVariable connType = ViGlobalVariable.Parse(line, ViTypeCreation.Create);
            if (connType == null || connType.Type == null)
                return false;

            this.AddChild(connType);
            return true;
        }

        #endregion

        #region Save

        /// <summary>
        /// 将信息保存到文件中。
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <returns>成功/失败？</returns>
        public virtual bool Save(string fileName)
        {
            return this.Save(fileName, false);
        }

        /// <summary>
        /// 将信息保存到文件中。
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <param name="force">是否强制保存？（即使文件内容没有修改也更新文件）</param>
        /// <returns>成功/失败？</returns>
        public virtual bool Save(string fileName, bool force)
        {
            if (string.IsNullOrEmpty(this.Name))
                this.Name = Path.GetFileNameWithoutExtension(fileName);

            string content = GetContent();
            if (content == null)
                return false;

            if (!force && ViTextFile.CompareContent(content, fileName))
                return true;

            try
            {
                using (StreamWriter writer = ViTextFile.CreateWriter(fileName))
                    return this.Write(writer, content);
            }
            catch (Exception ee)
            {
                Trace.WriteLine("### [" + ee.Source + "] Exception: " + ee.Message);
                Trace.WriteLine("### " + ee.StackTrace);
            }
            return false;
        }

        /// <summary>
        /// 将信息保存到文件中。
        /// </summary>
        /// <param name="writer">文件对象</param>
        /// <returns>成功/失败？</returns>
        public virtual bool Save(TextWriter writer)
        {
            this.SaveLeading(writer);
            this.SaveVariables(writer);
            this.SaveTailing(writer);
            return true;
        }

        /// <summary>
        /// 得到信息的文本行描述。
        /// </summary>
        /// <returns>信息的文本行描述。</returns>
        public virtual string GetContent()
        {
            try
            {
                StringBuilder sbContent = new StringBuilder();
                using (StringWriter writer = ViTextFile.CreateWriter(sbContent))
                {
                    if (this.Save(writer))
                        return sbContent.ToString();
                }
            }
            catch (Exception ee)
            {
                Trace.WriteLine("### [" + ee.Source + "] Exception: " + ee.Message);
                Trace.WriteLine("### " + ee.StackTrace);
            }
            return null;
        }

        /// <summary>
        /// 将文本内容，写入文件。
        /// </summary>
        /// <param name="writer">文件对象</param>
        /// <param name="content">需要写入的文本</param>
        /// <returns>成功与否？</returns>
        protected virtual bool Write(TextWriter writer, string content)
        {
            writer.Write(content);
            return true;
        }

        /// <summary>
        /// 保存信息头部。
        /// </summary>
        /// <param name="writer">文本对象</param>
        protected virtual void SaveLeading(TextWriter writer)
        {
            //writer.Write(HeadLines);
            writer.WriteLine("PROGRAM {0}", this.Name);
            writer.WriteLine("VAR_GLOBAL");
        }

        /// <summary>
        /// 保存变量信息。
        /// </summary>
        /// <param name="writer">文本对象</param>
        protected virtual void SaveVariables(TextWriter writer)
        {
            foreach (ViNamedObject obj in this.Children)
            {
                ViGlobalVariable connType = obj as ViGlobalVariable;
                if (connType != null)
                    writer.WriteLine(ViTextFile.Indent + connType.ToString(true));
            }
        }

        /// <summary>
        /// 保存信息尾部。
        /// </summary>
        /// <param name="writer">文本对象</param>
        protected virtual void SaveTailing(TextWriter writer)
        {
            writer.WriteLine("END_VAR");
            writer.WriteLine("END_PROGRAM");
        }

        #endregion
    }

    /// <summary>
    /// ViGET 编辑器中的变量类型。
    /// </summary>
    public class ViGlobalVariable : ViConnType
    {
        #region Life cycle

        /// <summary>
        /// 构建对象。
        /// </summary>
        /// <param name="name">管脚名称</param>
        /// <param name="dataType">管脚数据类型</param>
        public ViGlobalVariable(string name, ViDataType dataType)
            : base(name, dataType)
        {
        }

        #endregion

        #region UUID

        /// <summary>
        /// The UUID of current globalvariable.
        /// </summary>
        public string UUID
        {
            get { return this.uuid; }
            set
            {
                if (!ViGlobalVariable.CheckUUID(value))
                    throw new InvalidDataException();

                this.uuid = value;
            }
        }
        private string uuid;

        /// <summary>
        /// Check the whether the uuid is valid
        /// </summary>
        public static bool CheckUUID(string uuid)
        {
            if (string.IsNullOrEmpty(uuid))
                return false;

	        bool isValidUUID = uuid.Length == 36;
	        int hyphenPos = uuid.IndexOf('-', 0);
	        isValidUUID = isValidUUID && hyphenPos == 8;
            hyphenPos = uuid.IndexOf('-', hyphenPos + 1);
	        isValidUUID = isValidUUID && hyphenPos == 13;
            hyphenPos = uuid.IndexOf('-', hyphenPos + 1);
	        isValidUUID = isValidUUID && hyphenPos == 18;
            hyphenPos = uuid.IndexOf('-', hyphenPos + 1);
	        isValidUUID = isValidUUID && hyphenPos == 23;

	        return isValidUUID;
        }

        #endregion

        #region Parse

        /// <summary>
        /// 将文本行描述的变量定义，解析为变量对象。
        /// </summary>
        /// <param name="decl">变量的文本行描述</param>
        /// <param name="typeCreation">自动创建变量数据类型的方式</param>
        /// <returns>变量对象，null 表示解析失败</returns>
        public static new ViGlobalVariable Parse(string decl, ViTypeCreation typeCreation)
        {
            ViGlobalVariable variable = Parse(decl, typeCreation, (name, dataType) => new ViGlobalVariable(name, dataType)) as ViGlobalVariable;
            if (variable == null) return null;

            StringConfig config = new StringConfig(variable.Comment);
            variable.Comment = config["_C"];
            variable.UUID = config["UUID"];
            //
            return variable;
        }

        /// <summary>
        /// 得到备注的描述字符串（不包括前后的 (* *)）。
        /// </summary>
        /// <returns>备注的描述字符串。</returns>
        public override string GetCommentDecl()
        {
            StringConfig config = new StringConfig(string.Empty);

            if (!string.IsNullOrEmpty(this.Comment))
                config["_C"] = this.Comment;

            if (!string.IsNullOrEmpty(this.UUID))
                config["UUID"] = this.UUID;

            return config.ToString();
        }

        #endregion
    }
}

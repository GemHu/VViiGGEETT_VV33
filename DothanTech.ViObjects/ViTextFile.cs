/// <summary>
/// @file   ViTextFile.cs
///	@brief  ViGET 文本文件相关的操作。
/// @author	DothanTech 刘伟宏
/// 
/// Copyright(C) 2011~2014, DothanTech. All rights reserved.
/// </summary>

using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;

namespace Dothan.ViObject
{
    /// <summary>
    /// ViGET 文本文件相关的操作。
    /// </summary>
    public static class ViTextFile
    {
        /// <summary>
        /// 缩进字符串。
        /// </summary>
        public const string Indent = "  ";

        /// <summary>
        /// 注释行开始。
        /// </summary>
        public const string CommentBegin = "(*";

        /// <summary>
        /// 注释行结束。
        /// </summary>
        public const string CommentEnd = "*)";

        /// <summary>
        /// 换行字符串，保持和原来 ViGET 文件的兼容性。
        /// </summary>
        public const string NewLine = "\n";

        /// <summary>
        /// 比较两文本文件的内容。
        /// </summary>
        /// <param name="file1">文件1名称</param>
        /// <param name="file2">文件2名称</param>
        /// <returns>两文件是否完全相同？</returns>
        public static bool Compare(string file1, string file2)
        {
            if (string.IsNullOrEmpty(file1))
                return false;
            if (string.IsNullOrEmpty(file2))
                return false;

            if (file1.Equals(file2, StringComparison.OrdinalIgnoreCase))
                return true;

            try
            {
                using (StreamReader reader1 = new StreamReader(file1, Encoding.Default))
                using (StreamReader reader2 = new StreamReader(file2, Encoding.Default))
                {
#if true
                    return reader1.ReadToEnd() == reader2.ReadToEnd();
#else
                    string line1, line2;
                    while (true)
                    {
                        line1 = reader1.ReadLine();
                        line2 = reader2.ReadLine();
                        if (line1 == null)
                            return (line2 == null);
                        if (line2 == null)
                            return false;

                        if (line1 != line2)
                            return false;
                    }
#endif
                }
            }
            catch (Exception ee)
            {
                Trace.WriteLine("### [" + ee.Source + "] Exception: " + ee.Message);
                Trace.WriteLine("### " + ee.StackTrace);
            }
            return false;
        }

        /// <summary>
        /// 比较两文本文件的内容。
        /// </summary>
        /// <param name="content">第一个文本文件的内容</param>
        /// <param name="file">第二个文本文件的名称</param>
        /// <returns>两文件是否完全相同？</returns>
        public static bool CompareContent(string content, string file)
        {
            if (string.IsNullOrEmpty(file))
                return false;

            try
            {
                using (StreamReader reader = new StreamReader(file, Encoding.Default))
                    return CompareContent(content, reader);
            }
            catch (Exception ee)
            {
                Trace.WriteLine("### [" + ee.Source + "] Exception: " + ee.Message);
                Trace.WriteLine("### " + ee.StackTrace);
            }
            return false;
        }

        /// <summary>
        /// 比较两文本文件的内容。
        /// </summary>
        /// <param name="content">第一个文本文件的内容</param>
        /// <param name="file">第二个文本文件</param>
        /// <returns>两文件是否完全相同？</returns>
        public static bool CompareContent(string content, TextReader reader)
        {
#if true
            return content == reader.ReadToEnd();
#else
            string line1, line2;
            int start = 0, pos;
            while (true)
            {
                // 从文本中抽取一行
                if (start > content.Length)
                {
                    line1 = null;
                }
                else if (start == content.Length)
                {
                    line1 = string.Empty;
                    start = content.Length + 1;
                }
                else
                {
                    pos = content.IndexOf('\n', start);
                    if (pos < 0)
                    {
                        line1 = content.Substring(start);
                        start = content.Length + 1;
                    }
                    else
                    {
                        line1 = content.Substring(start, pos - start);
                        start = pos + 1;
                    }

                    // 去掉结尾的换行符
                    if (!string.IsNullOrEmpty(line1) && line1[line1.Length - 1] == '\r')
                        line1 = line1.Substring(0, line1.Length - 1);
                }

                // 从文件中读取一行
                line2 = reader.ReadLine();

                if (line1 == null)
                    return (line2 == null);
                if (line2 == null)
                    return false;

                if (line1 != line2)
                    return false;
            }
#endif
        }

        /// <summary>
        /// 创建文本文件的写对象。主要目的是为了统一文件采用何种换行符。
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <returns>文件写对象。</returns>
        public static StreamWriter CreateWriter(string fileName)
        {
            return CreateWriter(fileName, Encoding.Default);
        }

        /// <summary>
        /// 创建文本文件的写对象。主要目的是为了统一文件采用何种换行符。
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <param name="encoding">文件编码类型</param>
        /// <returns>文件写对象。</returns>
        public static StreamWriter CreateWriter(string fileName, Encoding encoding)
        {
            StreamWriter writer = new StreamWriter(fileName, false, encoding);
            writer.NewLine = ViTextFile.NewLine;
            return writer;
        }

        /// <summary>
        /// 创建字符串的写对象。主要目的是为了统一文件采用何种换行符。
        /// </summary>
        /// <param name="sb">字符串 StringBuilder</param>
        /// <returns>文件写对象。</returns>
        public static StringWriter CreateWriter(StringBuilder sb)
        {
            StringWriter writer = new StringWriter(sb);
            writer.NewLine = ViTextFile.NewLine;
            return writer;
        }
    }
}

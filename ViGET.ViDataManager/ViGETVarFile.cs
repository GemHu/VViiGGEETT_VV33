/// <summary>
/// @file   ViGETVarFile.cs
///	@brief  ViGET 工程管理器中操作工程 .VAR 文件的模块。
/// @author	DothanTech 刘伟宏
/// 
/// Copyright(C) 2011~2014, DothanTech. All rights reserved.
/// </summary>

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Dothan.Helpers;
using Dothan.ViObject;

namespace Dothan.Manager
{
    /// <summary>
    /// ViGET 工程管理器中操作工程 .VAR 文件的模块。
    /// </summary>
    public class ViGETVarFile : ViNamedObject
    {

        /// <summary>
        /// 构建对象。自动加载工程文件信息。
        /// </summary>
        /// <param name="varFileName">工程文件名称。</param>
        //public ViGETVarFile(string varFileName)
        //    : base(varFileName)
        //{
        //    this.LoadInfo(varFileName);
        //}

        /// <summary>
        /// 得到组合之后的全路径名称。原始文件名称中可能没有目录信息，可能没有扩展名信息。
        /// </summary>
        public static string GetFullFileName(string fileName, string basePath, string extName)
        {
            if (string.IsNullOrEmpty(fileName))
                return null;

            if (string.IsNullOrEmpty(Path.GetExtension(fileName)))
                fileName += extName;

            return FileName.GetAbsoluteFileName(basePath, fileName);
        }

        /// <summary>
        /// 得到保存到 INI 格式文件中的文件名称。
        /// </summary>
        /// <param name="fileName">文件名称。</param>
        /// <param name="basePath">基准目录。</param>
        /// <param name="withExt">是否需要文件扩展名？</param>
        /// <returns>保存到 INI 格式文件中的文件名称。</returns>
        public static string GetSaveFileName(string fileName, string basePath, bool withExt)
        {
            // 得到以 \ 开头的相对路径
            if (!string.IsNullOrEmpty(basePath))
            {
                basePath = FileName.GetSafePath(basePath);
                if (fileName.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
                    fileName = fileName.Substring(basePath.Length - 1); // 第一个字符为 \ 。
            }

            // 去掉文件扩展名
            if (!withExt)
                fileName = FileName.GetNewExtFileName(fileName, "");

            return fileName;
        }

        /// <summary>
        /// 保存文件列表信息到 INI 文件的指定章节。
        /// </summary>
        /// <param name="iniFile">INI 文件。</param>
        /// <param name="section">INI 文件的章节。</param>
        /// <param name="files">文件列表信息。</param>
        /// <param name="type">需要保存的文件类型，None 表示保存列表中的所有文件。</param>
        /// <param name="basePath">文件名称的基准目录。</param>
        /// <param name="withExt">保存信息中是否需要包含文件扩展名？</param>
        public static void SaveFilesInfo(IniFile iniFile, string section, Dictionary<string, PCSFile> files, FileType type, string basePath, bool withExt)
        {
            // 对文件类型进行过滤
            IEnumerable<PCSFile> enumFiles;
            if (type != FileType.None)
            {
                enumFiles = files.Values.Where((file) => file.Type == type);
            }
            else
            {
                enumFiles = files.Values;
            }

            // 将文件名称生成数组
            int index = 0;
            string[] strFiles = new string[enumFiles.Count()];
            foreach (var file in enumFiles)
                strFiles[index++] = GetSaveFileName(file.File, basePath, withExt);

            // 对名称数组进行排序
            strFiles.OrderBy((file) => file.ToUpper());

            SaveFilesInfo(iniFile, section, strFiles);
        }

        /// <summary>
        /// 保存文件列表信息到 INI 文件的指定章节。
        /// </summary>
        /// <param name="iniFile">INI 文件。</param>
        /// <param name="section">INI 文件的章节。</param>
        /// <param name="files">文件列表信息。</param>
        public static void SaveFilesInfo(IniFile iniFile, string section, IEnumerable<string> files)
        {
            ArrayList saKey = new ArrayList(), saValue = new ArrayList();

            // 章节中的其它信息放在前面
            Dictionary<string, string> values = iniFile.GetSection(section);
            foreach (var kvp in values)
            {
                if (kvp.Key.Equals("COUNT", StringComparison.OrdinalIgnoreCase))
                    continue;
                if (kvp.Key.IsPrefixIndex("FILE"))
                    continue;

                saKey.Add(kvp.Key); saValue.Add(kvp.Value);
            }

            // FILE 信息
            saKey.Add("COUNT"); saValue.Add(files.Count().ToString());
            int index = 0;
            foreach (var file in files)
            {
                saKey.Add("FILE" + index.ToString()); ++index;
                saValue.Add(file);
            }

            iniFile.SetSection(section, saKey, saValue);
        }

        /// <summary>
        /// 判断指定的文件名称信息在 INI 文件的指定章节中是否存在？
        /// </summary>
        /// <param name="iniFile">INI 配置文件</param>
        /// <param name="section">INI 章节名称</param>
        /// <param name="file">文件名称</param>
        /// <returns>文件名称是否存在？</returns>
        public static bool IsFileExist(IniFile iniFile, string section, string file)
        {
            if (string.IsNullOrEmpty(file))
                return false;

            Dictionary<string, string> values = iniFile.GetSection(section);
            foreach (var kvp in values)
            {
                if (kvp.Key.IsPrefixIndex("FILE"))
                {
                    if (kvp.Value.Equals(file, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 向 INI 文件的指定章节中添加指定的文件名称。
        /// </summary>
        /// <param name="iniFile">INI 配置文件</param>
        /// <param name="section">INI 章节名称</param>
        /// <param name="file">要添加的文件名称</param>
        /// <returns>文件名称是否成功添加？如果文件名称已经存在，则返回 false。</returns>
        public static bool AddFileInfo(IniFile iniFile, string section, string file)
        {
            if (string.IsNullOrEmpty(file))
                return false;

            ArrayList saKey = new ArrayList(), saValue = new ArrayList();
            int fileIndex = 0, fileCount = 0;

            // 章节中的其它信息放在前面
            Dictionary<string, string> values = iniFile.GetSection(section);
            foreach (var kvp in values)
            {
                if (kvp.Key.Equals("COUNT", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (kvp.Key.IsPrefixIndex("FILE"))
                {
                    if (kvp.Value.Equals(file, StringComparison.OrdinalIgnoreCase))
                        return false;

                    ++fileCount;
                }
                else
                {
                    saKey.Add(kvp.Key); saValue.Add(kvp.Value);
                }
            }

            // FILE 信息
            saKey.Add("COUNT"); saValue.Add((fileCount + 1).ToString());
            foreach (var kvp in values)
            {
                if (kvp.Key.IsPrefixIndex("FILE"))
                {
                    saKey.Add("FILE" + fileIndex.ToString()); ++fileIndex;
                    saValue.Add(kvp.Value);
                }
            }
            //
            saKey.Add("FILE" + fileIndex.ToString());
            saValue.Add(file);

            iniFile.SetSection(section, saKey, saValue);

            return true;
        }

        /// <summary>
        /// 修改 INI 文件的指定章节中指定的文件名称。
        /// </summary>
        /// <param name="iniFile">INI 配置文件</param>
        /// <param name="section">INI 章节名称</param>
        /// <param name="oldFile">旧的文件名称</param>
        /// <param name="newFile">新的文件名称</param>
        /// <returns>文件名称是否成功修改？如果文件名称没有变化，则返回 false。</returns>
        public static bool RenameFileInfo(IniFile iniFile, string section, string oldFile, string newFile)
        {
            if (string.IsNullOrEmpty(oldFile) || string.IsNullOrEmpty(newFile))
                return false;
            if (oldFile.Equals(newFile))
                return false;

            ArrayList saKey = new ArrayList(), saValue = new ArrayList();
            bool fileExist = false;

            // 章节中的其它信息放在前面
            Dictionary<string, string> values = iniFile.GetSection(section);
            foreach (var kvp in values)
            {
                if (kvp.Key.IsPrefixIndex("FILE") &&
                    kvp.Value.Equals(oldFile, StringComparison.OrdinalIgnoreCase))
                {
                    fileExist = true;

                    saKey.Add(kvp.Key); saValue.Add(newFile);
                }
                else
                {
                    saKey.Add(kvp.Key); saValue.Add(kvp.Value);
                }
            }
            if (!fileExist)
                return false;

            iniFile.SetSection(section, saKey, saValue);

            return true;
        }

        /// <summary>
        /// 从 INI 文件的指定章节中删除指定的文件名称。
        /// </summary>
        /// <param name="iniFile">INI 配置文件</param>
        /// <param name="section">INI 章节名称</param>
        /// <param name="file">要删除的文件名称</param>
        /// <returns>文件名称是否成功删除？</returns>
        public static bool DeleteFileInfo(IniFile iniFile, string section, string file)
        {
            if (string.IsNullOrEmpty(file))
                return false;

            ArrayList saKey = new ArrayList(), saValue = new ArrayList();
            int fileIndex = 0, fileCount = 0; bool fileExist = false;

            // 章节中的其它信息放在前面
            Dictionary<string, string> values = iniFile.GetSection(section);
            foreach (var kvp in values)
            {
                if (kvp.Key.Equals("COUNT", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (kvp.Key.IsPrefixIndex("FILE"))
                {
                    if (kvp.Value.Equals(file, StringComparison.OrdinalIgnoreCase))
                        fileExist = true;
                    else
                        ++fileCount;
                }
                else
                {
                    saKey.Add(kvp.Key); saValue.Add(kvp.Value);
                }
            }

            // 删除的文件不存在？
            if (!fileExist)
                return false;

            // FILE 信息
            saKey.Add("COUNT"); saValue.Add((fileCount).ToString());
            foreach (var kvp in values)
            {
                if (kvp.Key.IsPrefixIndex("FILE"))
                {
                    if (!kvp.Value.Equals(file, StringComparison.OrdinalIgnoreCase))
                    {
                        saKey.Add("FILE" + fileIndex.ToString()); ++fileIndex;
                        saValue.Add(kvp.Value);
                    }
                }
            }

            iniFile.SetSection(section, saKey, saValue);

            return true;
        }
    }
}

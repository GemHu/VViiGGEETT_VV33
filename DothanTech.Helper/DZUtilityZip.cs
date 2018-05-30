/*
 * Copyright (c) 2010, 许继电气股份有限公司深圳分公司
 * All rights reserved.
 * 
 * 文件名称：ZipUtil.cs
 * 文件标识：见配置管理计划书
 * 摘    要：功能块打包压缩工具类, 采用zip格式. 可解压winrar生成的zip格式, 不能解压rar格式.
 * 作    者：王春生(wcsbull@163.com)
 * 
 * 参    考：http://blog.csdn.net/kevin_cheung/archive/2008/12/04/3442164.aspx
 * 创建日期：V0.1, wcsbull, 20100810, Shenzhen。
 */
using System;
using System.IO;
using System.Collections.Generic;

using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace Dothan.Helpers
{
    /// <summary>
    /// 压缩及解压类
    /// </summary>
    public class ZipUtil
    {
        /// <summary>
        /// 选择性将多个目录和文件打包压缩
        /// </summary>
        /// <param name="FolderToZips">待压缩的文件夹列表，全路径格式</param>
        /// <param name="ZipedFile">压缩后的文件名，全路径格式</param>
        /// <returns></returns>
        // Dictionary<string, Dictionary<string,bool>> <folder, <fileext, true-select/false-deny>>
        public static List<string> PackageSelected(Dictionary<string, Dictionary<string, bool>> FolderToZips,
            string[] FilesToZip, string ZipedFile, string Password)
        {
            List<string> errors = new List<string>();

            ZipOutputStream s = new ZipOutputStream(File.Create(ZipedFile));
            s.SetLevel(6);
            s.Password = Password;

            //foreach (string FolderToZip in FolderToZips)
            foreach (KeyValuePair<string, Dictionary<string, bool>> FolderToZip in FolderToZips)
            {
                if (!Directory.Exists(FolderToZip.Key))
                {
                    errors.Add(FolderToZip.Key);
                    continue;
                }

                ZipFileDirectory(FolderToZip.Key, FolderToZip.Value, s, "");
            }

            //打包文件
            ZipEntry entry = null;
            FileStream fs = null;

            foreach (string FileToZip in FilesToZip)
            {
                //如果文件没有找到，则报错
                if (!File.Exists(FileToZip))
                {
                    //throw new System.IO.FileNotFoundException("指定要压缩的文件: " + FileToZip + " 不存在!");
                    errors.Add(FileToZip);
                    continue;
                }

                //打开待压缩文件
                fs = File.OpenRead(FileToZip);

                byte[] buffer = new byte[fs.Length];
                fs.Read(buffer, 0, buffer.Length);
                entry = new ZipEntry(Path.GetFileName(FileToZip));

                entry.DateTime = File.GetLastWriteTime(FileToZip);
                entry.Size = fs.Length;
                fs.Close();

                s.PutNextEntry(entry);

                s.Write(buffer, 0, buffer.Length);
            }

            s.Finish();
            s.Close();

            return errors;
        }

        /// <summary>
        /// 将多个目录打包压缩
        /// </summary>
        /// <param name="FolderToZips">待压缩的文件夹列表，全路径格式</param>
        /// <param name="ZipedFile">压缩后的文件名，全路径格式</param>
        /// <returns></returns>
        public static void PackageFolders(string[] FolderToZips, string ZipedFile, String Password)
        {
            ZipOutputStream s = new ZipOutputStream(File.Create(ZipedFile));
            s.SetLevel(6);
            s.Password = Password;

            //打包folder
            foreach (string FolderToZip in FolderToZips)
            {
                if (!Directory.Exists(FolderToZip))
                {
                    continue;
                }

                ZipFileDirectory(FolderToZip, null, s, "");
            }

            s.Finish();
            s.Close();
        }

        /// <summary>
        /// 将多个文件打包压缩
        /// </summary>
        /// <param name="FileToZip">要进行压缩的文件名</param>
        /// <param name="ZipedFile">压缩后生成的压缩文件名</param>
        /// <returns></returns>
        public static bool PackageFiles(string[] FilesToZip, string ZipedFile, string Password)
        {
            FileStream fs = null;
            ZipOutputStream s = null;
            ZipEntry entry = null;

            bool res = true;
            try
            {
                s = new ZipOutputStream(File.Create(ZipedFile));
                s.SetLevel(6);
                s.Password = Password;

                foreach (string FileToZip in FilesToZip)
                {
                    //如果文件没有找到，则报错
                    if (!File.Exists(FileToZip))
                    {
                        throw new System.IO.FileNotFoundException("指定要压缩的文件: " + FileToZip + " 不存在!");
                    }

                    //打开待压缩文件
                    fs = File.OpenRead(FileToZip);

                    byte[] buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, buffer.Length);
                    entry = new ZipEntry(Path.GetFileName(FileToZip));

                    entry.DateTime = File.GetLastWriteTime(FileToZip);
                    entry.Size = fs.Length;
                    fs.Close();

                    s.PutNextEntry(entry);

                    s.Write(buffer, 0, buffer.Length);
                }

                s.Finish();
                s.Close();
            }
            catch
            {
                res = false;
            }
            finally
            {
                if (entry != null)
                {
                    entry = null;
                }
                if (s != null)
                {
                    s.Finish();
                    s.Close();
                }
                if (fs != null)
                {
                    fs.Close();
                    fs = null;
                }
                GC.Collect();
                GC.Collect(1);
            }

            return res;
        }

        /// <summary>
        /// 压缩文件 和 文件夹
        /// </summary>
        /// <param name="FileToZip">待压缩的文件或文件夹，全路径格式</param>
        /// <param name="ZipedFile">压缩后生成的压缩文件名，全路径格式</param>
        /// <returns></returns>
        public static bool Zip(String FileToZip, String ZipedFile, String Password)
        {
            if (Directory.Exists(FileToZip))
            {
                return ZipFileDirectory(FileToZip, null, ZipedFile, Password);
            }
            else if (File.Exists(FileToZip))
            {
                return ZipFile(FileToZip, ZipedFile, Password);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 解压功能(解压压缩文件到指定目录)
        /// </summary>
        /// <param name="FileToUpZip">待解压的文件</param>
        /// <param name="ZipedFolder">指定解压目标目录</param>
        public static void UnZip(string FileToUpZip, string ZipedFolder, string Password)
        {
            if (!File.Exists(FileToUpZip))
            {
                return;
            }

            if (!Directory.Exists(ZipedFolder))
            {
                Directory.CreateDirectory(ZipedFolder);
            }
            //防止当解压缩后的文件名为"\"或者"/"的时候，当去掉第一个字符后，则对应字符串为""，
            //此时在执行Path.Combine操作，则结果会将文件夹当成文件来处理，从而出现错误的现象
            if (!ZipedFolder.EndsWith("\\"))
                ZipedFolder += "\\";

            ZipInputStream s = null;
            ZipEntry theEntry = null;

            string fileName;
            FileStream streamWriter = null;

            try
            {
                s = new ZipInputStream(File.OpenRead(FileToUpZip));

                if ("" != Password)
                    s.Password = Password;

                while ((theEntry = s.GetNextEntry()) != null)
                {
                    if (theEntry.Name != String.Empty)
                    {
                        fileName = theEntry.Name;
                        //如果theEntry的Name是以'\'或者'/'开头，则在进行Path.Combine所获取到的
                        //字符串跟实际的不符，所以需要在此对字符窜进行修正
                        if (fileName.StartsWith("/") || fileName.StartsWith("\\"))
                            fileName = fileName.Substring(1);
                        fileName = Path.Combine(ZipedFolder, fileName);

                        ///判断文件路径是否是文件夹
                        if (fileName.EndsWith("/") || fileName.EndsWith("\\"))
                        {
                            if (!Directory.Exists(fileName))
                                Directory.CreateDirectory(fileName);
                            continue;
                        }

                        streamWriter = File.Create(fileName);
                        int size = 2048;
                        byte[] data = new byte[2048];
                        while (true)
                        {
                            size = s.Read(data, 0, data.Length);
                            if (size > 0)
                            {
                                streamWriter.Write(data, 0, size);
                            }
                            else
                            {
                                break;
                            }
                        }

                        streamWriter.Close();

                        File.SetLastWriteTime(fileName, theEntry.DateTime);
                    }
                }
            }
            finally
            {
                if (streamWriter != null)
                {
                    streamWriter.Close();
                    streamWriter = null;
                }
                if (theEntry != null)
                {
                    theEntry = null;
                }
                if (s != null)
                {
                    s.Close();
                    s = null;
                }
                GC.Collect();
                GC.Collect(1);
            }
        }

        #region Private Zip Methods
        /// <summary>
        /// 递归压缩文件夹方法
        /// </summary>
        /// <param name="FolderToZip"></param>
        /// <param name="s"></param>
        /// <param name="ParentFolderName"></param>
        private static bool ZipFileDirectory(string FolderToZip, Dictionary<string, bool> filters, ZipOutputStream s, string ParentFolderName)
        {
            bool res = true;
            string[] folders, filenames;
            ZipEntry entry = null;
            FileStream fs = null;

            try
            {
                //如果FolderToZip是以'\'结尾，这在进行Path.GetFileName的时候会获取不到当前文件夹名称
                if (FolderToZip.EndsWith("\\"))
                {
                    FolderToZip = FolderToZip.Substring(0, FolderToZip.Length - 1);
                }
                //创建当前文件夹
                //加上 “/” 才会当成是文件夹创建
                entry = new ZipEntry(Path.Combine(ParentFolderName, Path.GetFileName(FolderToZip) + "\\"));
                entry.DateTime = Directory.GetCreationTime(FolderToZip);
                entry.Size = 0;

                s.PutNextEntry(entry);
                s.Flush();

                //先压缩文件，再递归压缩文件夹 
                filenames = Directory.GetFiles(FolderToZip);
                foreach (string file in filenames)
                {
                    if (filters != null)
                    {
                        //如果指定有过滤器，则只打包过滤器指定的文件类型
                        if (!filters.ContainsKey(Path.GetExtension(file).ToUpper()))
                            continue;
                    }

                    //打开待压缩文件
                    fs = File.OpenRead(file);

                    byte[] buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, buffer.Length);
                    entry = new ZipEntry(Path.Combine(ParentFolderName, Path.GetFileName(FolderToZip)
                        + "\\" + Path.GetFileName(file)));

                    entry.DateTime = File.GetLastWriteTime(file);
                    entry.Size = fs.Length;
                    fs.Close();

                    s.PutNextEntry(entry);

                    s.Write(buffer, 0, buffer.Length);
                }
            }
            catch
            {
                res = false;
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                    fs = null;
                }
                if (entry != null)
                {
                    entry = null;
                }
                GC.Collect();
                GC.Collect(1);
            }

            folders = Directory.GetDirectories(FolderToZip);
            foreach (string folder in folders)
            {
                if (!ZipFileDirectory(folder, filters, s, Path.Combine(ParentFolderName, Path.GetFileName(FolderToZip))))
                {
                    return false;
                }
            }

            return res;
        }

        /// <summary>
        /// 压缩目录
        /// </summary>
        /// <param name="FolderToZip">待压缩的文件夹，全路径格式</param>
        /// <param name="ZipedFile">压缩后的文件名，全路径格式</param>
        /// <returns></returns>
        private static bool ZipFileDirectory(string FolderToZip, Dictionary<string, bool> filters, string ZipedFile, string Password)
        {
            bool res;
            if (!Directory.Exists(FolderToZip))
            {
                return false;
            }

            ZipOutputStream s = new ZipOutputStream(File.Create(ZipedFile));
            s.SetLevel(6);

            s.Password = Password;

            res = ZipFileDirectory(FolderToZip, filters, s, "");

            s.Finish();
            s.Close();

            return res;
        }

        /// <summary>
        /// 压缩文件
        /// </summary>
        /// <param name="FileToZip">要进行压缩的文件名</param>
        /// <param name="ZipedFile">压缩后生成的压缩文件名</param>
        /// <returns></returns>
        private static bool ZipFile(string FileToZip, string ZipedFile, string Password)
        {
            //如果文件没有找到，则报错
            if (!File.Exists(FileToZip))
            {
                //throw new System.IO.FileNotFoundException("指定要压缩的文件: " + FileToZip + " 不存在!");
                throw new System.IO.FileNotFoundException("The file to be zipped (" + FileToZip + ") doesn't exist!");
            }

            FileStream fs = null;
            ZipOutputStream s = null;
            ZipEntry entry = null;

            bool res = true;
            try
            {
                fs = File.OpenRead(FileToZip);
                byte[] buffer = new byte[fs.Length];
                fs.Read(buffer, 0, buffer.Length);

                fs = File.Create(ZipedFile);
                s = new ZipOutputStream(fs);

                s.Password = Password;

                entry = new ZipEntry(Path.GetFileName(FileToZip));

                entry.DateTime = File.GetLastWriteTime(FileToZip);
                entry.Size = fs.Length;

                fs.Close();

                s.PutNextEntry(entry);
                s.SetLevel(6);

                s.Write(buffer, 0, buffer.Length);
            }
            catch
            {
                res = false;
            }
            finally
            {
                if (entry != null)
                {
                    entry = null;
                }
                if (s != null)
                {
                    s.Finish();
                    s.Close();
                }
                if (fs != null)
                {
                    fs.Close();
                    fs = null;
                }
                GC.Collect();
                GC.Collect(1);
            }

            return res;
        }

        /// <summary>
        /// get the root directory name from the zip file
        /// </summary>
        /// <param name="zipFile">the zip file</param>
        /// <returns>if the first entry is directory, return the directory name, else return string.Empty</returns>
        public static string GetTheRootDirName(string zipFile)
        {
            //if the ZIP file isn't exist, throw exception
            if (!File.Exists(zipFile))
            {
                throw new System.IO.FileNotFoundException("The file to be zipped (" + zipFile + ") doesn't exist!");
            }

            string result = string.Empty;
            try
            {
                using (ZipInputStream s = new ZipInputStream(File.OpenRead(zipFile)))
                {
                    ZipEntry theEntry = s.GetNextEntry();
                    result = theEntry.Name;
                    if (result.EndsWith("\\") || result.EndsWith("/"))
                    {
                        result = result.Substring(0, result.Length - 1);
                    }
                }
            }
            catch
            {
            }
            return result;
        }
        #endregion
    }
}

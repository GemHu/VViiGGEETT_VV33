/// <summary>
/// @file   SuperFileName.cs
///	@brief  强劲文件名称，能同时记住相对路径和绝对路径。
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
using System.ComponentModel;

namespace Dothan.Helpers
{
    /// <summary>
    /// 强劲文件名称，能同时记住相对路径和绝对路径。
    /// </summary>
    public class SuperFileName : IComparable<SuperFileName>, IComparable
    {
        /// <summary>
        /// 文件名称绝对路径。
        /// </summary>
        public string AbsoluteName { get; set; }

        /// <summary>
        /// 文件名称相对路径。
        /// </summary>
        public string RelativeName { get; set; }

        #region 构造函数

        public SuperFileName()
        {
            this.AbsoluteName = string.Empty;
            this.RelativeName = string.Empty;
        }

        public SuperFileName(string absoluteName)
        {
            this.AbsoluteName = absoluteName;
            this.RelativeName = string.Empty;
        }

        public SuperFileName(string baseFile, string absoluteName)
            : this()
        {
            if (!string.IsNullOrEmpty(absoluteName))
            {
                this.AbsoluteName = absoluteName;
                if (!string.IsNullOrEmpty(baseFile))
                {
                    this.RelativeName = FileName.GetRelativeFileName
                        (FileName.GetFilePath(baseFile), absoluteName);
                }
            }
        }

        public SuperFileName(SuperFileName clone)
        {
            this.AbsoluteName = clone.AbsoluteName;
            this.RelativeName = clone.RelativeName;
        }

        #endregion

        /// <summary>
        /// 根据指定的相对文件，得到真正的文件名称。
        /// </summary>
        /// <param name="baseFile">相对文件的名称</param>
        /// <returns>真正的文件名称</returns>
        public string GetActualName(string baseFile)
        {
            if (!string.IsNullOrEmpty(baseFile))
            {
                string fileName = FileName.GetAbsoluteFileName
                    (FileName.GetFilePath(baseFile), this.RelativeName);
                if (File.Exists(fileName))
                    return fileName;
            }
            return this.AbsoluteName ?? string.Empty;
        }

        /// <summary>
        /// 得到文件的显示名称。
        /// </summary>
        public string GetShownName()
        {
            if (!string.IsNullOrEmpty(this.RelativeName))
            {
                // 如果第一个是目录分隔符、第二个不是，则从显示美观的角度，
                // 去掉这第一个目录分隔符。
                if (this.RelativeName.Length >= 2 &&
                    this.RelativeName[0] == '\\' &&
                    this.RelativeName[1] != '\\')
                    return this.RelativeName.Substring(1);

                return this.RelativeName;
            }
            return this.AbsoluteName ?? string.Empty;
        }

        /// <summary>
        /// 得到文件的显示名称。
        /// </summary>
        static public string GetShownName(string baseFile, string absoluteName)
        {
            return (new SuperFileName(baseFile, absoluteName)).GetShownName();
        }

        /// <summary>
        /// 判断两 SuperFileName 是否相等？
        /// </summary>
        public bool Equals(SuperFileName compareTo)
        {
            return string.Equals(this.AbsoluteName, compareTo.AbsoluteName) &&
                   string.Equals(this.RelativeName, compareTo.RelativeName);
        }

        /// <summary>
        /// 判断 SuperFileName 的绝对路径是否等于指定文件名称？
        /// </summary>
        public bool Equals(string fileName)
        {
            return string.Equals(this.AbsoluteName, fileName);
        }

        /// <summary>
        /// 空文件名称。
        /// </summary>
        public static SuperFileName Empty = new SuperFileName();

        /// <summary>
        /// 判断对象是否为空文件名称？
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsEmpty
        {
            get
            {
                return string.IsNullOrEmpty(this.AbsoluteName);
            }
        }

        public int CompareTo(SuperFileName other)
        {
            if (other == null)
                return 1;

            int ret = string.Compare(this.AbsoluteName, other.AbsoluteName, true);
            if (ret != 0) return ret;
            return string.Compare(this.RelativeName, other.RelativeName, true);
        }

        public int CompareTo(object other)
        {
            return this.CompareTo(other as SuperFileName);
        }
    }
}

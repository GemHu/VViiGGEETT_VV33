using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Dothan.ViObject
{
    /// <summary>
    /// 文件类型。
    /// </summary>
    [Flags]
    public enum FileType
    {
        None = 0x0000,                          ///< Invalid value

        PROGRAM = 0x0010,
        FUNCTION_BLOCK = 0x0020,
        FUNCTION = 0x0040,
        XXType = 0x00F0,                        ///< Program/FB... type's mask

        CFCFile = 0x0100,
        FBDFile = 0x0200,
        ILFile = 0x0400,
        LDFile = 0x0800,
        SFCFile = 0x1000,
        STFile = 0x2000,
        OldCFCFile = 0x4000,
        XXFile = 0xFF00,                       ///< CFC/IL... type's mask

        CFCProgram = (CFCFile | PROGRAM),
        CFCFuncBlock = (CFCFile | FUNCTION_BLOCK),
        OldCFCProgram = (OldCFCFile | PROGRAM),
        OldCFCFuncBlock = (OldCFCFile | FUNCTION_BLOCK),
        //
        FBDProgram = (FBDFile | PROGRAM),
        FBDFuncBlock = (FBDFile | FUNCTION_BLOCK),
        FBDFunction = (FBDFile | FUNCTION),
        //
        ILProgram = (ILFile | PROGRAM),
        ILFuncBlock = (ILFile | FUNCTION_BLOCK),
        ILFunction = (ILFile | FUNCTION),
        //
        LDProgram = (LDFile | PROGRAM),
        LDFuncBlock = (LDFile | FUNCTION_BLOCK),
        //
        SFCProgram = (SFCFile | PROGRAM),
        //
        STProgram = (STFile | PROGRAM),
        STFuncBlock = (STFile | FUNCTION_BLOCK),
        STFunction = (STFile | FUNCTION),

        HWCFile = 0x0001,                ///< Hardware Config file
        MakeFile,                               ///< CPU makefile
        Library,                                ///< Library file
        Variable,                               ///< Variable file
        NWCFile,                              ///< NetConfig file
        Other                                  ///< Other file
    }

    public static class Extension
    {
        public const String SolutionFile = ".vgsln";
        public const String ProjectFile = ".vgstation";

        public const String CFCProgram = ".XCFC";
        public const String CFCFuncBlock = ".XCFC";
        public const String ILProgram = ".POE";
        public const String ILFuncBlock = ".POE";
        public const String ILFunction = ".POE";
        public const String STProgram = ".ST";
        public const String STFuncBlock = ".ST";
        public const String STFunction = ".ST";
        public const String MakeFile = ".MAK";
        public const String HWCFile = ".vghwc";
        public const String NWCFile = ".vgnwc";
    }

    public static class Section
    {
        public const String CFCProgram = "CFC_PROGRAM";
        public const String CFCFuncBlock = "CFC_FUNCTIONBLOCK";
        public const String ILProgram = "PROGRAM";
        public const String ILFuncBlock = "FUNCTIONBLOCK";
        public const String ILFunction = "FUNCTION";
        public const String STProgram = "ST_PROGRAM";
        public const String STFuncBlock = "ST_FUNCTIONBLOCK";
        public const String STFunction = "ST_FUNCTION";
        public const String MakeFile = "MAKFILE";
        public const String HWCFile = "STATION";
        public const String NWCFile = "NETWORK_CONFIG";
    }

    public class PcsFileInfo
    {
        private static Dictionary<FileType, PcsFileInfo> _DPcsFileInfos;

        public FileType type;
        public string extName;
        public string varSection;

        public PcsFileInfo(FileType type, string extName, string varSection)
        {
            this.type = type;
            this.extName = extName;
            this.varSection = varSection;
        }

        /// <summary>
        /// 文件类型与文件后缀、文件Section对应的关联信息。
        /// </summary>
        public static Dictionary<FileType, PcsFileInfo> DPcsFileInfos
        {
            get
            {
                if (_DPcsFileInfos == null)
                {
                    _DPcsFileInfos = new Dictionary<FileType, PcsFileInfo>();
                    _DPcsFileInfos[FileType.CFCProgram] = new PcsFileInfo(FileType.CFCProgram, Extension.CFCProgram, Section.CFCProgram);
                    _DPcsFileInfos[FileType.CFCFuncBlock] = new PcsFileInfo(FileType.CFCFuncBlock, Extension.CFCFuncBlock, Section.CFCFuncBlock);
                    _DPcsFileInfos[FileType.ILProgram] = new PcsFileInfo(FileType.ILProgram, Extension.ILProgram, Section.ILProgram);
                    _DPcsFileInfos[FileType.ILFuncBlock] = new PcsFileInfo(FileType.ILFuncBlock, Extension.ILFuncBlock, Section.ILFuncBlock);
                    _DPcsFileInfos[FileType.ILFunction] = new PcsFileInfo(FileType.ILFunction, Extension.ILFunction, Section.ILFunction);
                    _DPcsFileInfos[FileType.STProgram] = new PcsFileInfo(FileType.STProgram, Extension.STProgram, Section.STProgram);
                    _DPcsFileInfos[FileType.STFuncBlock] = new PcsFileInfo(FileType.STFuncBlock, Extension.STFuncBlock, Section.STFuncBlock);
                    _DPcsFileInfos[FileType.STFunction] = new PcsFileInfo(FileType.STFunction, Extension.STFunction, Section.STFunction);

                    //_DPcsFileInfos[FileType.STFunction] = new PcsFileInfo(FileType.Makefile, ".MAK", "EPU10A");
                    //_DPcsFileInfos[FileType.STFunction] = new PcsFileInfo(FileType.Makefile, ".MAK", "ESP10B");
                    _DPcsFileInfos[FileType.STFunction] = new PcsFileInfo(FileType.MakeFile, Extension.MakeFile, Section.MakeFile);

                    _DPcsFileInfos[FileType.HWCFile] = new PcsFileInfo(FileType.HWCFile, Extension.HWCFile, Section.HWCFile);
                    _DPcsFileInfos[FileType.Variable] = new PcsFileInfo(FileType.Variable, ".PIV", "EXTERNALS");
                    _DPcsFileInfos[FileType.Other] = new PcsFileInfo(FileType.Other, "", "EXTERNALS");
                }

                return _DPcsFileInfos;
            }
        }

        public static PcsFileInfo Parse(String section)
        {
            return PcsFileInfo.Parse(section, PcsFileInfo.DPcsFileInfos[FileType.Other]);
        }

        public static PcsFileInfo Parse(String section, PcsFileInfo defaultValue)
        {
            if (String.IsNullOrEmpty(section))
                return defaultValue;

            foreach (PcsFileInfo item in PcsFileInfo.DPcsFileInfos.Values)
            {
                if (item.varSection.Equals(section, StringComparison.OrdinalIgnoreCase))
                    return item;
            }

            return defaultValue;
        }
    }

    /// <summary>
    /// 工程 .VAR 文件中包含的工程文件信息。
    /// </summary>
    public class PCSFile : IComparable<PCSFile>, IComparable
    {
        public PCSFile(FileType type, string file)
        {
            this.Type = type;
            this.File = file;
            this.Key = GetFileKey(file);
        }

        public FileType Type { get; protected set; }
        public string File { get; protected set; }
        public string Key { get; protected set; }

        public int CompareTo(PCSFile other)
        {
            if (other == null) return -1;
            return string.Compare(this.Key, other.Key, true);
        }

        public int CompareTo(object other)
        {
            return this.CompareTo(other as PCSFile);
        }

        /// <summary>
        /// 得到文件名称中的对象关键字。
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <returns>对象关键字</returns>
        public static string GetFileKey(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return string.Empty;
            return Path.GetFileNameWithoutExtension(fileName).ToUpper();
        }

        public static FileType GetFileType(string file, String section)
        {
            string extName = Path.GetExtension(file);
            if (string.IsNullOrEmpty(extName))
                return FileType.None;

            // 根据后缀名，获取文件类型，根据section，获取功能块类型；
            foreach (PcsFileInfo info in PcsFileInfo.DPcsFileInfos.Values)
            {
                if (string.IsNullOrEmpty(info.extName))
                    continue;
                if (string.Compare(extName, info.extName, true) != 0)
                    continue;

                // 1、根据文件后缀名，获取文件类型；
                // 2、根据section获取功能类型；
                FileType type = PCSFile.Section2FileType(section);
                //return (info.type & FileType.XXFile) | (type & FileType.XXType);
                return info.type;
            }
            return FileType.None;
        }

        /// <summary>
        /// 将枚举类型的文件类型转换为字符串，以便进行文件的保存操作；
        /// </summary>
        /// <param name="fileType"></param>
        /// <returns></returns>
        public static String FileType2Section(FileType fileType)
        {
            string section = null;
            switch (fileType & FileType.XXFile)
            {
                case FileType.CFCFile:
                    section = "CFC";
                    break;
                case FileType.FBDFile:
                    section = "FBD";
                    break;
                case FileType.ILFile:
                    section = "";
                    break;
                case FileType.LDFile:
                    section = "LD";
                    break;
                case FileType.SFCFile:
                    section = "SFC";
                    break;
                case FileType.STFile:
                    section = "ST";
                    break;
                default:
                    return String.Empty;
            }

            if (!string.IsNullOrEmpty(section))
                section += '_';

            switch (fileType & FileType.XXType)
            {
                case FileType.PROGRAM:
                    section += "PROGRAM";
                    break;
                case FileType.FUNCTION_BLOCK:
                    section += "FUNCTIONBLOCK";
                    break;
                case FileType.FUNCTION:
                    section += "FUNCTION";
                    break;
                default:
                    return String.Empty;
            }

            return section;
        }

        /// <summary>
        /// 将从文件中读取到的字符串形式的文件类型转换为枚举值；
        /// </summary>
        /// <param name="section"></param>
        /// <returns></returns>
        public static FileType Section2FileType(String section)
        {
            if (String.IsNullOrEmpty(section))
                return FileType.None;

            FileType fileType = FileType.None;
            if (section.Equals(Section.MakeFile, StringComparison.OrdinalIgnoreCase))
            {
                fileType = FileType.MakeFile;
            }
            else if (section.Equals(Section.HWCFile, StringComparison.OrdinalIgnoreCase))
            {
                fileType = FileType.HWCFile;
            }
            else if (section.Equals("LIBRARY", StringComparison.OrdinalIgnoreCase))
            {
                fileType = FileType.Library;
            }
            else
            {
                // XXType
                if (section.EndsWith("PROGRAM", StringComparison.OrdinalIgnoreCase))
                {
                    fileType = FileType.PROGRAM;
                    section = section.Substring(0, section.Length - 7);
                }
                else if (section.EndsWith("FUNCTIONBLOCK", StringComparison.OrdinalIgnoreCase))
                {
                    fileType = FileType.FUNCTION_BLOCK;
                    section = section.Substring(0, section.Length - 13);
                }
                else if (section.EndsWith("FUNCTION", StringComparison.OrdinalIgnoreCase))
                {
                    fileType = FileType.FUNCTION;
                    section = section.Substring(0, section.Length - 8);
                }
                else
                {
                    return FileType.None;
                }

                // XXFile
                if (section.EndsWith("_"))
                    section = section.Substring(0, section.Length - 1);
                //
                if (section == "" /*|| section.Equals("IL", StringComparison.OrdinalIgnoreCase)*/)
                {
                    fileType |= FileType.ILFile;
                }
                else if (section.Equals("CFC", StringComparison.OrdinalIgnoreCase))
                {
                    fileType |= FileType.CFCFile;
                }
                else if (section.Equals("FBD", StringComparison.OrdinalIgnoreCase))
                {
                    fileType |= FileType.FBDFile;
                }
                else if (section.Equals("LD", StringComparison.OrdinalIgnoreCase))
                {
                    fileType |= FileType.LDFile;
                }
                else if (section.Equals("SFC", StringComparison.OrdinalIgnoreCase))
                {
                    fileType |= FileType.SFCFile;
                }
                else if (section.Equals("ST", StringComparison.OrdinalIgnoreCase))
                {
                    fileType |= FileType.STFile;
                }
            }

            return fileType;
        }

    }

}

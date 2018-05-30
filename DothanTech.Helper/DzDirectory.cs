using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Microsoft.Win32;

namespace Dothan.Helpers
{
    public sealed class DZDirectory
    {
        public static bool Create(string path)
        {
            try
            {
                return (Directory.CreateDirectory(path) != null);
            }
            catch (Exception e)
            {
                Trace.WriteLine("### [" + e.Source + "] Exception: " + e.Message);
                Trace.WriteLine("### " + e.StackTrace);
                return false;
            }
        }

        public static bool Delete(string path)
        {
            if (!Directory.Exists(path))
                return true;

            try
            {
                Directory.Delete(path, true);
                return true;
            }
            catch (Exception e)
            {
                Trace.WriteLine("### [" + e.Source + "] Exception: " + e.Message);
                Trace.WriteLine("### " + e.StackTrace);
                return false;
            }
        }

        public static bool Rename(string oldPath, string newPath)
        {
            if (!Directory.Exists(oldPath))
                return true;
            if (oldPath.Equals(newPath, StringComparison.OrdinalIgnoreCase))
                return true;

            try
            {
                Directory.Move(oldPath, newPath);
                return true;
            }
            catch (Exception e)
            {
                Trace.WriteLine("### [" + e.Source + "] Exception: " + e.Message);
                Trace.WriteLine("### " + e.StackTrace);
                return false;
            }
        }

        public static bool IsEmpty(string path)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(path);
                return (di.GetDirectories().Length <= 0 && di.GetFiles().Length <= 0);
            }
            catch (Exception e)
            {
                Trace.WriteLine("### [" + e.Source + "] Exception: " + e.Message);
                Trace.WriteLine("### " + e.StackTrace);
                return false;
            }
        }

        #region Delete file to RecycleBin
        public static bool DeleteToRecycleBin(string path)
        {
            return FileName.DeleteToRecycleBin(path);
        }
        #endregion

        public static bool DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            try
            {
                // Get the subdirectories for the specified directory.
                DirectoryInfo dir = new DirectoryInfo(sourceDirName);
                DirectoryInfo[] dirs = dir.GetDirectories();

                if (!dir.Exists)
                {
                    throw new DirectoryNotFoundException(
                        "Source directory does not exist or could not be found: "
                        + sourceDirName);
                }

                // If the destination directory doesn't exist, create it. 
                if (!Directory.Exists(destDirName))
                {
                    Directory.CreateDirectory(destDirName);
                }

                // Get the files in the directory and copy them to the new location.
                FileInfo[] files = dir.GetFiles();
                foreach (FileInfo file in files)
                {
                    string temppath = Path.Combine(destDirName, file.Name);
                    file.CopyTo(temppath, false);
                }

                // If copying subdirectories, copy them and their contents to new location. 
                if (copySubDirs)
                {
                    foreach (DirectoryInfo subdir in dirs)
                    {
                        string temppath = Path.Combine(destDirName, subdir.Name);
                        DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                Trace.WriteLine("### [" + e.Source + "] Exception: " + e.Message);
                Trace.WriteLine("### " + e.StackTrace);
                return false;
            }
        }

        public static bool DirectoryDelete(string sourceDirName)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(sourceDirName);

                di.Delete(true);

                return true;
            }
            catch (Exception e)
            {
                Trace.WriteLine("### [" + e.Source + "] Exception: " + e.Message);
                Trace.WriteLine("### " + e.StackTrace);
                return false;
            }
        }

        /// <summary>
        /// search file from the directory which has the given extensions
        /// </summary>
        /// <param name="directory">the directory which search from</param>
        /// <param name="ext">file extension for search</param>
        /// <returns>if the file is not exits,and then return null;else return the file path</returns>
        public static string SearchFile(string directory, string ext)
        {
            if (!Directory.Exists(directory))
                return string.Empty;

            string[] files = Directory.GetFiles(directory);
            foreach (string file in files)
            {
                if (file.EndsWith(ext))
                {
                    return file;
                }
            }
            //Traversing files
            string[] dirs = Directory.GetDirectories(directory);
            foreach (string dir in dirs)
            {
                string result = SearchFile(dir, ext);
                if (!string.IsNullOrEmpty(result))
                {
                    return result;
                }
            }
            return string.Empty;
        }
    }
}

/// <summary>
/// @file   RegistryKeyExtension.cs
///	@brief  RegistryKey 类的扩展函数。
/// @author	DothanTech 刘伟宏
/// 
/// Copyright(C) 2011~2014, DothanTech. All rights reserved.
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace Dothan.Helpers
{
    /// <summary>
    /// RegistryKey 类的扩展函数。
    /// </summary>
    public static class RegistryKeyExtension
    {
        /// <summary>
        /// 打开注册表键值。键值名称中可以以 HKEY_CLASSES_ROOT 等根节点名称开始，
        /// 如果没有这些标明根节点的信息，则默认根节点为 HKEY_CURRENT_USER。
        /// </summary>
        /// <param name="name">键值路径</param>
        /// <param name="writable">是否已可写方式打开键值？</param>
        /// <returns>打开的注册表键值对象。</returns>
        public static RegistryKey OpenSubKey(string name, bool writable)
        {
            RegistryKey theRoot = Registry.CurrentUser;

            if (!string.IsNullOrEmpty(name))
            {
                int intIndex = name.IndexOf("\\");

                if (intIndex >= 0)
                {
                    string strRoot = name.Substring(0, intIndex).ToUpper();
                    string newName = name.Substring(intIndex + 1);
                    switch (strRoot)
                    {
                        case "HKEY_CLASSES_ROOT":
                            theRoot = Registry.ClassesRoot;
                            break;
                        case "HKEY_CURRENT_CONFIG":
                            theRoot = Registry.CurrentConfig;
                            break;
                        case "HKEY_CURRENT_USER":
                            theRoot = Registry.CurrentUser;
                            break;
                        //case "HKEY_DYN_DATA":
                        //    theRoot = Registry.DynData;
                        //    break;
                        case "HKEY_LOCAL_MACHINE":
                            theRoot = Registry.LocalMachine;
                            break;
                        case "HKEY_PERFORMANCE_DATA":
                            theRoot = Registry.PerformanceData;
                            break;
                        case "HKEY_USERS":
                            theRoot = Registry.Users;
                            break;
                        default:
                            theRoot = Registry.CurrentUser;
                            newName = name;
                            break;
                    }

                    name = newName;
                }
            }

            return OpenSubKey(theRoot, name, writable);
        }

        /// <summary>
        /// 打开注册表键值。
        /// </summary>
        /// <param name="root">注册表根节点，可以为 Registry.ClassesRoot 等全局对象。</param>
        /// <param name="name">键值路径。</param>
        /// <param name="writable">是否已可写方式打开键值？</param>
        /// <returns></returns>
        public static RegistryKey OpenSubKey(RegistryKey root, string name, bool writable)
        {
            RegistryKey key = root.OpenSubKey(name, writable);
            if (key == null && writable)
                key = root.CreateSubKey(name);
            return key;
        }

        /// <summary>
        /// 从注册表中读取 Int 型数值。
        /// </summary>
        /// <param name="name">键值名称。</param>
        /// <returns>Int 型数值。</returns>
        public static int GetInt(this RegistryKey This, string name)
        {
            return This.GetInt(name, 0);
        }

        /// <summary>
        /// 从注册表中读取 Int 型数值。
        /// </summary>
        /// <param name="name">键值名称。</param>
        /// <param name="defaultValue">缺省值。</param>
        /// <returns>Int 型数值。</returns>
        public static int GetInt(this RegistryKey This, string name, int defaultValue)
        {
            if (This == null) return defaultValue;

            try
            {
                return (int)This.GetValue(name, defaultValue);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// 设置注册表键值为 Int 型数值。
        /// </summary>
        /// <param name="name">键值名称。</param>
        /// <param name="value">Int 型数值。</param>
        /// <returns>成功与否？</returns>
        public static bool SetInt(this RegistryKey This, string name, int value)
        {
            try
            {
                This.SetValue(name, value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 从注册表中读取字符串类型值。
        /// </summary>
        /// <param name="name">键值名称。</param>
        /// <returns>字符串类型值。</returns>
        public static string GetString(this RegistryKey This, string name)
        {
            return This.GetString(name, string.Empty);
        }

        /// <summary>
        /// 从注册表中读取字符串类型值。
        /// </summary>
        /// <param name="name">键值名称。</param>
        /// <param name="defaultValue">缺省值。</param>
        /// <returns>字符串类型值。</returns>
        public static string GetString(this RegistryKey This, string name, string defaultValue)
        {
            if (This == null) return defaultValue;

            try
            {
                return (string)This.GetValue(name, defaultValue);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// 设置注册表键值为字符串类型值。
        /// </summary>
        /// <param name="name">键值名称。</param>
        /// <param name="value">字符串类型值。</param>
        /// <returns>成功与否？</returns>
        public static bool SetString(this RegistryKey This, string name, string value)
        {
            try
            {
                This.SetValue(name, value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 从注册表中读取 bool 型数值。
        /// </summary>
        /// <param name="name">键值名称。</param>
        /// <returns>bool 型数值。</returns>
        public static bool GetBool(this RegistryKey This, string name)
        {
            return This.GetBool(name, false);
        }

        /// <summary>
        /// 从注册表中读取 bool 型数值。
        /// </summary>
        /// <param name="name">键值名称。</param>
        /// <param name="defaultValue">缺省值。</param>
        /// <returns>bool 型数值。</returns>
        public static bool GetBool(this RegistryKey This, string name, bool defaultValue)
        {
            if (This == null) return defaultValue;

            string strValue = This.GetString(name, string.Empty);
            if (string.IsNullOrEmpty(strValue))
                return defaultValue;

            bool result;
            if (bool.TryParse(strValue, out result))
                return result;

            switch (strValue[0])
            {
                case '-':
                case '0':
                case 'N':
                case 'n':
                case 'F':
                case 'f':
                default: return false;
                case '+':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                case 'Y':
                case 'y':
                case 'T':
                case 't': return true;
                case 'o':
                case 'O':
                    if (strValue.Length == 1)
                        return false;
                    return strValue[1] == 'n'
                        || strValue[1] == 'N';
            }
        }

        /// <summary>
        /// 设置注册表键值为 bool 型数值。
        /// </summary>
        /// <param name="name">键值名称。</param>
        /// <param name="value">bool 型数值。</param>
        /// <returns>成功与否？</returns>
        public static bool SetBool(this RegistryKey This, string name, bool value)
        {
            try
            {
                This.SetValue(name, value.ToString());
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 从注册表中读取指定枚举类型值。
        /// </summary>
        /// <param name="name">键值名称。</param>
        /// <param name="defaultValue">缺省值。</param>
        /// <returns>指定枚举类型值。</returns>
        public static EnumType GetEnum<EnumType>(this RegistryKey This, string name, EnumType defaultValue)
        {
            if (This == null) return defaultValue;

            try
            {
                string strValue = This.GetString(name, defaultValue.ToString());
                return (EnumType)Enum.Parse(typeof(EnumType), strValue, true);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// 设置注册表键值为指定枚举类型值。
        /// </summary>
        /// <param name="name">键值名称。</param>
        /// <param name="value">枚举类型值。</param>
        /// <returns>成功与否？</returns>
        public static bool SetEnum<EnumType>(this RegistryKey This, string name, EnumType value)
        {
            return This.SetString(name, value.ToString());
        }
    }
}

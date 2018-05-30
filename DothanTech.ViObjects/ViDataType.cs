/// <summary>
/// @file   ViDataType.cs
///	@brief  ViGET 编辑器中的基础数据类型。其列表在 OpenPCS.520\DisplayStringMapping.xml 中定义。
/// @author	DothanTech 刘伟宏
/// 
/// Copyright(C) 2011~2014, DothanTech. All rights reserved.
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Collections;
using System.Diagnostics;
using System.Windows;
using System.Collections.ObjectModel;

using Dothan.Helpers;

namespace Dothan.ViObject
{
    /// <summary>
    /// 创建数据类型的方式。
    /// </summary>
    public enum ViTypeCreation
    {
        None,                       ///< 不自动创建数据类型
        Create,                     ///< 自动创建数据类型，但是不添加到全局集合中
        CreateGlobal,               ///< 自动创建数据类型，同时添加到全局集合中
    }

    /// <summary>
    /// ViGET 编辑器中的基础数据类型。其列表在 OpenPCS.520\DisplayStringMapping.xml 中定义。
    /// </summary>
    public class ViDataType : ViType
    {
        #region Static Init

        static ViDataType()
        {
            InitCache();
        }

        /// <summary>
        /// 初始化全局缓存。
        /// </summary>
        public static void InitCache()
        {
            ViDataType.allDataTypes = null;
            ViDataType.mapAllDTypes = null;
        }

        #endregion

        #region Life cycle

        /// <summary>
        /// 构建对象。
        /// </summary>
        /// <param name="name">类型名称</param>
        /// <param name="shortName">类型短名称</param>
        public ViDataType(string name, string shortName)
        {
            this.Name = name;
            this.ShortName = shortName;
        }

        #endregion

        /// <summary>
        /// 显示用数据类型短名称。一般在图形化显示功能块的管脚类型时使用。
        /// </summary>
        public string ShortName { get; protected set; }

        /// <summary>
        /// 是否是 IOAddress 类型？
        /// </summary>
        public bool IsIOAddress { get; protected set; }

        #region D Auto Created

        public static readonly DependencyProperty AutoCreatedProperty =
            DependencyProperty.Register("AutoCreated", typeof(bool), typeof(ViDataType),
                                        new FrameworkPropertyMetadata(false));

        /// <summary>
        /// 数据类型对象是否是自动创建的？
        /// </summary>
        public bool AutoCreated
        {
            get
            {
                return (bool)GetValue(AutoCreatedProperty);
            }
            set
            {
                if (value)
                    SetValue(AutoCreatedProperty, value);
                else
                    ClearValue(AutoCreatedProperty);
            }
        }

        #endregion

        #region DataType collection

        /// <summary>
        /// 得到所有的数据类型集合。
        /// </summary>
        public static ViObservableCollection<ViDataType> AllDataTypes
        {
            get
            {
                if (allDataTypes == null)
                {
                    allDataTypes = new ViObservableCollection<ViDataType>();
                    mapAllDTypes = new Dictionary<string, ViDataType>();

                    // 从配置文件中读取
                    XmlDocument doc = new XmlDocument();
                    try
                    {
                        doc.Load(ViGlobal.ProjODKPath + "DisplayStringMapping.xml");
                        XmlNodeList nodeList = doc.SelectSingleNode("Types").ChildNodes;
                        foreach (XmlElement node in nodeList)
                        {
                            string strOriginal = node.GetAttribute("Original");
                            string strDisplayString = node.GetAttribute("DisplayString");
                            bool bIOAddressType = node.GetAttributeBool("IOAddressType");
                            if (string.IsNullOrEmpty(strOriginal) || string.IsNullOrEmpty(strDisplayString))
                            {
                                Trace.WriteLine(string.Format("Invalid DisplayStringMapping: {0}, {1}", strOriginal, strDisplayString));
                            }
                            else
                            {
                                string key = strOriginal.ToUpper();
                                if (mapAllDTypes.ContainsKey(key))
                                {
                                    Trace.WriteLine(string.Format("Duplicated Mapping Item: {0}, {1}", strOriginal, strDisplayString));
                                }
                                else
                                {
                                    ViDataType dataType = new ViDataType(strOriginal, strDisplayString);
                                    dataType.IsIOAddress = bIOAddressType;
                                    allDataTypes.Add(dataType);
                                    mapAllDTypes[key] = dataType;
                                }
                            }
                        }
                    }
                    catch (Exception ee)
                    {
                        Trace.WriteLine("### [" + ee.Source + "] Exception: " + ee.Message);
                        Trace.WriteLine("### " + ee.StackTrace);
                    }
                }

                return allDataTypes;
            }
        }
        protected static ViObservableCollection<ViDataType> allDataTypes;
        protected static Dictionary<string, ViDataType> mapAllDTypes;

        /// <summary>
        /// 得到指定名称的数据类型对象。如果该名称的数据类型不存在，则不会自动创建。
        /// </summary>
        /// <param name="name">类型名称</param>
        /// <returns>数据类型对象</returns>
        public static ViDataType GetDataType(string name)
        {
            return GetDataType(name, ViTypeCreation.None);
        }

        /// <summary>
        /// 得到指定名称的数据类型对象。如果该名称的数据类型不存在，则会按照指定的创建方式进行创建。
        /// </summary>
        /// <param name="name">类型名称</param>
        /// <param name="creation">自动创建数据类型的方式</param>
        /// <returns>数据类型对象</returns>
        public static ViDataType GetDataType(string name, ViTypeCreation creation)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            // 确保数据被加载了
            ViObservableCollection<ViDataType> allDataTypes = AllDataTypes;

            // 便于使用，去掉括号后面的注释信息
            int pos = name.IndexOf('(');
            if (pos > 0)
                name = name.Substring(0, pos);

            // 关键字都是大写
            string key = name.ToUpper().Trim();

            // 从字典中取得指定名称的对象
            if (mapAllDTypes.ContainsKey(key))
                return mapAllDTypes[key];

            // 自动创建？
            if (creation != ViTypeCreation.None &&
                ViIECStandard.IsValidName(name))
            {
                ViDataType dataType = new ViDataType(name, string.Empty);
                dataType.AutoCreated = true;
                if (creation == ViTypeCreation.CreateGlobal)
                {
                    allDataTypes.Add(dataType);
                    mapAllDTypes[key] = dataType;
                }
                //
                return dataType;
            }

            // 没有找到
            return null;
        }

        #endregion
    }
}

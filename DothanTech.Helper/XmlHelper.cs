/// <summary>
/// @file   XmlHelper.cs
///	@brief  提供 Xml 序列化操作的辅助函数。
/// @author	DothanTech 胡殿兴
/// 
/// Copyright(C) 2011~2014, DothanTech. All rights reserved.
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Diagnostics;

namespace Dothan.Helpers
{
	/// <summary>
	/// 提供 Xml 序列化操作的辅助函数。
	/// </summary>
    public class XmlHelper
    {
		/// <summary>
		/// Provide the case insensitive property obtain method,return empty string when not existed.
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="name"></param>
		/// <returns></returns>
        static public string GetAttribute(XmlReader reader, string name)
        {
            try
            {
                if (reader.HasAttributes)
                {
                    for (int i = 0; i < reader.AttributeCount; i++)
                    {
                        if (i == 0)
                            reader.MoveToFirstAttribute();
                        else
                            reader.MoveToNextAttribute();

                        // case insensitive property name compare
                        if (string.Compare(name, reader.Name, true) == 0)
                            return reader.Value == null ? "" : reader.Value.Trim();
                    }
                }
            }
            catch (Exception ee)
            {
                Trace.WriteLine("### [" + ee.Source + "] Exception: " + ee.Message);
                Trace.WriteLine("### " + ee.StackTrace);
            }
            return "";
        }
    }
}

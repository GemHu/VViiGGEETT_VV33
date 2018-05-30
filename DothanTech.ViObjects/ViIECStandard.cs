/// <summary>
/// @file   ViIECStandard.cs
///	@brief  ViGET IEC 标准相关的辅助信息和函数。
/// @author	DothanTech 刘伟宏
/// 
/// Copyright(C) 2011~2014, DothanTech. All rights reserved.
/// </summary>

using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;

namespace Dothan.ViObject
{
    /// <summary>
    /// ViGET IEC 标准相关的辅助信息和函数。
    /// </summary>
    public static class ViIECStandard
    {
        #region IEC 关键字

        /// <summary>
        /// 是否是 IEC 关键字？
        /// </summary>
        /// <param name="str">名称</param>
        /// <returns>是否是 IEC 关键字？</returns>
        public static bool IsIECKeyWord(string str)
        {
            #region 准备关键字字典

            if (_IEC_KeyWords == null)
            {
                _IEC_KeyWords = new Dictionary<string, bool>();

                _IEC_KeyWords["ABS"] = true;
                _IEC_KeyWords["ACTION"] = true;
                _IEC_KeyWords["AND"] = true;
                _IEC_KeyWords["ANY"] = true;
                _IEC_KeyWords["ANY_DATE"] = true;
                _IEC_KeyWords["ANY_NUM"] = true;
                _IEC_KeyWords["ARRAY"] = true;
                _IEC_KeyWords["AT"] = true;
                _IEC_KeyWords["ACOS"] = true;
                _IEC_KeyWords["ADD"] = true;
                _IEC_KeyWords["ANDN"] = true;
                _IEC_KeyWords["ANY_BIT"] = true;
                _IEC_KeyWords["ANY_INT"] = true;
                _IEC_KeyWords["ANY_REAL"] = true;
                _IEC_KeyWords["ASIN"] = true;
                _IEC_KeyWords["ATAN"] = true;
                _IEC_KeyWords["BOOL"] = true;
                _IEC_KeyWords["BY"] = true;
                _IEC_KeyWords["BUSY"] = true;
                _IEC_KeyWords["BYTE"] = true;
                _IEC_KeyWords["CAL"] = true;
                _IEC_KeyWords["CALCN"] = true;
                _IEC_KeyWords["CD"] = true;
                _IEC_KeyWords["CLAIM"] = true;
                _IEC_KeyWords["CONCAT"] = true;
                _IEC_KeyWords["CONSTANT"] = true;
                _IEC_KeyWords["CTD"] = true;
                _IEC_KeyWords["CTUD"] = true;
                _IEC_KeyWords["CV"] = true;
                _IEC_KeyWords["CALC"] = true;
                _IEC_KeyWords["CASE"] = true;
                _IEC_KeyWords["CDT"] = true;
                _IEC_KeyWords["CLK"] = true;
                _IEC_KeyWords["CONFIGURATION"] = true;
                _IEC_KeyWords["COS"] = true;
                _IEC_KeyWords["CTU"] = true;
                _IEC_KeyWords["CU"] = true;
                _IEC_KeyWords["D"] = true;
                _IEC_KeyWords["DATE_AND_TIME"] = true;
                _IEC_KeyWords["DINT"] = true;
                _IEC_KeyWords["DO"] = true;
                _IEC_KeyWords["DT"] = true;
                _IEC_KeyWords["DATE"] = true;
                _IEC_KeyWords["DELETE"] = true;
                _IEC_KeyWords["DIV"] = true;
                _IEC_KeyWords["DS"] = true;
                _IEC_KeyWords["DWORD"] = true;
                _IEC_KeyWords["ELSE"] = true;
                _IEC_KeyWords["ELSEIF"] = true;
                _IEC_KeyWords["END_ACTION"] = true;
                _IEC_KeyWords["END_CASE"] = true;
                _IEC_KeyWords["END_CONFIGURATION"] = true;
                _IEC_KeyWords["END_FOR"] = true;
                _IEC_KeyWords["END_FUNTION"] = true;
                _IEC_KeyWords["END_FUNCTION_BLOCK"] = true;
                _IEC_KeyWords["END_IF"] = true;
                _IEC_KeyWords["END_PROGRAM"] = true;
                _IEC_KeyWords["END_REPEAT"] = true;
                _IEC_KeyWords["END_RESOURCE"] = true;
                _IEC_KeyWords["END_STEP"] = true;
                _IEC_KeyWords["END_STRUCT"] = true;
                _IEC_KeyWords["END_TRANSITION"] = true;
                _IEC_KeyWords["END_TYPE"] = true;
                _IEC_KeyWords["END_TYPE"] = true;
                _IEC_KeyWords["END_VAR"] = true;
                _IEC_KeyWords["END_WHILE"] = true;
                _IEC_KeyWords["EN"] = true;
                _IEC_KeyWords["ENO"] = true;
                _IEC_KeyWords["EQ"] = true;
                _IEC_KeyWords["ET"] = true;
                _IEC_KeyWords["EXIT"] = true;
                _IEC_KeyWords["EXP"] = true;
                _IEC_KeyWords["EXPT"] = true;
                _IEC_KeyWords["FALSE"] = true;
                _IEC_KeyWords["F_EDGE"] = true;
                _IEC_KeyWords["F_TRIG"] = true;
                _IEC_KeyWords["FIND"] = true;
                _IEC_KeyWords["FOR"] = true;
                _IEC_KeyWords["FROM"] = true;
                _IEC_KeyWords["FUNCTION"] = true;
                _IEC_KeyWords["FUNCTION_BLOCK"] = true;
                _IEC_KeyWords["GE"] = true;
                _IEC_KeyWords["GT"] = true;
                _IEC_KeyWords["IF"] = true;
                _IEC_KeyWords["IN"] = true;
                _IEC_KeyWords["INITIAL_STEP"] = true;
                _IEC_KeyWords["INSERT"] = true;
                _IEC_KeyWords["INT"] = true;
                _IEC_KeyWords["INTERVAL"] = true;
                _IEC_KeyWords["JMP"] = true;
                _IEC_KeyWords["JMPC"] = true;
                _IEC_KeyWords["JMPCN"] = true;
                _IEC_KeyWords["L"] = true;
                _IEC_KeyWords["LD"] = true;
                _IEC_KeyWords["LDN"] = true;
                _IEC_KeyWords["LE"] = true;
                _IEC_KeyWords["LEFT"] = true;
                _IEC_KeyWords["LEN"] = true;
                _IEC_KeyWords["LIMIT"] = true;
                _IEC_KeyWords["LINT"] = true;
                _IEC_KeyWords["LN"] = true;
                _IEC_KeyWords["LOG"] = true;
                _IEC_KeyWords["LREAL"] = true;
                _IEC_KeyWords["LT"] = true;
                _IEC_KeyWords["LWORD"] = true;
                _IEC_KeyWords["MAX"] = true;
                _IEC_KeyWords["MID"] = true;
                _IEC_KeyWords["MIN"] = true;
                _IEC_KeyWords["MOD"] = true;
                _IEC_KeyWords["MOVE"] = true;
                _IEC_KeyWords["MUL"] = true;
                _IEC_KeyWords["MUX"] = true;
                _IEC_KeyWords["N"] = true;
                _IEC_KeyWords["NE"] = true;
                _IEC_KeyWords["NEG"] = true;
                _IEC_KeyWords["NOT"] = true;
                _IEC_KeyWords["OF"] = true;
                _IEC_KeyWords["ON"] = true;
                _IEC_KeyWords["OR"] = true;
                _IEC_KeyWords["ORN"] = true;
                _IEC_KeyWords["P"] = true;
                _IEC_KeyWords["PRIORITY"] = true;
                _IEC_KeyWords["PROGRAM"] = true;
                _IEC_KeyWords["PT"] = true;
                _IEC_KeyWords["PV"] = true;
                _IEC_KeyWords["Q"] = true;
                _IEC_KeyWords["Q1"] = true;
                _IEC_KeyWords["QU"] = true;
                _IEC_KeyWords["QD"] = true;
                _IEC_KeyWords["R"] = true;
                _IEC_KeyWords["R1"] = true;
                _IEC_KeyWords["R_TRIG"] = true;
                _IEC_KeyWords["READ_ONLY"] = true;
                _IEC_KeyWords["READ_WRITE"] = true;
                _IEC_KeyWords["REAL"] = true;
                _IEC_KeyWords["RELEASE"] = true;
                _IEC_KeyWords["REPEAT"] = true;
                _IEC_KeyWords["REPLACE"] = true;
                _IEC_KeyWords["RESOURCE"] = true;
                _IEC_KeyWords["RET"] = true;
                _IEC_KeyWords["RETAIN"] = true;
                _IEC_KeyWords["RETC"] = true;
                _IEC_KeyWords["RETCN"] = true;
                _IEC_KeyWords["RETURN"] = true;
                _IEC_KeyWords["RIGHT"] = true;
                _IEC_KeyWords["ROL"] = true;
                _IEC_KeyWords["ROR"] = true;
                _IEC_KeyWords["RS"] = true;
                _IEC_KeyWords["RTC"] = true;
                _IEC_KeyWords["R_EDGE"] = true;
                _IEC_KeyWords["S"] = true;
                _IEC_KeyWords["S1"] = true;
                _IEC_KeyWords["SD"] = true;
                _IEC_KeyWords["SEL"] = true;
                _IEC_KeyWords["SEMA"] = true;
                _IEC_KeyWords["SHL"] = true;
                _IEC_KeyWords["SHR"] = true;
                _IEC_KeyWords["SIN"] = true;
                _IEC_KeyWords["SINGLE"] = true;
                _IEC_KeyWords["SINT"] = true;
                _IEC_KeyWords["SL"] = true;
                _IEC_KeyWords["SQRT"] = true;
                _IEC_KeyWords["SR"] = true;
                _IEC_KeyWords["ST"] = true;
                _IEC_KeyWords["STEP"] = true;
                _IEC_KeyWords["STN"] = true;
                _IEC_KeyWords["STRING"] = true;
                _IEC_KeyWords["STRUCT"] = true;
                _IEC_KeyWords["SUB"] = true;
                _IEC_KeyWords["TAN"] = true;
                _IEC_KeyWords["TASK"] = true;
                _IEC_KeyWords["THEN"] = true;
                _IEC_KeyWords["TIME"] = true;
                _IEC_KeyWords["TIME_OF_DAY"] = true;
                _IEC_KeyWords["TO"] = true;
                _IEC_KeyWords["TOD"] = true;
                _IEC_KeyWords["TOF"] = true;
                _IEC_KeyWords["TON"] = true;
                _IEC_KeyWords["TP"] = true;
                _IEC_KeyWords["TRANSITION"] = true;
                _IEC_KeyWords["TRUE"] = true;
                _IEC_KeyWords["TYPE"] = true;
                _IEC_KeyWords["UDINT"] = true;
                _IEC_KeyWords["UINT"] = true;
                _IEC_KeyWords["ULINT"] = true;
                _IEC_KeyWords["UNTIL"] = true;
                _IEC_KeyWords["USINT"] = true;
                _IEC_KeyWords["VAR"] = true;
                _IEC_KeyWords["VAR_ACCESS"] = true;
                _IEC_KeyWords["VAR_EXTERNAL"] = true;
                _IEC_KeyWords["VAR_GLOBAL"] = true;
                _IEC_KeyWords["VAR_INPUT"] = true;
                _IEC_KeyWords["VAR_IN_OUT"] = true;
                _IEC_KeyWords["VAR_OUTPUT"] = true;
                _IEC_KeyWords["WHILE"] = true;
                _IEC_KeyWords["WITH"] = true;
                _IEC_KeyWords["WORD"] = true;
                _IEC_KeyWords["XOR"] = true;
                _IEC_KeyWords["XORN"] = true;
                _IEC_KeyWords["ELSIF"] = true;
                _IEC_KeyWords["TRUE"] = true;
                _IEC_KeyWords["END_FUNCTION"] = true;
            }

            #endregion

            return _IEC_KeyWords.ContainsKey(str.ToUpper());
        }
        static private Dictionary<string, bool> _IEC_KeyWords = null;

        #endregion

        #region IEC 对象/变量名称

        /// <summary>
        /// 是否是有效的 IEC 对象/变量名称？
        /// </summary>
        /// <param name="str">名称</param>
        /// <returns>是否是有效的 IEC 对象/变量名称？</returns>
        public static bool IsValidName(string str)
        {
            if (string.IsNullOrEmpty(str))
                return false;

            // Name can only contains 0~9, a~z, A~Z, and _.
            foreach (char ch in str)
            {
                if (!(ch >= '0' && ch <= '9' ||
                      ch >= 'a' && ch <= 'z' ||
                      ch >= 'A' && ch <= 'Z' ||
                      ch == '_'))
                    return false;
            }

            // and can NOT be started with digital, and can NOT be terminated with _.
            if (str[0] >= '0' && str[0] <= '9')
                return false;
            if (str[str.Length - 1] == '_')
                return false;

            return true;
        }

        #endregion

        #region IEC 初始值数值

        /// <summary>
        /// 是否是有效的初始值？
        /// </summary>
        /// <param name="dataType">数据类型</param>
        /// <param name="value">初始值</param>
        /// <returns>是否是有效的初始值？是则返回 true</returns>
        public static bool IsValidValue(ViDataType dataType, string value)
        {
            return IsValidValue(dataType.Name, value);
        }

        /// <summary>
        /// 是否是有效的初始值？
        /// </summary>
        /// <param name="dataTypeName">数据类型名称</param>
        /// <param name="value">初始值</param>
        /// <returns>是否是有效的初始值？是则返回 true</returns>
        public static bool IsValidValue(string dataTypeName, string value)
        {
            // 定义有符号整型参数
            int i;
            // 定义无符号整型参数
            uint ui;
            // 定义有符号长整形参数
            long l;
            // 定义无符号长整型参数
            ulong ul;
            // 定义字符型参数
            string str;
            // 定义时间型参数
            DateTime dt;

            try
            {
                switch (dataTypeName)
                {
                    case "ANY_NUM":
                        foreach (char ch in value)
                        {
                            if (ch < '0' || ch > '9')
                            {
                                return false;
                            }
                        }
                        return true;

                    case "RTIME":
                        return true;
                    case "VARINFO":
                        return true;
                    case "VME_ADDRESS":
                        return true;

                    case "BYTE":
                        i = Convert.ToByte(value);
                        return true;
                    case "DATE":
                        if (Regex.IsMatch(value, @"^[Dd]+[#]+\d{4}[-]+\d\d[-]+\d\d$"))
                        {
                            if (Convert.ToInt32(value.Substring(2, 4)) > 0 && Convert.ToInt32(value.Substring(2, 4)) < 2151)
                            {
                                str = value.Substring(2);
                                dt = Convert.ToDateTime(str);
                                return true;
                            }
                        }
                        break;
                    case "DATE_AND_TIME":
                        if (Regex.IsMatch(value, @"^[Dd]+[Tt]+[#]+\d{4}[-]+\d\d[-]+\d\d[-]+\d\d[:]\d\d[:]\d\d(\.\d\d?)?$"))
                        {
                            if (Convert.ToInt32(value.Substring(3, 4)) > 0 && Convert.ToInt32(value.Substring(3, 4)) < 2151)
                            {
                                str = value.Substring(3, 10);
                                dt = Convert.ToDateTime(str);
                                str = value.Substring(14);
                                dt = Convert.ToDateTime(str);
                                return true;
                            }
                        }
                        break;
                    case "DINT":
                        i = Convert.ToInt32(value);
                        return true;
                    case "DWORD":
                        ui = Convert.ToUInt32(value);
                        return true;
                    case "INT":
                        i = Convert.ToInt16(value);
                        return true;
                    case "LINT":
                        l = (long)Convert.ToInt64(value);
                        return true;
                    case "LREAL":
                        double d;
                        return double.TryParse(value, out d);
                    case "LWORD":
                        ul = (ulong)Convert.ToUInt64(value);
                        return true;
                    case "REAL":
                        float f;
                        return float.TryParse(value, out f);
                    case "SINT":
                        i = Convert.ToSByte(value);
                        return true;
                    case "STRING":
                        foreach (char ch in value)
                        {
                            if (ch > 0x80)
                            {
                                return false;
                            }
                        }
                        return true;
                    case "TIME":
                        if (Regex.IsMatch(value, @"^[Tt]+[#]+-?$"))
                        {
                            return false;
                        }
                        else if (Regex.IsMatch(value, @"^[Tt]+[#]+-?(\d+(\.\d+)?[Dd])?(\d+(\.\d+)?[Hh])?(\d+(\.\d+)?[Mm])?(\d+(\.\d+)?[Ss])?(\d+(\.\d+)?[Mm][Ss])?$"))
                        {
                            return true;
                        }
                        break;
                    case "TIME_OF_DAY":
                        if (Regex.IsMatch(value, @"^[Tt]+[Oo]+[Dd]+[#]+\d\d[:]\d\d[:]\d\d(\.\d\d?)?$"))
                        {
                            str = value.Substring(4);
                            dt = Convert.ToDateTime(str);
                            return true;
                        }
                        break;
                    case "UDINT":
                        ui = Convert.ToUInt32(value);
                        return true;
                    case "UINT":
                        ui = Convert.ToUInt16(value);
                        return true;
                    case "ULINT":
                        ul = (ulong)Convert.ToUInt64(value);
                        return true;
                    case "USINT":
                        i = Convert.ToByte(value);
                        return true;
                    case "WORD":
                        ui = Convert.ToUInt16(value);
                        return true;
                    case "WSTRING":
                        foreach (char ch in value)
                        {
                            if (ch < 0x80)
                            {
                                return false;
                            }
                        }
                        return true;
                    default:
                        break;
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        #endregion

        #region IEC字符串长度

        /// <summary>
        /// 检测字符串长度，如果关键字长度超过64个字符，编译的时候回报错，为了避免这种错误，可以在创建的时候通过该函数进行非法性检测；
        /// </summary>
        /// <param name="str">检测字符串</param>
        /// <param name="validLength">目标字符串最大长度</param>
        /// <returns></returns>
        public static bool IsValidLength(string str, int validLength = 64)
        {
            // 不检查空白字符串，字检查长度
            if (str == null)
                return false;

            if (str.Length > validLength)
                return false;

            return true;
        }
        
        #endregion
    }
}

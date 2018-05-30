using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Dothan.Helpers
{
    public class DzKeyBoardHelper
    {
        /// <summary>
        /// 判断给定的Key值是否是数字。
        /// </summary>
        public static bool IsNumKeyPressed(Key key)
        {
            if (key >= Key.D0 && key <= Key.D9 || key >= Key.NumPad0 && key <= Key.NumPad9)
                return true;

            return false;
        }

        /// <summary>
        /// 判断给定的Key值是否为字符键。
        /// </summary>
        public static bool IsCharKeyPressed(Key key)
        {
            if (key >= Key.A && key <= Key.Z)
                return true;

            return false;
        }

        /// <summary>
        /// 判断给定的Key值是否是字符或数字。
        /// </summary>
        public static bool IsCharOrNumKeyPressed(Key key)
        {
            return IsCharKeyPressed(key) || IsNumKeyPressed(key);
        }

    }
}

/// <summary>
/// @file   BitmapHelper.cs
///	@brief  提供 Bitmap 操作的辅助函数。
/// @author	DothanTech 刘伟宏
/// 
/// Copyright(C) 2011~2014, DothanTech. All rights reserved.
/// </summary>

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace Dothan.Helpers
{
    /// <summary>
    /// 提供 Bitmap 操作的辅助函数。
    /// </summary>
    public static class BitmapHelper
    {
        /// <summary>
        /// 从文件中读取位图对象。因为通过标准的 Uri 的方式读取图像时，图像文件不知道何时被
        /// 释放，这个就导致临时图像文件可能不能被成功删除，因此此处用读取文件到内存的方式，
        /// 然后从内存中创建图像对象，就解决该问题了。
        /// </summary>
        public static BitmapFrame LoadImage(string fileName)
        {
            try
            {
                using (FileStream fs = new FileStream(fileName, FileMode.Open))
                {
                    // 先将文件全部读取到内存
                    byte[] buff = new byte[fs.Length];
                    fs.Read(buff, 0, (int)fs.Length);

                    // 根据内存中的文件内容，创建位图对象
                    MemoryStream ms = new MemoryStream(buff);
                    return BitmapFrame.Create(ms);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}

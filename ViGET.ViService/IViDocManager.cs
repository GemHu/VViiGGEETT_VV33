using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DothanTech.ViGET.ViService
{
    public interface IViDocManager
    {
        /// <summary>
        /// 打开目标文件；
        /// </summary>
        bool OpenDocument(String fileName);

        /// <summary>
        /// 关闭目标文件；
        /// </summary>
        bool CloseDocument(String fileName);

        /// <summary>
        /// 关闭所有文档；
        /// </summary>
        void CloseAllDocuments();

        ///// <summary>
        ///// 关闭所有其他的文档；
        ///// </summary>
        ///// <param name="fileName"></param>
        //void CloseAllButThis(String fileName);
    }
}

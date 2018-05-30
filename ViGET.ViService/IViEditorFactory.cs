using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DothanTech.ViGET.ViService
{
    public interface IViEditorFactory
    {
        /// <summary>
        /// 创建一个编辑器实例；
        /// </summary>
        IViEditor CreateEditorInstance();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DothanTech.ViGET.Model
{
    /// <summary>
    /// 相关硬件模块基类；
    /// </summary>
    public class ViModel
    {
        /// <summary>
        /// 当前模块实例的唯一标识；
        /// </summary>
        public Guid UUID { get; set; }

        /// <summary>
        /// 模块名称；
        /// </summary>
        public String ModelName { get; set; }

        /// <summary>
        /// 模块类型；
        /// </summary>
        public String ModelType { get; set; }

        /// <summary>
        /// 模块配置文件相对路径；
        /// </summary>
        public String Include { get; set; }
    }

    /// <summary>
    /// CPU模块
    /// </summary>
    class CPUModel : ViModel
    {
    }

    /// <summary>
    /// 机架模块；
    /// </summary>
    public class RackModel : ViModel
    {

    }

    /// <summary>
    /// 站点模块；
    /// </summary>
    public class StationModel : ViModel
    {
        public RackModel Rack { get; set; }

        public List<CPUModel> CPUList { get; set; }
    }
}

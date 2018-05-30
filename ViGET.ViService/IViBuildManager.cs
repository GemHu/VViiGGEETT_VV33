using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DothanTech.ViGET.ViService
{
    public interface IViBuildManager
    {
        bool Build(IViBuildGroup group, bool rebuild = false);

        bool Build(IViBuildItem item, bool rebuild = false);

        bool Clean(IViBuildGroup group);

        bool Clean(IViBuildItem item);

        /// <summary>
        /// 停止正在进行的相关任务；
        /// </summary>
        void Stop();
    }

    public interface IViBuildItem
    {
        String BuildFile { get; }
    }

    public interface IViBuildGroup
    {
        void LoopBuildItem(Action<IViBuildItem> action);
    }
}

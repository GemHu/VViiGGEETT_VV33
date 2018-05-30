using DothanTech.ViGET.ViService;
using Prism.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DothanTech.ViGET.Shell
{
    public class ViLogger : ILoggerFacade
    {
        /// <summary>
        /// 当相关窗口未加载完毕的时候，可以先将相关的Log信息备份起来；
        /// </summary>
        private readonly Queue<Tuple<string, Category, Priority>> savedLogs =
            new Queue<Tuple<string, Category, Priority>>();

        /// <summary>
        /// 相关输出窗口加载完毕后，通过调用该回调来显示Log信息；
        /// </summary>
        public Action<string, Category, Priority> Callback { get; set; }

        public void Log(string message, Category category, Priority priority)
        {
            if (this.Callback != null)
            {
                this.Callback(message, category, priority);
            }
            else
            {
                this.savedLogs.Enqueue(new Tuple<String, Category, Priority>(message, category, priority));
            }
        }

        /// <summary>
        /// 当相关输出模块加载完毕后，可以通过该方法来取出已经保存了的所有log信息；
        /// </summary>
        public void ReplaySavedLogs()
        {
            if (this.Callback != null)
            {
                while (this.savedLogs.Count > 0)
                {
                    var log = this.savedLogs.Dequeue();
                    this.Callback(log.Item1, log.Item2, log.Item3);
                }
            }
        }
    }

    /// <summary>
    /// Output窗口信息；
    /// </summary>
    public class ViOutputLogger : ViLogger
    {

    }

    /// <summary>
    /// ErrorList窗口信息；
    /// </summary>
    public class ViErrorListLogger : ViLogger, IViErrorListLogger
    {

    }
}

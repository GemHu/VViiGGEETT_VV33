using Prism.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DothanTech.ViGET.ViService
{
    public interface IViErrorListLogger
    {
        void Log(string message, Category category, Priority priority);
    }
}

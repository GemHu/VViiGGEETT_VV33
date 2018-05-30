using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dothan.Helpers;
using System.Diagnostics;
using Dothan.ViObject;
using System.IO;
namespace DothanTech.ViGET.Manager
{
    public partial class ViCPUInfo
    {
        #region Tasks

        public ViObservableCollection<ViTaskInfo> Tasks
        {
            get
            {
                if (this._tasks == null)
                    this._tasks = new ViObservableCollection<ViTaskInfo>();

                return this._tasks;
            }
        }
        private ViObservableCollection<ViTaskInfo> _tasks;

        #endregion

        #region Children

        #endregion
    }
}

using Dothan.ViObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DothanTech.ViGET.Manager
{
    public partial class SolutionManager
    {
        #region 用于监控需要持久化的数据；
        
        /// <summary>
        /// 用于监控需要持久化的数据是否发生变化；
        /// </summary>
        private void OnChildPropertyChanged(object sender, ViDataChangedEventArgs e)
        {
            if ((e.ChangeType & ViChangeType.CauseDirty) != ViChangeType.None)
            {
                this.IsDirty = true;
                // 项目个数发生了变化，则需要实时更新显示信息；
                this.UpdateShownName();
            }
        }

        #endregion

    }
}

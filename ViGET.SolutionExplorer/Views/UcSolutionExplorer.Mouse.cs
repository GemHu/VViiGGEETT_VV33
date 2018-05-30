using Dothan.Helpers;
using DothanTech.ViGET.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DothanTech.ViGET.SolutionExplorer
{
    /// <summary>
    /// SolutionExplorer鼠标及事件相关属性及方法；
    /// </summary>
    public partial class UcSolutionExplorer
    {
        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);

            Point pos = e.GetPosition(treeProjects);
            HitTestResult result = VisualTreeHelper.HitTest(treeProjects, pos);
            TreeViewItem item = null;
            if (result != null)
                item = ViewHelper.FindVisualParent<TreeViewItem>(result.VisualHit);

            if (e.RightButton == MouseButtonState.Pressed)
            {
                if (item != null)
                    item.IsSelected = true;
            }

            if (e.ClickCount == 2)
            {
                if (item != null && item.DataContext is ViFileInfo)
                {
                    ViFileInfo node = item.DataContext as ViFileInfo;
                    node.DoubleClick(e);
                }

                return;
            }
        }
    }
}

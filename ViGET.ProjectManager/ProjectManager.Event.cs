using Dothan.Controls;
using DothanTech.ViGET.ViCommand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DothanTech.ViGET.Manager
{
    public partial class ProjectManager
    {
        #region Command

        public override void CanExecuteCommand(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            if (e.Command == ViCommands.AddExistingItem)
            {
                e.CanExecute = true;
                return;
            }
            else if (e.Command == ApplicationCommands.Delete)
            {
                e.CanExecute = true;
            }
            else if (e.Command == ViCommands.Rename)
            {

            }


            base.CanExecuteCommand(sender, e);
        }

        public override void ExecutedCommand(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            if (e.Command == ViCommands.AddExistingItem)
            {
                // 1、弹出文件选择对话框；

                // 2、判断目标文件在哪，如果不在当前目录下，则复制目标文件（复制文件待定，因为还需要考虑依赖项）；

                // 3、加载目标文件；
                base.AddExistingItem();
            }
            else if (e.Command == ApplicationCommands.Delete)
            {
                if (ViMessageBox.Show("确定要删除该项目吗？", "Delete Project", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        this.TheSolution.RemoveProject(this);
                    }));
                }
            }
            else if (e.Command == ViCommands.Rename)
            {
                String newName = e.Parameter as String;
                if (String.IsNullOrEmpty(newName))
                    return;

                //this.RenameProject(newName);
            }


            base.ExecutedCommand(sender, e);
        }


        #endregion
    }
}

using Dothan.ViObject;
using DothanTech.ViGET.ViCommand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DothanTech.ViGET.Manager
{
    public partial class ViCPUInfo
    {
        #region Command

        public override void CanExecuteCommand(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            if (e.Command == ViCommands.AddNewItem || 
                e.Command == ViCommands.AddExistingItem)
            {
                e.CanExecute = true;
                return;
            }

            base.CanExecuteCommand(sender, e);
        }

        public override void ExecutedCommand(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            if (e.Command == ViCommands.AddNewItem)
            {
                this.TheFactory.TempWizard.RunCreateFileWizard(this.DirectoryPath, this.Type, (file, type) =>
                {
                    this.AddChild(ViFileInfo.CreateFile(type, file));
                });
            }
            else if (e.Command == ViCommands.AddExistingItem)
            {

            }

            base.ExecutedCommand(sender, e);
        }

        #endregion
    }
}

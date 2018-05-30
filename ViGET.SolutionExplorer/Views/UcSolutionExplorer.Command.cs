using DothanTech.ViGET.Manager;
using DothanTech.ViGET.ViCommand;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DothanTech.ViGET.SolutionExplorer
{
    public partial class UcSolutionExplorer
    {
        private void initCommands()
        {
			//-----------------------Project相关命令--------------------//
            this.CommandBindings.Add(new CommandBinding(ViCommands.AddNewProject, this.AddNewProject_Executed, this.AddNewProject_CanExecute));
            this.CommandBindings.Add(new CommandBinding(ViCommands.AddExistingProject, this.AddExistingProject_Executed, this.AddExistingProject_CanExecute));
            this.CommandBindings.Add(new CommandBinding(ViCommands.OpenLocalFolder, this.ExecutedCommand, this.CanExecuteCommand));

            //--------------------- Project Item 相关命令--------------------//
            this.CommandBindings.Add(new CommandBinding(ViCommands.AddNewItem, this.ExecutedCommand, this.CanExecuteCommand));
            this.CommandBindings.Add(new CommandBinding(ViCommands.AddExistingItem, this.ExecutedCommand, this.CanExecuteCommand));

            //
            //this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, this.ExecutedCommand, this.CanExecuteCommand));
            //this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Cut, this.ExecutedCommand, this.CanExecuteCommand));
            //this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, this.ExecutedCommand, this.CanExecuteCommand));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Delete, this.ExecutedCommand, this.CanExecuteCommand));
            //this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Print, this.ExecutedCommand, this.CanExecuteCommand));
            //this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Properties, this.ExecutedCommand, this.CanExecuteCommand));
            //this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Find, this.ExecutedCommand, this.CanExecuteCommand));

            //this.CommandBindings.Add(new CommandBinding(ViCommands.Build, this.ExecutedCommand, this.CanExecuteCommand));
            //this.CommandBindings.Add(new CommandBinding(ViCommands.Rebuild, this.ExecutedCommand, this.CanExecuteCommand));
            //this.CommandBindings.Add(new CommandBinding(ViCommands.BatchBuild, this.ExecutedCommand, this.CanExecuteCommand));
            //this.CommandBindings.Add(new CommandBinding(ViCommands.Clean, this.ExecutedCommand, this.CanExecuteCommand));
            //this.CommandBindings.Add(new CommandBinding(ViCommands.Rename, this.ExecutedCommand, this.CanExecuteCommand));
            //this.CommandBindings.Add(new CommandBinding(ViCommands.Git, this.ExecutedCommand, this.CanExecuteCommand));
            //this.CommandBindings.Add(new CommandBinding(ViCommands.OpenLocalFolder, this.ExecutedCommand, this.CanExecuteCommand));
            //this.CommandBindings.Add(new CommandBinding(ViCommands.Active, this.ExecutedCommand, this.CanExecuteCommand));
            //this.CommandBindings.Add(new CommandBinding(ViCommands.Online, this.ExecutedCommand, this.CanExecuteCommand));

        }

        [Import(typeof(IViProjectFactory))]
        public ProjectFactory Factory { get; set; }

        #region 通用命令执行函数

        private void CanExecuteCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Parameter is ViFileInfo)
            {
                (e.Parameter as ViFileInfo).CanExecuteCommand(sender, e);
            }
        }

        private void ExecutedCommand(object sender, ExecutedRoutedEventArgs e)
        {
            (e.Parameter as ViFileInfo).ExecutedCommand(sender, e);
        }

        #endregion

        #region AddNewProject Command

        private void AddNewProject_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void AddNewProject_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Factory.AddNewProject();
        }

        #endregion

        #region AddExistingProject Command

        private void AddExistingProject_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void AddExistingProject_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Factory.AddExistingProject();
        }

        #endregion

        
    }
}

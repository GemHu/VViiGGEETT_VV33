using Dothan.Helpers;
using DothanTech.ViGET.SolutionExplorer;
using DothanTech.ViGET.ViCommand;
using DothanTech.ViGET.ViService;
using MahApps.Metro;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DothanTech.ViGET.Shell
{
    public partial class ViShell
    {
        #region initCommand

        private void InitCommand()
        {
            // --------------------- Project 相关命令----------------------- //
            this.CommandBindings.Add(new CommandBinding(ViCommands.NewProject, this.NewProject_Executed, this.NewProject_CanExecute));
            this.CommandBindings.Add(new CommandBinding(ViCommands.OpenProject, this.OpenProject_Executed, this.OpenProject_CanExecute));
            this.CommandBindings.Add(new CommandBinding(ViCommands.AddNewProject, this.AddNewProject_Executed, this.AddNewProject_CanExecute));
            this.CommandBindings.Add(new CommandBinding(ViCommands.AddExistingProject, this.AddExistingProject_Executed, this.AddExistingProject_CanExecute));
            this.CommandBindings.Add(new CommandBinding(ViCommands.CloseSolution, this.CloseSolution_Executed, this.CloseSolution_CanExecute));

        }

        #endregion

        #region SolutionExplorer 相关命令

        public ProjectFactory Factory
        {
            get
            {
                return ProjectFactory.GetInstance();
            }
        }

        #region 新建项目 Command

        private void NewProject_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.Factory.NewProjectCmd.CanExecute();
        }

        private void NewProject_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Factory.NewProjectCmd.Execute();
        }

        #endregion

        #region 打开已有项目 Command

        private void OpenProject_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.Factory.OpenProjectCmd.CanExecute(null);
        }

        private void OpenProject_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Factory.OpenProjectCmd.Execute(null);
        }

        #endregion

        #region 添加新项目 Command

        private void AddNewProject_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.Factory.AddNewProjectCmd.CanExecute();
        }

        private void AddNewProject_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Factory.AddNewProjectCmd.Execute();
        }

        #endregion

        #region 添加已有项目 Command

        private void AddExistingProject_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.Factory.AddExistingProjectCmd.CanExecute(null);
        }

        private void AddExistingProject_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Factory.AddExistingProjectCmd.Execute(null);
        }

        #endregion

        #region 关闭解决方案 Command

        private void CloseSolution_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.Factory.CloseSolutionCmd.CanExecute();
        }

        private void CloseSolution_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Factory.CloseSolutionCmd.Execute();
        }

        #endregion

        #region 切换主题

        public DelegateCommand<String> ChangeThemeCmd
        {
            get
            {
                if (this._changeThemeCmd == null)
                {
                    this._changeThemeCmd = new DelegateCommand<string>((theme) =>
                    {
                        this.ChangeAppStyle(null, theme);
                    });
                }
                return this._changeThemeCmd;
            }
        }
        private DelegateCommand<String> _changeThemeCmd;

        public DelegateCommand<String> ChangeAccentCmd
        {
            get
            {
                if (this._changeAccentCmd == null)
                {
                    this._changeAccentCmd = new DelegateCommand<string>((accent) =>
                    {
                        this.ChangeAppStyle(accent, null);
                    });
                }
                return this._changeAccentCmd;
            }
        }
        private DelegateCommand<String> _changeAccentCmd;

        private void ChangeAppStyle(String accent, String theme)
        {
            String appAccentKey = "AppAccent";
            String appThemeKey = "AppTheme";
            // 获取当前样式
            DzAppDataConfig config = new DzAppDataConfig();
            if (String.IsNullOrEmpty(accent))
                accent = config.GetValue(appAccentKey, "Blue");
            if (String.IsNullOrEmpty(theme))
                theme = config.GetValue(appThemeKey, "BaseLight");

            // now set the Green accent and dark theme
            ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent(accent), ThemeManager.GetAppTheme(theme));
            config.SetValue(appAccentKey, accent);
            config.SetValue(appThemeKey, theme);
        }

        #endregion

        #endregion
    }
}

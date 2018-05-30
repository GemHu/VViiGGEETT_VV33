/// <summary>
/// @file   UcSolutionExplorer.xaml.cs
///	@brief  Solution Explorer 操作界面；
/// @author	DothanTech 胡殿兴
/// 
/// Copyright(C) 2011~2018, DothanTech. All rights reserved.
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using DothanTech.ViGET.ViCommand;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition;
using DothanTech.ViGET.ViService;
using DothanTech.ViGET.Manager;

namespace DothanTech.ViGET.SolutionExplorer
{
    [Export]
    public partial class UcSolutionExplorer : UserControl, IPartImportsSatisfiedNotification
    {
        public UcSolutionExplorer()
        {
            InitializeComponent();
            // 初始化相关命令；
            this.initCommands();
        }

        public void OnImportsSatisfied()
        {
            this.DataContext = this.Factory;
        }
    }
}

/// <summary>
/// @file   ViCommands.cs
///	@brief  ViGET项目中所用到的相关命令。
/// @author	DothanTech 胡殿兴
/// 
/// Copyright(C) 2011~2018, DothanTech. All rights reserved.
/// </summary>

using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace DothanTech.ViGET.ViCommand
{
    public class ViCommands
    {
        //----------------- Project相关命令----------------//
        public static RoutedCommand NewProject = new RoutedCommand();
        public static RoutedCommand OpenProject = new RoutedCommand();
        public static RoutedCommand AddNewProject = new RoutedCommand();
        public static RoutedCommand AddExistingProject = new RoutedCommand();
        public static RoutedCommand CloseSolution = new RoutedCommand();
        public static RoutedCommand Exit = new RoutedCommand();
        public static RoutedCommand OpenLocalFolder = new RoutedCommand();

        // ------------------ProjectItem相关命令--------------//
        public static RoutedUICommand AddNewItem = new RoutedUICommand();
        public static RoutedUICommand AddExistingItem = new RoutedUICommand();

        //-------------------File-------------------//
        //public static CompositeCommand NewFile = new CompositeCommand();
        //public static CompositeCommand OpenFile = new CompositeCommand();
        //public static CompositeCommand Close = new CompositeCommand();
        //public static CompositeCommand Save = new CompositeCommand();
        //public static CompositeCommand SaveAll = new CompositeCommand();

        //------------------Comman------------------//;
        public static RoutedUICommand Rename = new RoutedUICommand();

        //----------- Show View----------------//
        //public static RoutedUICommand ShowSolutionExplorer = ViCommands.CreateUICommand("ShowSolutionExplorer");
        //public static RoutedUICommand ShowStartPage = ViCommands.CreateUICommand("ShowStartPage");
        //public static RoutedUICommand ShowErrorList = ViCommands.CreateUICommand("ShowErrorList");
        //public static RoutedUICommand ShowOutput = ViCommands.CreateUICommand("ShowOutput");
        //public static RoutedUICommand ShowPOUs = ViCommands.CreateUICommand("ShowPOUs");
        //public static RoutedUICommand ShowProperties = ViCommands.CreateUICommand("ShowProperties");
        //------------Build------------------//
        public static RoutedUICommand Build = new RoutedUICommand();        // Build CPU
        public static RoutedUICommand Rebuild = new RoutedUICommand();
        public static RoutedUICommand Clean = new RoutedUICommand();

        public static RoutedUICommand BuildSolution = new RoutedUICommand();
        public static RoutedUICommand RebuildSolution = new RoutedUICommand();
        public static RoutedUICommand CleanSolution = new RoutedUICommand();

        public static RoutedUICommand BuildActiveProject = new RoutedUICommand();
        public static RoutedUICommand RebuildActiveProject = new RoutedUICommand();
        public static RoutedUICommand CleanActiveProject = new RoutedUICommand();

        public static RoutedUICommand BatchBuild = new RoutedUICommand();
        public static RoutedUICommand BuildStop = new RoutedUICommand();
        //----------------Git--------------------//

    }
}

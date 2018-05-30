/// <summary>
/// @file   DataGridPus.cs
///	@brief  提供 DataGrid 扩展方法的辅助函数。
/// @author	DothanTech 蔡俊杰
/// 
/// Copyright(C) 2011~2014, DothanTech. All rights reserved.
/// </summary>

using Microsoft.Windows.Controls;
using Microsoft.Windows.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows;

namespace Dothan.Controls
{
    /// <summary>
    /// 提供 DataGrid 扩展方法的辅助函数。
    /// </summary>
    public static class DataGridPus
    {
        /// <summary>  
        /// 获取DataGrid控件单元格  
        /// </summary>  
        /// <param name="dataGrid">DataGrid控件</param>  
        /// <param name="rowIndex">单元格所在的行号</param>  
        /// <param name="columnIndex">单元格所在的列号</param>  
        /// <returns>指定的单元格</returns>  
        public static DataGridCell GetCell(this DataGrid dataGrid, int rowIndex, int columnIndex)
        {
            DataGridRow rowContainer = dataGrid.GetRow(rowIndex);
            if (rowContainer != null)
            {
                DataGridCellsPresenter presenter = GetVisualChild<DataGridCellsPresenter>(rowContainer);
                DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(columnIndex);
                if (cell == null)
                {
                    dataGrid.ScrollIntoView(rowContainer, dataGrid.Columns[columnIndex]);
                    cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(columnIndex);
                }
                return cell;
            }
            return null;
        }

        /// <summary>  
        /// 获取DataGrid的行  
        /// </summary>  
        /// <param name="dataGrid">DataGrid控件</param>  
        /// <param name="rowIndex">DataGrid行号</param>  
        /// <returns>指定的行号</returns>  
        public static DataGridRow GetRow(this DataGrid dataGrid, int rowIndex)
        {
            DataGridRow rowContainer = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(rowIndex);
            if (rowContainer == null)
            {
                dataGrid.UpdateLayout();
                dataGrid.ScrollIntoView(dataGrid.Items[rowIndex]);
                rowContainer = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(rowIndex);
            }
            return rowContainer;
        }

        /// <summary>  
        /// 获取父可视对象中第一个指定类型的子可视对象  
        /// </summary>  
        /// <typeparam name="T">可视对象类型</typeparam>  
        /// <param name="parent">父可视对象</param>  
        /// <returns>第一个指定类型的子可视对象</returns>  
        public static T GetVisualChild<T>(Visual parent) where T : Visual
        {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }  

        /// <summary>
        /// Grid最后一列加行和往右拐
        /// </summary>
        /// <typeparam name="Tresult"></typeparam>
        /// <param name="dataGrid"></param>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="li"></param>
        public static void Grid_Right<Tresult>(this DataGrid dataGrid, object sender, KeyEventArgs e) where Tresult : class, new()
        {
            var uie = e.OriginalSource as UIElement;
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                uie.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                dataGrid.BeginEdit();
                if (dataGrid.CurrentColumn != null)
                {
                    if (dataGrid.CurrentColumn.IsReadOnly == true)
                    {

                        for (int b = dataGrid.CurrentColumn.DisplayIndex; b < dataGrid.Columns.Count; b++)
                        {
                            for (int k = 0; k < dataGrid.Columns.Count; k++)
                            {
                                if (dataGrid.Columns[k].DisplayIndex == b)
                                {
                                    if (dataGrid.Columns[k].IsReadOnly == false && dataGrid.Columns[k].Visibility == Visibility.Visible)
                                    {
                                        dataGrid.CurrentColumn = dataGrid.Columns[k];
                                        dataGrid.SelectedItem = dataGrid.CurrentItem;
                                        dataGrid.BeginEdit();
                                        return;
                                    }
                                    else if (k == dataGrid.Columns.Count - 1 && (dataGrid.Columns[k].IsReadOnly == true || dataGrid.Columns[k].Visibility == Visibility.Hidden))
                                    {
                                        if (dataGrid.SelectedIndex != dataGrid.Items.Count - 1)
                                        {
                                            dataGrid.SelectedIndex = dataGrid.SelectedIndex + 1;
                                            dataGrid.CurrentColumn = dataGrid.Columns[0];
                                            dataGrid.CurrentItem = dataGrid.SelectedItem;
                                            b = 0;
                                        }
                                        else
                                        {
                                            //PropertyInfo p = dataGrid.CurrentItem.GetType();
                                            ObservableCollection<Tresult> li = dataGrid.ItemsSource as ObservableCollection<Tresult>;
                                            li.Insert(dataGrid.SelectedIndex + 1, new Tresult());
                                            dataGrid.SelectedIndex = dataGrid.SelectedIndex + 1;
                                            dataGrid.CurrentColumn = dataGrid.Columns[0];
                                            dataGrid.CurrentItem = dataGrid.SelectedItem;
                                            b = 0;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}

using Microsoft.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows;
using System.Windows.Media;

namespace Dothan.Controls
{
    public class DzDataGrid : DataGrid
    {
        int i = 0;

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            Int32 row = base.Items.IndexOf(base.CurrentItem);
            Int32 Col = base.Columns.IndexOf(base.CurrentColumn);
            // 向下方向键
            if (e.Key == Key.Down)
            {
                if (base.SelectedIndex < base.Items.Count - 1)
                {
                    DataGridCell dgc = this.GetCell(row, Col);
                    dgc.IsSelected = true;
                    dgc.Focus();
                }
            }
            // 向上方向键
            else if (e.Key == Key.Up)
            {
                if (base.SelectedIndex > 0)
                {
                    DataGridCell dgc = this.GetCell(row, Col);
                    dgc.IsSelected = true;
                    dgc.Focus();
                }
            }
            // Tab键
            else if (e.Key == Key.Tab)
            {
                if (i ==0 && Col == 0)
                {
                    var dataGridCellInfo = new DataGridCellInfo(this.Items[row], this.Columns[Col]);
                    this.CurrentCell = dataGridCellInfo;
                    i++;
                }
                else if (i == 1 && Col == 1)
                {
                    DataGridCell dgc = this.GetCell(row, Col);
                    dgc.IsSelected = true;
                    dgc.Focus();
                    i++;
                }
                else
                {
                    if (row > -1 && row < this.Items.Count && Col >= 0 && Col < this.Columns.Count - 1)
                    {
                        var dataGridCellInfo = new DataGridCellInfo(this.Items[row], this.Columns[Col + 1]);
                        this.CurrentCell = dataGridCellInfo;
                        i = 0;
                    }
                    // 聚焦到每行最后一列，聚焦到下一行第一列，最后一行除外
                    else if (row > -1 && row < this.Items.Count - 1 && Col == this.Columns.Count - 1)
                    {
                        var dataGridCellInfo = new DataGridCellInfo(this.Items[row + 1], this.Columns[0]);
                        this.CurrentCell = dataGridCellInfo;
                        i = 0;
                    }
                }
            }
        }
    }
}

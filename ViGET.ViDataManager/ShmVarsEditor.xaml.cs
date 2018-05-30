using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using System.Windows.Controls;
using Microsoft.Windows.Controls;

namespace Dothan.Manager
{
    /// <summary>
    /// Interaction logic for ShmVarEditor.xaml
    /// </summary>
    public partial class ShmVarsEditor : UserControl
    {
        public static DataGrid dataGrid = null;
        private DispatcherTimer timer = null;
        private bool contextMenuOpening = false;
        private static bool hasInvalidValue1 = false; //stores errors from IEC identifier validation (class IECIdentifierRule)
        private static bool hasInvalidValue2 = false; //stores errors from unique identifier validation (class UniqueIdentifierRule)

        internal static bool boolHasInvalidValue = false;//20140103,SKK
        internal static string strNewValidValue = ""; //20140103,SKK

        public static SharedMemoryVariableList ShmVariablesList = null;

        public static bool HasInvalidValue1
        {
            get { return hasInvalidValue1; }
            set
            {
                hasInvalidValue1 = value;

                if (ShmVariablesList != null)
                {
                    if (hasInvalidValue1 || hasInvalidValue2)
                    {
                        ShmVariablesList.IsValid = false;
                    }
                    else
                    {
                        ShmVariablesList.IsValid = true;
                    }
                }
            }
        }

        public static bool HasInvalidValue2
        {
            get { return hasInvalidValue2; }
            set
            {
                hasInvalidValue2 = value;

                if (ShmVariablesList != null)
                {
                    if (hasInvalidValue1 || hasInvalidValue2)
                    {
                        ShmVariablesList.IsValid = false;
                    }
                    else
                    {
                        ShmVariablesList.IsValid = true;
                    }
                }
            }
        }

        public ShmVarsEditor()
        {
            InitializeComponent();

            dataGrid = ShmVarDataGrid;

            //prepare timer for temporary display of a "no matches" message (find function)
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 2);
            timer.Tick += new EventHandler(Timer_Tick);
        }

        public SharedMemoryVariableList ListOfShmVariables
        {
            get { return (SharedMemoryVariableList)GetValue(ListOfShmVariablesProperty); }
            set { SetValue(ListOfShmVariablesProperty, value); }
        }
        public static readonly DependencyProperty ListOfShmVariablesProperty =
            DependencyProperty.Register("ListOfShmVariables", typeof(SharedMemoryVariableList), typeof(ShmVarsEditor), new UIPropertyMetadata(null));

        private IXRef xref = null;

        public void Init(IXRef xref)
        {
            //MessageBox.Show("Init(XRef xref)");  //20130730,SKK
            this.xref = xref;

            if (xref == null)
            {
                ListOfShmVariables = null;
                ShmVariablesList = null;
                editorDockPanel.IsEnabled = false;
            }
            else
            {
                SharedMemoryVariableList list = null;
                xref.GetSharedMemoryManager().GetShmVariableList(ref list);
                ListOfShmVariables = list;
                ShmVariablesList = list;

                //lsu 20120307: enable while initialization
                editorDockPanel.IsEnabled = true;
            }
        }

        public void ToggleEditorEnabled(bool enable)
        {
            editorDockPanel.IsEnabled = enable;
        }

        /// <summary>
        /// 得到保证唯一的变量名称。
        /// </summary>
        /// <returns>保证唯一的变量名称。</returns>
        internal static string GetUniqueVarName()
        {
            string varName = Guid.NewGuid().ToString();
            varName = varName.Replace("-", "");
            varName = "var_" + varName;
            return varName;
        }

        private void DataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (e.Column == DataGridNameColumn || e.Column == DataGridTypeColumn)
            {
                if (xref != null && ShmVarDataGrid.SelectedItem != null)
                {
                    if (((SharedMemoryVariable)ShmVarDataGrid.SelectedItem).IsConnected)
                    {
                        MessageBox.Show("Changing name or data type of this variable is not possible, because the variable is connected.", "Validation Failed", MessageBoxButton.OK, MessageBoxImage.Information);
                        e.Cancel = true;
                    }
                }
            }
        }

        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            /************************************************************
            * 20140103,SKK, 避免命名错误 --- Code start
            * **********************************************************/
            //MessageBox.Show("DataGrid_CellEditEnding");
            if (boolHasInvalidValue)
            {
                //MessageBox.Show("DataGrid_CellEditEnding");
                //(ShmVarDataGrid.CurrentItem as SharedMemoryVariable).Name = strNewValidValue;
                //(ShmVarsEditor.dataGrid.CurrentItem as SharedMemoryVariable).Name = strNewValidValue;
                boolHasInvalidValue = false;
                ((SharedMemoryVariable)ShmVarDataGrid.SelectedItem).Name = strNewValidValue;
            }
            /************************************************************
            * 20140103,SKK, 避免命名错误 --- Code start
            * **********************************************************/
        }

        private void AddNewRow()
        {
            //MessageBox.Show("AddNewRow");       //20130730,SKK
            if (xref == null) return;

            //add a new shared memory variable at the end of the list
            xref.GetSharedMemoryManager().AddNewShmVarWithDefaultValue();

            //first set focus
            ShmVarDataGrid.Focus();

            //select the last element of the list
            ShmVarDataGrid.SelectedItem = ListOfShmVariables.Last();

            //then create a new cell info, with the item we wish to edit and the column number of the cell we want in edit mode
            DataGridCellInfo cellInfo = new DataGridCellInfo(ShmVarDataGrid.SelectedItem, ShmVarDataGrid.Columns[0]);

            //set the cell to be the active one
            ShmVarDataGrid.CurrentCell = cellInfo;

            //scroll the item into view
            ShmVarDataGrid.ScrollIntoView(ShmVarDataGrid.SelectedItem);

            //begin the edit
            ShmVarDataGrid.BeginEdit();
        }

        private void DeleteSelectedRows()
        {
            if (xref == null) return;

            if (ShmVarDataGrid.SelectedItems.Count > 0 && MessageBox.Show("Do you really want to delete all selected variables?", "ViGET V2.0", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            foreach (var item in ShmVarDataGrid.SelectedItems)
            {
                SharedMemoryVariable shmVar = item as SharedMemoryVariable;
                if (shmVar.ConnectionCounter > 0)
                {
                    MessageBox.Show("At least one of the selected variables can not be deleted, because it is connected.\n\nNone of the selected variables will be deleted.", "Delete Not Possible");
                    return;
                }
            }

            int indexOfLastDeletedRow = -1;

            //remove all selected rows(shared memory variables)
            while (ShmVarDataGrid.SelectedItems.Count > 0)
            {
                indexOfLastDeletedRow = ShmVarDataGrid.SelectedIndex;
                xref.GetSharedMemoryManager().RemoveShmVar((ShmVarDataGrid.SelectedItem as SharedMemoryVariable).Name);
            }

            //first set focus
            ShmVarDataGrid.Focus();

            //set selected either the last row or the row below last deleted row
            ShmVarDataGrid.SelectedIndex = Math.Min(indexOfLastDeletedRow, ListOfShmVariables.Count - 1);

            //make the selected item visible
            if (ShmVarDataGrid.SelectedItem != null)
            {
                //then create a new cell info, with the item we wish to edit and the column number of the cell we want in edit mode
                DataGridCellInfo cellInfo = new DataGridCellInfo(ShmVarDataGrid.SelectedItem, ShmVarDataGrid.Columns[0]);
                //set the cell to be the active one
                ShmVarDataGrid.CurrentCell = cellInfo;
                //scroll the item into view
                ShmVarDataGrid.ScrollIntoView(ShmVarDataGrid.SelectedItem);
            }
        }

        private void AddShmVar_Click(object sender, RoutedEventArgs e)
        {
            //check for validation errors
            if (hasInvalidValue1 || hasInvalidValue2)
            {
                MessageBox.Show("There is a validation error in the variables list.\n\nPlease enter a valid value first, then try again.", "Action Currently Not Allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            AddNewRow();
        }

        private void DelShmVar_Click(object sender, RoutedEventArgs e)
        {
            //check for validation errors
            if (hasInvalidValue1 || hasInvalidValue2)
            {
                MessageBox.Show("There is a validation error in the variables list.\n\nPlease enter a valid value first, then try again.", "Action Currently Not Allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DeleteSelectedRows();
        }

        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!(e.OriginalSource is TextBox) && e.Key == Key.Insert)
            {
                AddShmVar_Click(null, null);
                e.Handled = true;
            }

            if (e.Key == Key.Delete)
            {
                if (e.OriginalSource is TextBox)
                {
                    //THIS IS SPECIAL CODE TO FIX AN ISSUE THAT ONLY EXISTS IN THE ViGET FRAMEWORK:
                    //the ViGET frame catches all key down events for the Del key (and others), so it is not reveived any more here
                    //workaround: at least do delete in text boxes on key up of the Del key
                    TextBox textBox = (TextBox)e.OriginalSource;
                    if (textBox.SelectionLength > 0)
                    {
                        //delete selected text
                        int pos = textBox.SelectionStart;
                        textBox.Text = textBox.Text.Substring(0, textBox.SelectionStart) + textBox.Text.Substring(textBox.SelectionStart + textBox.SelectionLength);
                        textBox.Select(pos, 0);
                    }
                    else
                    {
                        //delete one character
                        int pos = textBox.SelectionStart;
                        if (pos < textBox.Text.Length)
                        {
                            textBox.Text = textBox.Text.Substring(0, textBox.SelectionStart) + textBox.Text.Substring(textBox.SelectionStart + 1);
                            textBox.Select(pos, 0);
                        }
                    }
                }
                else
                {
                    //delete variables
                    DelShmVar_Click(null, null);
                    e.Handled = true;
                }
            }

            //ctrl+A for select all
            if (!(e.OriginalSource is TextBox) && e.Key == Key.A)
            {
                if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                {
                    ShmVarDataGrid.SelectAll();
                }

                e.Handled = true;
            }

            //ctrl+F for go to search text box
            if (!(e.OriginalSource is TextBox) && e.Key == Key.F)
            {
                if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                {
                    //toggle visibility of search panel
                    /*if (panelSearch.Visibility != Visibility.Visible)
                    {
                        panelSearch.Visibility = Visibility.Visible;
                        searchTextBox.Focus();
                    }
                    else
                    {
                        panelSearch.Visibility = Visibility.Collapsed;
                    }*/

                    searchTextBox.Focus();
                }

                e.Handled = true;
            }

            //F3 for continue search
            if (!(e.OriginalSource is TextBox) && e.Key == Key.F3)
            {
                SearchTextBox_KeyUp(null, null);
                e.Handled = true;
            }
        }

        private void CheckBoxFast_Click(object sender, RoutedEventArgs e)
        {
            bool checkState = (bool)(sender as CheckBox).IsChecked;
            //simultaneously applay to all selected "fast" checkboxes
            foreach (var item in ShmVarDataGrid.SelectedCells)
            {
                if ((string)item.Column.Header == "Fast")
                {
                    SharedMemoryVariable shmVar = item.Item as SharedMemoryVariable;
                    shmVar.IsFast = checkState;

                    //lsu 20120510: remember to generate POE if necessary
                    if (shmVar.IsConnected)
                    {
                        SharedMemoryConnectionList list = null;
                        xref.GetSharedMemoryManager().GetShmConnectionList(ref list);
                        foreach (var connection in list)
                        {
                            if (connection.Variable.Name == shmVar.Name)
                                xref.GetBuildListManager().Register(connection.PlanName, connection.CPUName);
                        }
                    }
                }
            }
            e.Handled = true;
        }

        private int startIndx = 0;
        private void SearchTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (timer.IsEnabled)
            {
                //stop timer for "no matches" message immediately
                Timer_Tick(null, null);
            }

            if (e == null || e.Key == Key.Return || e.Key == Key.F3)
            {
                ShmVarDataGrid.UnselectAll();
                int i = 0;
                for (i = startIndx; i < ShmVarDataGrid.Items.Count; i++)
                {
                    SharedMemoryVariable currentVar = ShmVarDataGrid.Items[i] as SharedMemoryVariable;
                    if (currentVar.Name.ToUpper().IndexOf(searchTextBox.Text.ToUpper()) == 0)
                    {
                        ShmVarDataGrid.SelectedItem = ShmVarDataGrid.Items[i];
                        startIndx = i + 1;
                        ShmVarDataGrid.ScrollIntoView(ShmVarDataGrid.SelectedItem);
                        break;
                    }
                }

                if (i == ShmVarDataGrid.Items.Count)
                {
                    //not found
                    startIndx = 0;
                    borderFindNoMatches.Visibility = Visibility.Visible; //instead of a message box, show a hint in the search area
                    timer.Start(); //start timer to hide the hint after a short time
                    searchTextBox.Focus();
                }
            }
        }

        private void ButtonFindNext_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox_KeyUp(null, null);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            borderFindNoMatches.Visibility = Visibility.Hidden;
        }

        private void UserControl_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!this.IsMouseOver && !contextMenuOpening)
            {
                //if the user clicks somewhere outside of this control while being in edit mode in the data grid, edit mode must be quit
                //ShmVarDataGrid.CancelEdit();
                ShmVarDataGrid.CommitEdit(DataGridEditingUnit.Row, true);
                //we use commit edit, so that the behaviour is the same as clicking somewhere else within the control; this allows committing e.g. an invalid name, but this case must be handled separately anyway (SHM variables must not be used for new connections while the editor is in "invalid" state)
            }
            //MessageBox.Show("UserControl_LostKeyboardFocus");
            contextMenuOpening = false;
        }

        private void UserControl_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            contextMenuOpening = true;
        }

        /***************************************************************************************************************************
         * 通过绑定的方式当DataGrid中的TextBox是去焦点后，同时DataGrid也没有获取焦点，此时系统不会自动的将TextBox中的数据保存到共享变量中，
         * 所以当TextBox失去焦点的时候，我们需要手动的将TextBox中的数据保存到共享变量中。
         * 从而就有了下面两个方法。
         * *************************************************************************************************************************/

        private void tbName_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            TextBox nameBox = sender as TextBox;
            SharedMemoryVariable shmVar = nameBox.Tag as SharedMemoryVariable;
            if (this.checkIECIdentifier(nameBox.Text, shmVar) && this.checkUniqueIdentifier(nameBox.Text, shmVar))
            {
                shmVar.Name = nameBox.Text;
            }
        }

        private void tbComment_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            TextBox commentBox = sender as TextBox;
            SharedMemoryVariable shmVar = commentBox.Tag as SharedMemoryVariable;
            shmVar.Comment = commentBox.Text;
        }

        private bool checkIECIdentifier(string value, SharedMemoryVariable shmVar)
        {
            if (!SharedMemoryVariableList.IsValidIECIdentifier(value))
            {
                MessageBox.Show("The entered name is not a valid IEC 61131 identifier.\n\nPlease enter a valid name.", "Validation Failed", MessageBoxButton.OK, MessageBoxImage.Information);

                ShmVarsEditor.boolHasInvalidValue = true;
                ShmVarsEditor.strNewValidValue = ShmVarsEditor.GetUniqueVarName();
                shmVar.Name = ShmVarsEditor.strNewValidValue;
                return false;
            }

            return true;
        }

        private bool checkUniqueIdentifier(string value, SharedMemoryVariable shmVar)
        {
            if (!SharedMemoryVariableList.IsUniqueIdentifier(value, shmVar))
            {
                MessageBox.Show("The entered name already exists as a shared memory variable.\n\nPlease enter a unique name.", "Validation Failed", MessageBoxButton.OK, MessageBoxImage.Information);

                ShmVarsEditor.boolHasInvalidValue = true;
                ShmVarsEditor.strNewValidValue = ShmVarsEditor.GetUniqueVarName();
                shmVar.Name = ShmVarsEditor.strNewValidValue;
                return false;
            }

            return true;
        }
    }

    //converting the bool value of IsConnected to string "Yes"
    internal class BoolToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((bool)value)
            {
                return "Yes";
            }
            else
            {
                return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

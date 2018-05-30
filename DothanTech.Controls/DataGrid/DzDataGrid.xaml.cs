using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections;
using System.Collections.ObjectModel;

namespace Dothan.Controls
{
    /// <summary>
    /// DzDataGrid.xaml 的交互逻辑
    /// </summary>
    public partial class DzDataGrid : UserControl
    {
        public DzDataGrid()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(DzDataGrid_Loaded);

        }

        void DzDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            RefDataGrid();
        }

        /// <summary>
        /// 刷新
        /// </summary>
        public void RefDataGrid()
        {
            spMain.Children.Clear();
            SetHeaders();
            SetRows();
        }

        public static DependencyProperty HasNumRowProperty =
           DependencyProperty.Register("HasNumRow", typeof(bool),
           typeof(DzDataGrid), new FrameworkPropertyMetadata(null));
        /// <summary>
        /// 是否有行标题
        /// </summary>
        public bool HasNumRow
        {
            get
            {
                return (bool)GetValue(HasNumRowProperty);
            }
            set
            {
                SetValue(HasNumRowProperty, value);
            }
        }

        public static DependencyProperty RowHeightProperty =
           DependencyProperty.Register("RowHeight", typeof(double),
           typeof(DzDataGrid), new FrameworkPropertyMetadata(null));
        /// <summary>
        /// 行高
        /// </summary>
        public bool RowHeight
        {
            get
            {
                return (bool)GetValue(RowHeightProperty);
            }
            set
            {
                SetValue(RowHeightProperty, value);
            }
        }

        public static DependencyProperty ColumnsProperty =
            DependencyProperty.Register("Columns", typeof(ObservableCollection<DzDataGridColumn>),
            typeof(DzDataGrid), new FrameworkPropertyMetadata(null));
        /// <summary>
        /// 列标题
        /// </summary>
        public ObservableCollection<DzDataGridColumn> Columns
        {
            get
            {
                return (ObservableCollection<DzDataGridColumn>)GetValue(ColumnsProperty);
            }
            set
            {
                if (value != null)
                {
                    SetValue(ColumnsProperty, value);
                }
                else
                    this.ClearValue(ColumnsProperty);
            }
        }

        public static DependencyProperty ItemsSourceProperty =
          DependencyProperty.Register("ItemsSource", typeof(IEnumerable),
          typeof(DzDataGrid), new FrameworkPropertyMetadata(null));
        /// <summary>
        /// 数据源
        /// </summary>
        public IEnumerable ItemsSource
        {
            get
            {
                return (IEnumerable)GetValue(ItemsSourceProperty);
            }
            set
            {
                if (value != null)
                {
                    SetValue(ItemsSourceProperty, value);
                }
                else
                    this.ClearValue(ItemsSourceProperty);
            }
        }

        /// <summary>
        /// 设置标题
        /// </summary>
        /// <param name="headers"></param>
        /// <param name="hasRowTitle"></param>
        private void SetHeaders()
        {
            if (Columns != null)
            {
                StackPanel spHeader = new StackPanel() { Orientation = Orientation.Horizontal };
                if (HasNumRow)
                {
                    spHeader.Children.Add(new TextBlock() { Width = 30 });

                }
                foreach (DzDataGridColumn header in Columns)
                {
                    //外层容器
                    Border container = new Border() { Width = header.Width };
                    if (header.Content != null && header.Content is Control)
                    {
                        container.Child = header.Content as Control;
                    }
                    else if (header.Content != null && header.Content is UserControl)
                    {
                        container.Child = header.Content as UserControl;
                    }
                    else if (header.Content != null && header.Content is string)
                    {
                        TextBlock tb = new TextBlock();
                        //内容填充
                        if (!string.IsNullOrEmpty(header.Content.ToString()))
                        {
                            tb.Text = header.Content.ToString();
                        }
                        container.Child = tb;
                    }
                    //设置宽度
                    if (header.Width > 0)
                    {
                        container.Width = header.Width;
                    }

                    spHeader.Children.Add(container);
                }
                spMain.Children.Add(spHeader);
            }
        }

        /// <summary>
        /// 设置行内容
        /// </summary>
        private void SetRows()
        {
            if (ItemsSource != null && Columns != null)
            {
                int rowIndex = 1;
                foreach (var row in ItemsSource)
                {
                    StackPanel spRow = new StackPanel() { Orientation = Orientation.Horizontal };
                   
                    if (HasNumRow)
                    {
                        spRow.Children.Add(new TextBlock() { Width = 30, Text = rowIndex.ToString(), HorizontalAlignment = System.Windows.HorizontalAlignment.Center });
                        rowIndex++;

                    }
                    foreach (DzDataGridColumn column in Columns)
                    {
                        
                        //外层容器
                        Border container = new Border() { Width = column.Width };
                        Binding bd = new Binding(column.BindingKey);
                        bd.Source = row;

                        if (column.ColTemplete != null && column.ColTemplete is Control)
                        {
                            Control ctl = column.ColTemplete as Control;
                            ctl.DataContext = bd;
                            container.Child = ctl;
                        }
                        else if (column.ColTemplete != null && column.ColTemplete is UserControl)
                        {
                            UserControl ctl = column.ColTemplete as UserControl;
                            ctl.DataContext = bd;
                            container.Child = ctl;
                        }
                        else
                        {
                            TextBox block = new TextBox() { BorderBrush = new SolidColorBrush(Colors.Transparent)  };
                            block.SetBinding(TextBox.TextProperty, bd);
                            container.Child = block;
                        }
                        spRow.Children.Add(container);

                    }

                    spMain.Children.Add(spRow);


                }
            }
        }

    }
}

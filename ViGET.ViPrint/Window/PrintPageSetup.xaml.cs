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
using System.Collections.ObjectModel;
using Dothan.Helpers;
using Dothan.ViObject;
using System.Diagnostics;

namespace Dothan.Print
{
    /// <summary>
    /// Interaction logic for PrintPageSetup.xaml
    /// </summary>
    public partial class PrintPageSetup : Window
    {
        #region Life Cycle

        private TextBox curTextBox;

        public PrintPageSetup()
        {
            InitializeComponent();

            //加载纸张样式列表项
            lbStyles.Items.Add(PaperFormat.A3);
            lbStyles.Items.Add(PaperFormat.A4);
            //lbStyles.Items.Add(PaperFormat.A5);

            // load data
            chkPrintColor.IsChecked = PrintPageSetup.PrintWithColor;
            if (PrintPageSetup.PrintLandscape)
                rbLandscape.IsChecked = true;
            else
                rbPortrait.IsChecked = true;

            txtHeaderLeft.Text = PrintPageSetup.HeaderLeftInfo;
            txtHeaderCenter.Text = PrintPageSetup.HeaderCenterInfo;
            txtHeaderRight.Text = PrintPageSetup.HeaderRightInfo;
            txtFooterLeft.Text = PrintPageSetup.FooterLeftInfo;
            txtFooterCenter.Text = PrintPageSetup.FooterCenterInfo;
            txtFooterRight.Text = PrintPageSetup.FooterRightInfo;

            PaperFormat paper = PrintPageSetup.PaperStyle;
            foreach (PaperFormat item in lbStyles.Items)
            {
                if (paper == item)
                {
                    lbStyles.SelectedItem = item;
                    txtCurStyle.Text = item.ToString();
                }
            }

            //初始化数据类型列表
            ShowDatas datas = this.FindResource("DataList") as ShowDatas;
            if (datas != null)
            {
                //datas = new ObservableCollection<ShowData>();
                //datas.Add(new ShowData(ShowType.TEXT));
                datas.Add(new ShowData(ShowType.PAGE));
                datas.Add(new ShowData(ShowType.PAGE_OF));
                datas.Add(new ShowData(ShowType.CUR_DATE));
                datas.Add(new ShowData(ShowType.CUR_TIME));
                datas.Add(new ShowData(ShowType.CUR_DATE_TIME));
                datas.Add(new ShowData(ShowType.FILE_PATH));
                datas.Add(new ShowData(ShowType.PROJECT_PATH));
                datas.Add(new ShowData(ShowType.DATE_PRINTED));
                datas.Add(new ShowData(ShowType.DATE_CREATED));
                datas.Add(new ShowData(ShowType.DATE_LAST_MODIFIED));
                datas.Add(new ShowData(ShowType.OBJECT));
                datas.Add(new ShowData(ShowType.OBJECT_TYPE));

                PopList.Tag = datas;
            }
        }

        #endregion

        #region Static Property

        public static bool PrintWithColor
        {
            get
            {
                RegFile regFile = ViGlobal.GlobalReg;
                return regFile.GetValue("PrintOptions", "PrintWithColors", true);
            }
            set
            {
                RegFile regFile = ViGlobal.GlobalReg;
                regFile.SetValue("PrintOptions", "PrintWithColors", value);
            }
        }

        public static bool PrintLandscape
        {
            get
            {
                RegFile regFile = ViGlobal.GlobalReg;
                return regFile.GetValue("PrintOptions", "Landscape", true);
            }
            set
            {
                RegFile regFile = ViGlobal.GlobalReg;
                regFile.SetValue("PrintOptions", "Landscape", value);
            }
        }

        public static string HeaderLeftInfo
        {
            get
            {
                RegFile regFile = ViGlobal.GlobalReg;
                return regFile.GetValue("PrintOptions", "HeaderLeft", "");
            }
            set
            {
                RegFile regFile = ViGlobal.GlobalReg;
                regFile.SetValue("PrintOptions", "HeaderLeft", value);
            }
        }

        public static string HeaderCenterInfo
        {
            get
            {
                RegFile regFile = ViGlobal.GlobalReg;
                return regFile.GetValue("PrintOptions", "HeaderCenter", ShowData.GetShowText(ShowData.STR_FILE_PATH));
            }
            set
            {
                RegFile regFile = ViGlobal.GlobalReg;
                regFile.SetValue("PrintOptions", "HeaderCenter", value);
            }
        }

        public static string HeaderRightInfo
        {
            get
            {
                RegFile regFile = ViGlobal.GlobalReg;
                return regFile.GetValue("PrintOptions", "HeaderRight", "");
            }
            set
            {
                RegFile regFile = ViGlobal.GlobalReg;
                regFile.SetValue("PrintOptions", "HeaderRight", value);
            }
        }

        public static string FooterLeftInfo
        {
            get
            {
                RegFile regFile = ViGlobal.GlobalReg;
                return regFile.GetValue("PrintOptions", "FooterLeft", ShowData.GetShowText(ShowData.STR_CUR_TIME));
            }
            set
            {
                RegFile regFile = ViGlobal.GlobalReg;
                regFile.SetValue("PrintOptions", "FooterLeft", value);
            }
        }

        public static string FooterCenterInfo
        {
            get
            {
                RegFile regFile = ViGlobal.GlobalReg;
                return regFile.GetValue("PrintOptions", "FooterCenter", "");
            }
            set
            {
                RegFile regFile = ViGlobal.GlobalReg;
                regFile.SetValue("PrintOptions", "FooterCenter", value);
            }
        }

        public static string FooterRightInfo
        {
            get
            {
                RegFile regFile = ViGlobal.GlobalReg;
                return regFile.GetValue("PrintOptions", "FooterRight", ShowData.GetShowText(ShowData.STR_PAGE + ShowData.GetShowText(ShowData.STR_PAGE_OF)));
            }
            set
            {
                RegFile regFile = ViGlobal.GlobalReg;
                regFile.SetValue("PrintOptions", "FooterRight", value);
            }
        }

        /// <summary>
        /// 纸张信息，如：A3、A4、A5等。
        /// </summary>
        public static PaperFormat PaperStyle
        {
            get
            {
                RegFile regFile = ViGlobal.GlobalReg;
                string format = regFile.GetValue("PrintOptions", "PaperStyle", PaperFormat.A4.ToString());
                return (PaperFormat)Enum.Parse(typeof(PaperFormat), format);
            }
            set
            {
                RegFile regFile = ViGlobal.GlobalReg;
                regFile.SetValue("PrintOptions", "PaperStyle", value.ToString());
            }
        }

        #endregion

        public bool PrintColor
        {
            get
            {
                try
                {
                    if (null != chkPrintColor && (bool)chkPrintColor.IsChecked)
                        return true;
                    else
                        return false;

                }
                catch (System.Exception ee)
                {
                    Trace.WriteLine("### [" + ee.Source + "] Exception: " + ee.Message);
                    Trace.WriteLine("### " + ee.StackTrace);
                    return false;
                }
            }
            set
            {
                if (null != chkPrintColor)
                    chkPrintColor.IsChecked = value;
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            // save data
            PrintPageSetup.PrintWithColor = (bool)chkPrintColor.IsChecked;
            PrintPageSetup.PrintLandscape = (bool)rbLandscape.IsChecked;
            if (null != lbStyles.SelectedItem)
                PrintPageSetup.PaperStyle = (PaperFormat)lbStyles.SelectedItem;

            PrintPageSetup.HeaderLeftInfo = txtHeaderLeft.Text;
            PrintPageSetup.HeaderCenterInfo = txtHeaderCenter.Text;
            PrintPageSetup.HeaderRightInfo = txtHeaderRight.Text;
            PrintPageSetup.FooterLeftInfo = txtFooterLeft.Text;
            PrintPageSetup.FooterCenterInfo = txtFooterCenter.Text;
            PrintPageSetup.FooterRightInfo = txtFooterRight.Text;

            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void PopList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (curTextBox != null)
            {
                ShowData data = PopList.SelectedValue as ShowData;
                if (null != data)
                {
                    int index = curTextBox.SelectionStart;
                    string strHead = curTextBox.Text.Substring(0, index);
                    string strTrail = curTextBox.Text.Substring(index);

                    curTextBox.Text = strHead + data.ShowText + strTrail;
                    curTextBox.SelectionStart = strHead.Length + data.ShowText.Length;
                }
            }
            Pop.IsOpen = false;
            PopList.UnselectAll();
            curTextBox.Focus();
        }

        private void lbStyles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }


        /// <summary>
        /// Set the default print page data
        /// </summary>
        /// <param name="datas"></param>
        private void SetDefaultPageData(ShowDatas datas)
        {
            if (null == datas)
            {
                return;
            }
            txtHeaderLeft.Text = "";
            txtHeaderCenter.Text = ShowData.GetShowText(ShowData.STR_FILE_PATH);
            txtHeaderRight.Text = "";
            txtFooterLeft.Text = ShowData.GetShowText(ShowData.STR_DATE_PRINTER);
            txtFooterCenter.Text = "";
            txtFooterRight.Text = ShowData.GetShowText(ShowData.STR_PAGE)
                               + ShowData.GetShowText(ShowData.STR_PAGE_OF);
        }

        /// <summary>
        /// Event response function for the DefaultButton
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSetDefault_Click(object sender, RoutedEventArgs e)
        {
            ShowDatas datas = PopList.Tag as ShowDatas;
            if (null != datas)
            {
                SetDefaultPageData(datas);
            }
        }

        #region Get current TextBox by the clicked Button
        private void btnHeaderLeft_Click(object sender, RoutedEventArgs e)
        {
            curTextBox = txtHeaderLeft;
            Pop.PlacementTarget = btnHeaderLeft;
            Pop.IsOpen = true;
        }

        private void btnHeaderCenter_Click(object sender, RoutedEventArgs e)
        {
            curTextBox = txtHeaderCenter;
            Pop.PlacementTarget = btnHeaderCenter;
            Pop.IsOpen = true;
        }

        private void btnHeaderRight_Click(object sender, RoutedEventArgs e)
        {
            curTextBox = txtHeaderRight;
            Pop.PlacementTarget = btnHeaderRight;
            Pop.IsOpen = true;
        }

        private void btnFooterLeft_Click(object sender, RoutedEventArgs e)
        {
            curTextBox = txtFooterLeft;
            Pop.PlacementTarget = btnFooterLeft;
            Pop.IsOpen = true;
        }

        private void btnFooterCenter_Click(object sender, RoutedEventArgs e)
        {
            curTextBox = txtFooterCenter;
            Pop.PlacementTarget = btnFooterCenter;
            Pop.IsOpen = true;
        }

        private void btnFooterRight_Click(object sender, RoutedEventArgs e)
        {
            curTextBox = txtFooterRight;
            Pop.PlacementTarget = btnFooterRight;
            Pop.IsOpen = true;
        }

        #endregion

    }

    public enum ShowType
    {
        PAGE,
        PAGE_OF,
        CUR_DATE,
        CUR_TIME,
        CUR_DATE_TIME,
        FILE_PATH,
        PROJECT_PATH,
        DATE_PRINTED,
        DATE_CREATED,
        DATE_LAST_MODIFIED,
        OBJECT,
        OBJECT_TYPE
    }


    /// <summary>
    /// 打印时在指定位置所展示的数据
    /// </summary>
    public class ShowData
    {
        public const string STR_CUR_DATE = "Date";
        public const string STR_CUR_TIME = "Time";
        public const string STR_CUR_DATE_TIME = "DateTime";
        public const string STR_PAGE = "Page";
        public const string STR_PAGE_OF = "Page Of";
        public const string STR_FILE_PATH = "File Path";
        public const string STR_PROJECT_PATH = "Project Path";
        public const string STR_DATE_CREATED = "Date Created";
        public const string STR_DATE_PRINTER = "Date Printed";
        public const string STR_DATE_LAST_MODIFIED = "Date Last Modified";
        public const string STR_OBJECT = "Object";
        public const string STR_OBJECT_TYPE = "Object Type";

        public ShowData() { }
        public ShowData(ShowType type)
        {
            this.Type = type;
        }

        private ShowType _Type;
        public ShowType Type
        {
            get { return _Type; }
            set { this._Type = value; }
        }

        private string _Key;
        public string Key
        {
            get
            {
                if (string.IsNullOrEmpty(_Key))
                {
                    this._Key = GetKey(this.Type);
                }
                return _Key;
            }
        }

        private string _ShowText;
        public string ShowText
        {
            get
            {
                if (string.IsNullOrEmpty(_ShowText))
                {
                    _ShowText = GetShowText(this.Key);
                }

                return _ShowText;
            }
        }

        /// <summary>
        /// 根据枚举类型返回关键字
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetKey(ShowType type)
        {
            string str = "";
            switch (type)
            {
                case ShowType.CUR_DATE:
                    str = STR_CUR_DATE;
                    break;
                case ShowType.CUR_TIME:
                    str = STR_CUR_TIME;
                    break;
                case ShowType.CUR_DATE_TIME:
                    str = STR_CUR_DATE_TIME;
                    break;
                case ShowType.PAGE:
                    str = STR_PAGE;
                    break;
                case ShowType.PAGE_OF:
                    str = STR_PAGE_OF;
                    break;
                case ShowType.FILE_PATH:
                    str = STR_FILE_PATH;
                    break;
                case ShowType.PROJECT_PATH:
                    str = STR_PROJECT_PATH;
                    break;
                case ShowType.DATE_PRINTED:
                    str = STR_DATE_PRINTER;
                    break;
                case ShowType.DATE_CREATED:
                    str = STR_DATE_CREATED;
                    break;
                case ShowType.DATE_LAST_MODIFIED:
                    str = STR_DATE_LAST_MODIFIED;
                    break;
                case ShowType.OBJECT:
                    str = STR_OBJECT;
                    break;
                case ShowType.OBJECT_TYPE:
                    str = STR_OBJECT_TYPE;
                    break;
                default:
                    break;
            }
            return str;
        }

        /// <summary>
        /// 根据关键字获取显示文本信息
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetShowText(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return "";
            }
            string str = "";
            switch (key)
            {
                case STR_PAGE:
                    str = "Page {" + STR_PAGE + "} ";
                    break;
                case STR_PAGE_OF:
                    str = "of {" + STR_PAGE_OF + "} ";
                    break;
                case STR_CUR_DATE:
                    str = "{" + STR_CUR_DATE + "} ";
                    break;
                case STR_CUR_TIME:
                    str = "{" + STR_CUR_TIME + "} ";
                    break;
                case STR_CUR_DATE_TIME:
                    str = "{" + STR_CUR_DATE_TIME + "} ";
                    break;
                case STR_FILE_PATH:
                    str = "File: {" + STR_FILE_PATH + "} ";
                    break;
                case STR_PROJECT_PATH:
                    str = "Project: {" + STR_PROJECT_PATH + "} ";
                    break;
                case STR_DATE_PRINTER:
                    str = "Print Time: {" + STR_DATE_PRINTER + "} ";
                    break;
                case STR_DATE_CREATED:
                    str = "Create Time: {" + STR_DATE_CREATED + "} ";
                    break;
                case STR_DATE_LAST_MODIFIED:
                    str = "Modify Time: {" + STR_DATE_LAST_MODIFIED + "} ";
                    break;
                case STR_OBJECT:
                    str = "{" + STR_OBJECT + "}";
                    break;
                case STR_OBJECT_TYPE:
                    str = "{" + STR_OBJECT_TYPE + "}";
                    break;
                default:
                    break;
            }
            return str;
        }

        /// <summary>
        /// check the whether the key is valid
        /// </summary>
        /// <param name="key">the test key</param>
        /// <returns>true: the key is valid and can decode, else can't decode</returns>
        public static bool CheckKey(string key)
        {
            bool res = false;
            switch (key)
            {
                case STR_PAGE:
                case STR_PAGE_OF:
                case STR_CUR_DATE:
                case STR_CUR_TIME:
                case STR_CUR_DATE_TIME:
                case STR_FILE_PATH:
                case STR_PROJECT_PATH:
                case STR_DATE_PRINTER:
                case STR_DATE_CREATED:
                case STR_DATE_LAST_MODIFIED:
                case STR_OBJECT:
                case STR_OBJECT_TYPE:
                    res = true;
                    break;
                default:
                    res = false;
                    break;
            }

            return res;
        }

        public static Dictionary<string, string> GetKeysByRegData(string regData)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            string[] temps = regData.Split('{');
            if (temps.Length > 1)
            {
                for (int i = 1; i < temps.Length; i++)
                {
                    int endIndex = temps[i].IndexOf('}');
                    if (endIndex > 0)
                    {
                        string key = temps[i].Substring(0, endIndex).Trim();
                        if (!string.IsNullOrEmpty(key))
                        {
                            if (ShowData.CheckKey(key))
                            {
                                dictionary[key] = "{" + key + "}";
                            }
                        }
                    }
                }
            }

            return dictionary;
        }
    }

    public class ShowDatas : ObservableCollection<ShowData>
    {

    }
}

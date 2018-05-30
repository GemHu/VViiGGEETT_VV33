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
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout;

using DothanTech.ViGET.ViCommand;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using Prism.Commands;
using DothanTech.ViGET.ViService;
using Xceed.Wpf.AvalonDock.Themes;
using Xceed.Wpf.AvalonDock.Layout.Serialization;
using System.IO;
using MahApps.Metro.Controls;

namespace DothanTech.ViGET.Shell
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [Export]
    public partial class ViShell : MetroWindow, IPartImportsSatisfiedNotification
    {
        public ViShell()
        {
            InitializeComponent();
            this.InitCommand();

            this.DataContext = this;
            this.Closing += this.OnShellClosing;
            //
            //this.LoadLayout();
            this.ChangeAppStyle(null, null);
        }

        #region Command

        #region Show DockPane Command

        #region Show SolutionExplorer Pane

        public ICommand ShowSolutionExplorer
        {
            get
            {
                if (this._ShowSolutionExplorer == null)
                {
                    this._ShowSolutionExplorer = new DelegateCommand(() =>
                    {
                        if (anchorSolutionExplorer.Parent == null)
                            paneRight.Children.Add(anchorSolutionExplorer);
                        else if (anchorSolutionExplorer.IsHidden)
                            anchorSolutionExplorer.Show();
                        else
                            anchorSolutionExplorer.IsActive = true;
                    }, () => { return true; });
                }
                return this._ShowSolutionExplorer;
            }
        }
        private ICommand _ShowSolutionExplorer;

        #endregion

        #region Show StartPage Pane

        public ICommand ShowStartPage
        {
            get
            {
                if (this._ShowStartPage == null)
                {
                    this._ShowStartPage = new DelegateCommand(() =>
                    {
                        this.DocManager.ShowStartPage();
                    }, () =>
                    {
                        return this.DocManager != null;
                    });
                }
                return _ShowStartPage;
            }
        }
        private ICommand _ShowStartPage;

        #endregion

        #region Show Hardware Library Pane

        public ICommand ShowHardwareLibrary
        {
            get
            {
                if (this._ShowHardwareLibrary == null)
                {
                    this._ShowHardwareLibrary = new DelegateCommand(() =>
                    {
                        if (this.anchorHardwareLibrary.Parent == null)
                            this.paneLeft.Children.Add(this.anchorHardwareLibrary);
                        else if (this.anchorHardwareLibrary.IsHidden)
                            this.anchorHardwareLibrary.Show();
                        else
                            this.anchorHardwareLibrary.IsActive = true;
                    });
                }
                return _ShowHardwareLibrary;
            }
        }
        private ICommand _ShowHardwareLibrary;

        #endregion

        #region Show POU Library Pane

        public ICommand ShowPOULibrary
        {
            get
            {
                if (this._ShowPOULibrary == null)
                {
                    this._ShowPOULibrary = new DelegateCommand(() =>
                    {
                        if (this.anchorPOUs.Parent == null)
                            this.paneLeft.Children.Add(this.anchorPOUs);
                        else if (this.anchorPOUs.IsHidden)
                            this.anchorPOUs.Show();
                        else
                            this.anchorPOUs.IsActive = true;
                    });
                }
                return _ShowPOULibrary;
            }
        }
        private ICommand _ShowPOULibrary;

        #endregion

        #region Show Error List Pane

        public ICommand ShowErrorList
        {
            get
            {
                if (this._ShowErrorList == null)
                {
                    this._ShowErrorList = new DelegateCommand(() =>
                    {
                        if (this.anchorErrorList.Parent == null)
                            this.paneBottom.Children.Add(this.anchorErrorList);
                        else if (this.anchorErrorList.IsHidden)
                            this.anchorErrorList.Show();
                        else
                            this.anchorErrorList.IsActive = true;
                    });
                }
                return _ShowErrorList;
            }
        }
        private ICommand _ShowErrorList;

        #endregion

        #region Show Output Pane

        [Import]
        private UcOutputPanel OutputPanel;
        //<!--<avalon:LayoutAnchorable x:Name="anchorOutput" Title="Output" ContentId="Output"></avalon:LayoutAnchorable>-->
        public ICommand ShowOutput
        {
            get
            {
                if (this._ShowOutput == null)
                {
                    this._ShowOutput = new DelegateCommand(() =>
                    {
                        if (this.OutputPanel == null)
                            return;

                        if (this.anchorOutput == null)
                        {
                            this.anchorOutput = new LayoutAnchorable();
                            this.anchorOutput.Title = "Output";
                            this.anchorOutput.ContentId = "Output";
                            this.anchorOutput.Content = this.OutputPanel;
                        }

                        if (this.anchorOutput.Parent == null)
                            this.paneBottom.Children.Add(this.anchorOutput);
                        else if (this.anchorOutput.IsHidden)
                            this.anchorOutput.Show();
                        else
                            this.anchorOutput.IsActive = true;
                    });
                }
                return _ShowOutput;
            }
        }
        private ICommand _ShowOutput;
        private LayoutAnchorable anchorOutput;

        #endregion

        #endregion

        #endregion

        #region 主题切换

        #region Theme Property

        //public static readonly DependencyProperty ThemeProperty =
        //    DependencyProperty.Register("Theme", typeof(Theme), typeof(ViShell),
        //                                new PropertyMetadata(null));

        ///// <summary>
        ///// AvalanDock主题；
        ///// 
        ///// 常用主题有：
        ///// AeroTheme();
        ///// GenericTheme();
        ///// MetroTheme();
        ///// VS2010Theme();
        ///// </summary>
        //public Theme Theme
        //{
        //    get { return (Theme)GetValue(ThemeProperty); }
        //    set { SetValue(ThemeProperty, value); }
        //}

        #endregion


        #endregion

        #region 布局效果的保存于恢复

        // TODO: 当LoadLayout后，再去打开文档后，不知道为什么不显示，所以暂时先不用该功能；
        //protected void SaveLayout()
        //{
        //    String file = "Layout.xml";
        //    XmlLayoutSerializer serializer = new XmlLayoutSerializer(this.dockManager);
        //    using (var stream = new StreamWriter(file))
        //    {
        //        serializer.Serialize(stream);
        //    }
        //}

        //protected void LoadLayout()
        //{
        //    String file = "Layout.xml";
        //    if (File.Exists(file))
        //    {
        //        XmlLayoutSerializer serializer = new XmlLayoutSerializer(this.dockManager);
        //        using (var stream = new StreamReader(file))
        //        {
        //            serializer.Deserialize(stream);
        //        }
        //    }
        //}

        #endregion

        public void OnImportsSatisfied()
        {
            this.ShowStartPage.Execute(this);
            this.ShowOutput.Execute(this);
        }
    }
}

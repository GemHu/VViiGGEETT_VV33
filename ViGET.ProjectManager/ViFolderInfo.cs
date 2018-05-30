using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dothan.ViObject;
using System.Collections.Specialized;
using System.Xml;
using System.IO;
using System.Diagnostics;
using Dothan.Helpers;
using DothanTech.ViGET.ViCommand;
using DothanTech.ViGET.ViService;
using Microsoft.Win32;

namespace DothanTech.ViGET.Manager
{
    public interface IViFolderInfo : IViFileInfo
    {

    }

    /// <summary>
    /// TreeNode相关基类，从该类集成过来的子类通常会在SolutionExplorer中显示对应的节点，并且该节点下面还会有其他的子节点；
    /// </summary>
    public class ViFolderInfo : ViFileInfo, IViFolderInfo
    {
        #region life cycle

        public ViFolderInfo(String file)
            : base(file)
        {
            this.Name = Path.GetFileNameWithoutExtension(file);
            this.Children = new ViObservableCollection<ViFileNode>();
        }

        public ViFolderInfo(PCSFile file)
            : this(file.File)
        {
            this.CurrFile = file;
        }

        #endregion

        #region File & Path

        /// <summary>
        /// 获取给定的文件相对于当前文件夹的相对路径；
        /// </summary>
        /// <param name="file">目标文件</param>
        /// <returns>相对路径</returns>
        public String GetRelativePath(String file)
        {
            if (String.IsNullOrEmpty(file) || String.IsNullOrEmpty(this.DirectoryPath))
                return file;
            if (!file.StartsWith(this.DirectoryPath))
                return file;

            return file.Substring(this.DirectoryPath.Length).Trim(Path.DirectorySeparatorChar);
        }

        #endregion

        #region 数据持久化

        // 由于单纯的某个文件数据的读取与保存一般都是有对应的编辑器进行管理的，而这里所要保存或者读取的相关信息都属于配置信息，
        // 所以现在只需要把数据持久化相关的代码放到 FolderNode中来；

        public bool IsDirty { get; set; }

        /// <summary>
        /// 用于判断是否增在加载文件；
        /// </summary>
        public bool IsLoading { get; protected set; }
        /// <summary>
        /// 用于判断是否增在保存文件操作；
        /// </summary>
        public bool IsSaveing { get; protected set; }
        
        /// <summary>
        /// 加载本地数据；
        /// </summary>
        public virtual bool Load()
        {
            return this.LoadFile(this.FullPath);
        }

        /// <summary>
        /// 从制定的文件中加载配置信息；
        /// </summary>
        public virtual bool LoadFile(String file)
        {
            if (!System.IO.File.Exists(this.FullPath))
                return false;

            try
            {
                XmlDocument doc = new XmlDocument();
                this.IsLoading = true;
                doc.Load(this.FullPath);
                if (!this.LoadDocument(doc))
                    return false;

                this.IsDirty = false;
                return true;
            }
            catch (Exception ee)
            {
                Trace.WriteLine("### [" + ee.Source + "] Exception: " + ee.Message);
                Trace.WriteLine("### " + ee.StackTrace);
                return false;
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        /// <summary>
        /// 从制定的XML文件中读取相关配置信息；
        /// </summary>
        protected virtual bool LoadDocument(XmlDocument doc)
        {
            return true;
        }

        public virtual bool LoadElement(XmlElement element)
        {
            return true;
        }
        
        /// <summary>
        /// 保存本地数据；
        /// </summary>
        public virtual bool Save()
        {
            return this.SaveFile(this.FullPath);
        }

        /// <summary>
        /// 保存相关数据到制定的文件；
        /// </summary>
        public virtual bool SaveFile(String file)
        {
            if (!File.Exists(this.FullPath))
                return false;
            if (!this.IsDirty)
                return true;

            try
            {
                XmlDocument doc = new XmlDocument();
                this.IsSaveing = true;
                if (!this.SaveDocument(doc))
                    return false;

                doc.Save(this.FullPath);
                this.IsDirty = false;
                return true;
            }
            catch (Exception ee)
            {
                Trace.WriteLine("### [" + ee.Source + "] Exception: " + ee.Message);
                Trace.WriteLine("### " + ee.StackTrace);

                return false;
            }
            finally
            {
                this.IsSaveing = false;
            }
        }

        /// <summary>
        /// 保存相关数据到指定的XML文件中；
        /// </summary>
        protected virtual bool SaveDocument(XmlDocument doc)
        {
            return true;
        }

        /// <summary>
        /// 保存相关配置信息到给定的XmlElement;
        /// </summary>
        public virtual bool SaveElement(XmlElement parent, XmlDocument doc)
        {
            return true;
        }

        #endregion

        #region Command Handle

        public override void CanExecuteCommand(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            if (e.Command == ViCommands.OpenLocalFolder ||
                e.Command == ViCommands.AddNewItem)
            {
                e.CanExecute = true;
                return;
            }
            else if (e.Command == ViCommands.Build ||
                e.Command == ViCommands.Rebuild ||
                e.Command == ViCommands.Clean)
            {
                e.CanExecute = this.CanBuild();
            }


            base.CanExecuteCommand(sender, e);
        }

        public override void ExecutedCommand(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            if (e.Command == ViCommands.OpenLocalFolder)
            {
                System.Diagnostics.Process.Start(this.FullPath);
                return;
            }
            else if (e.Command == ViCommands.AddNewItem)
            {
                this.AddNewItem();
            }
            else if (e.Command == ViCommands.Build)
            {
                this.Build(false);
            }
            else if (e.Command == ViCommands.Rebuild)
            {
                this.Build(true);
            }
            else if (e.Command == ViCommands.Clean)
            {
                this.BuildClean();
            }

            base.ExecutedCommand(sender, e);
        }

        #endregion

        /// <summary>
        /// 添加新项；
        /// </summary>
        protected void AddNewItem()
        {
            if (this.TheFactory == null || this.TheFactory.TempWizard == null)
                return;

            this.TheFactory.TempWizard.RunCreateFileWizard(this.FullPath, this.Type, (file, type) =>
            {
                ViFileInfo child = ViFileInfo.CreateFile(type, file);
                this.AddChild(child);
            });
        }

        /// <summary>
        /// 添加本地已存在的项；
        /// </summary>
        protected void AddExistingItem()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.DefaultExt = this.GetDefaultExt();
            dialog.Filter = this.GetFileFilter();
            if (dialog.ShowDialog() == true)
            {
                this.AddExistingItem(dialog.FileName, String.Empty);
            }
        }

        protected void AddExistingItem(String file, String fileType)
        {

        }

        protected virtual String GetDefaultExt()
        {
            return ".xcfc";
        }

        protected virtual String GetFileFilter()
        {
            return "All Files(*.*)|*.*" +
                    "|CFC File(*.xcfc)|*.xcfc" +
                    "|MAK File(*.mak)|*.mak";
        }

        #region Build Manager

        protected bool CanBuild()
        {
            if (this.TheFactory == null || this.TheFactory.BuildManager == null)
                return false;

            return (this is IViBuildGroup) || (this is IViBuildItem);
        }

        protected void Build(bool rebuild)
        {
            if (this is IViBuildGroup)
                this.TheFactory.BuildManager.Build(this as IViBuildGroup, rebuild);
            else if (this is IViBuildItem)
                this.TheFactory.BuildManager.Build(this as IViBuildItem, rebuild);
        }

        protected void BuildClean()
        {
            if (this is IViBuildGroup)
                this.TheFactory.BuildManager.Clean(this as IViBuildGroup);
            else if (this is IViBuildItem)
                this.TheFactory.BuildManager.Clean(this as IViBuildItem);
        }

        #endregion
    }
    public class ViFolderInfo<PT> : ViFolderInfo where PT : ViFolderInfo
    {
        #region Life Cycle

        public ViFolderInfo(PCSFile file)
            : base(file)
        {

        }

        public ViFolderInfo(String file)
            : base(file)
        {

        }

        #endregion

        #region Parent

        /// <summary>
        /// 得到父对象。
        /// </summary>
        /// <returns>父对象</returns>
        public new PT GetParent()
        {
            return this.Parent;
        }

        /// <summary>
        /// 得到父对象。
        /// </summary>
        public new PT Parent
        {
            get
            {
                return base.GetParent() as PT;
            }
            set
            {
                base.SetParent(value);
            }
        }

        #endregion
    }

}

using Dothan.ViObject;
using DothanTech.ViGET.ViService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DothanTech.ViGET.Manager
{
    public interface IViModel
    {
        /// <summary>
        /// 当前模块名称；
        /// </summary>
        String Name { get; }

        /// <summary>
        /// 当前模块所属类型；
        /// </summary>
        String Type { get; }

        /// <summary>
        /// 文件相对路径；
        /// </summary>
        String Include { get; }
    }

    public interface IViFileInfo
    {
        /// <summary>
        /// 本地文件全路径；
        /// </summary>
        String FullPath { get; }

        /// <summary>
        /// 文件所在的文件夹路径；
        /// </summary>
        String DirectoryPath { get; }

        /// <summary>
        /// 通过递归查找方式，获取当前文件的相对路径；该路径是相对于Project的一个路径；
        /// </summary>
        String ViPath { get; }

        /// <summary>
        /// 当前节点所属项目；
        /// </summary>
        ProjectManager CurrProject { get; }

        /// <summary>
        /// 当前节点所属Solution；
        /// </summary>
        SolutionManager TheSolution { get; }

        /// <summary>
        /// 当前项目所属根节点；
        /// </summary>
        IViProjectFactory TheFactory { get; }

        String Key { get; }

        PCSFile CurrFile { get; }
    }

    public class ViFileInfo : ViFileNode, IViFileInfo
    {
        public static class ViFileType
        {
            public const String CFCFile = "CFC";
        }

        #region Life Cycle

        public ViFileInfo(String file) : base(file)
        {
            // 先判断下给定的文件名称是不是文件全路径，如果不是，就需要加上项目路径；
        }

        #endregion
    
        #region IViNode & File & Path

        public string ViPath
        {
            get
            {
                ViFolderInfo parent = this.GetParent() as ViFolderInfo;
                if (parent == null)
                    return Path.DirectorySeparatorChar + this.Name;

                return Path.Combine(parent.ViPath, this.Name);
            }
        }

        public virtual ProjectManager CurrProject
        {
            get { return this.GetAncestor(typeof(ProjectManager)) as ProjectManager; }
        }

        public virtual SolutionManager TheSolution
        {
            get { return this.GetAncestor(typeof(SolutionManager)) as SolutionManager; }
        }

        public IViProjectFactory TheFactory
        {
            get { return this.GetAncestor(typeof(IViProjectFactory)) as IViProjectFactory; }
        }

        public override String Key
        {
            get 
            {
                if (!String.IsNullOrEmpty(this.FullPath))
                    return PCSFile.GetFileKey(this.FullPath);

                return base.Key;
            }
        }

        public PCSFile CurrFile { get; protected set; }

        public new String Type
        {
            get { return base.Type as String; }
            set { base.Type = value; }
        }

        #endregion

        #region 相关静态方法

        public static ViFileInfo CreateFile(String fileType, String filePath)
        {
            if (ViFileType.CFCFile.Equals(fileType, StringComparison.OrdinalIgnoreCase))
                return new ViCFCFile(filePath);

            return null;
        }

        #endregion

        #region 相关事件及处理函数

        public virtual void DoubleClick(MouseButtonEventArgs e)
        {
        }

        public virtual void CanExecuteCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        public virtual void ExecutedCommand(object sender, ExecutedRoutedEventArgs e)
        {

        }

        #endregion

        #region D Linked Property

        public static readonly DependencyProperty LinkedProperty =
            DependencyProperty.Register("Linked", typeof(bool), typeof(ViFileInfo),
                                        new PropertyMetadata(true));

        public bool Linked
        {
            get { return (bool)GetValue(LinkedProperty); }
            set { SetValue(LinkedProperty, value); }
        }

        #endregion

        #region File & Children

        /// <summary>
        /// 查找制定名称文件；
        /// </summary>
        /// <param name="name">需要查找的目标文件名称</param>
        /// <returns></returns>
        public virtual ViFileInfo GetFileInfo(String name, ViFileInfo exept = null)
        {
            if (String.IsNullOrEmpty(name) || exept == this)
                return null;

            if (Path.GetFileNameWithoutExtension(name).Equals(Path.GetFileNameWithoutExtension(this.Name), StringComparison.OrdinalIgnoreCase))
                return this;

            foreach (var item in this.Children)
            {
                var target = item is ViFileInfo ? (item as ViFileInfo).GetFileInfo(name) : null;
                if (target != null)
                    return target;
            }

            return null;
        }

        #endregion
    }

    public class ViFileInfo<PT> : ViFileInfo where PT : ViFolderInfo
    {
        #region Life Cycle

        public ViFileInfo(String file)
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

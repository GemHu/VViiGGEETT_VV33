using Dothan.ViObject;
using DothanTech.ViGET.ViService;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml;

namespace DothanTech.ViGET.Manager
{
    public class ViFileNode : ViNamedObject
    {
        #region Life Cycle

        public ViFileNode(String fullPath)
        {
            this.FullPath = fullPath;
            this.Children = new ViObservableCollection<ViFileNode>();
        }

        #endregion

        #region D FullPath Property

        public static readonly DependencyProperty FullPathProperty =
            DependencyProperty.Register("FullPath", typeof(String), typeof(ViFileNode),
                                        new PropertyMetadata(String.Empty));

        /// <summary>
        /// 本地文件全路径；
        /// </summary>
        public String FullPath
        {
            get { return (String)GetValue(FullPathProperty); }
            set 
            { 
                SetValue(FullPathProperty, value);
                this.Name = Path.GetFileName(value);
            }
        }

        #endregion

        #region DirectoryPath

        /// <summary>
        /// 当前文件所在文件夹路径全路径；
        /// </summary>
        public String DirectoryPath
        {
            get { return Path.GetDirectoryName(this.FullPath); }
        }

        #endregion

        #region Children

        public ViObservableCollection<ViFileNode> Children { get; protected set; }

        public override event ViDataChangedEventHandler PropertyChanged
        {
            add
            {
                base.PropertyChanged += value;
                this.Children.PropertyChanged += value;
            }
            remove
            {
                this.Children.PropertyChanged -= value;
                base.PropertyChanged -= value;
            }
        }

        /// <summary>
        /// 遍历制定型号的子类型；
        /// </summary>
        /// <typeparam name="CT"></typeparam>
        /// <param name="func"></param>
        public void LoopChild<CT>(Func<CT, bool> func) where CT : ViNamedObject
        {
            foreach (ViNamedObject item in this.Children)
            {
                if (item is CT)
                {
                    if (!func(item as CT))
                        break;
                }
            }
        }

        public ViNamedObject GetChild(String key)
        {
            if (String.IsNullOrEmpty(key))
                return null;

            foreach (var item in this.Children)
            {
                if (item is IViFileInfo)
                {
                    IViFileInfo subNode = item as IViFileInfo;
                    if (key.Equals(subNode.Key, StringComparison.OrdinalIgnoreCase))
                        return item;
                }
            }

            return null;
        }
        public virtual void AddChild(ViFileNode child)
        {
            if (child == null)
                return;

            this.Children.Add(child);
            child.SetParent(this);
        }
        public bool RemoveChild(ViFileNode child)
        {
            if (child == null)
                return false;

            if (child.GetParent() == this)
            {
                child.SetParent(null);
                this.Children.Remove(child);

                return true;
            }

            return false;
        }

        #endregion
    }
}

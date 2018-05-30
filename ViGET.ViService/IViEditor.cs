using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace DothanTech.ViGET.ViService
{
    public interface IViEditor
    {
        /// <summary>
        /// 编辑器视图对象；
        /// </summary>
        UserControl Control { get; }

        String FileName { get; }

        /// <summary>
        /// 家在相关数据；
        /// </summary>
        /// <returns></returns>
        bool Load(String file);

        /// <summary>
        /// 保存相关数据；
        /// </summary>
        /// <returns></returns>
        bool Save(String file);

        /// <summary>
        /// 关闭当前编辑器命令；
        /// </summary>
        ICommand CloseCommand {get;}
    }

    public abstract class ViEditor<TUIControl> : IViEditor
        where TUIControl : UserControl, new()
    {
        public ViEditor()
        {
            // where T : new(), 约束T必须有无惨构造函数，否则无法实例化控件；
            this.UIControl = new TUIControl();
        }

        #region IViEditor

        public string FileName { get; protected set; }

        public UserControl Control
        {
            get { return this.UIControl; }
        }

        public bool Load(string file)
        {
            // 目标文件和已有文件均不能为空；
            if (String.IsNullOrEmpty(file) && String.IsNullOrEmpty(this.FileName))
                throw new ArgumentNullException("file");

            this._Loading = true;
            try
            {
                // 如果给定的文件不为空，则加载目标文件
                if (!String.IsNullOrEmpty(file))
                    this.FileName = file;

                // --- Show the wait cursor while loading the file
                //VsUIShell.SetWaitCursor();

                // --- Load the file
                _IsDirty = !LoadFile(this.FileName);

                // --- Notify the load or reload
                NotifyDocChanged();
                return true;
            }
            catch (Exception e)
            {
                Trace.WriteLine("### " + e.ToString());
                Trace.WriteLine("### " + e.StackTrace);
                return false;
            }
            finally
            {
                this._Loading = false;
            }
        }

        public bool Save(String file)
        {
            if (String.IsNullOrEmpty(file) && String.IsNullOrEmpty(this.FileName))
                throw new ArgumentException("file is not exist!");

            try
            {
                if (String.IsNullOrEmpty(file) || String.Compare(file, this.FileName, true) == 0)
                {
                    _IsDirty = !SaveFile(this.FileName);
                }
                else
                {
                    // 另存为
                    this.SaveFile(file);
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine("### " + e.ToString());
                Trace.WriteLine("### " + e.StackTrace);
                return false;
            }

            return true;
        }

        #endregion

        public TUIControl UIControl { get; protected set; }

        private bool _IsDirty;

        private bool _Loading;

        protected abstract bool LoadFile(string fileName);
        protected abstract bool SaveFile(string fileName);

        // --------------------------------------------------------------------------------
        /// <summary>
        /// Gets an instance of the RunningDocumentTable (RDT) service which manages the 
        /// set of currently open documents in the environment and then notifies the 
        /// client that an open document has changed.
        /// 通知文档管理器，当前文档状态发生了变化；
        /// </summary>
        // --------------------------------------------------------------------------------
        private void NotifyDocChanged()
        {

        }


        public ICommand CloseCommand
        {
            get { return null; }
        }
    }
}

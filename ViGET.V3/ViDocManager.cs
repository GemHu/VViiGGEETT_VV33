using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DothanTech.ViGET.ViService;
using System.IO;
using Xceed.Wpf.AvalonDock.Layout;
using System.ComponentModel.Composition;
using System.Collections.Specialized;
using System.Windows.Threading;
using DothanTech.ViGET.Shell.ViewModels;
using System.Collections.ObjectModel;

namespace DothanTech.ViGET.Shell
{
    /// <summary>
    /// 多文档管理工具；
    /// 
    /// 当需要打开一个文件的时候，需要执行以下步骤来创建或者获取一个已有的编辑器对象；
    /// 1、根据后缀名，找到对应的Factory；
    /// 2、判断Factory是否存在，如果不存在则实例化；
    /// 3、根据文件全路径搜索检查文档是否存在，如果存在，则设置为Active；
    /// 4、如果目标文件不存在，则创建目标文档，并将文档添加到文档列表中；
    /// </summary>
    [Export(typeof(IViDocManager))]
    public class ViDocManager : IViDocManager
    {
        public ViDocManager()
        {

        }

        [Import]
        private UcStartPage startPage;

        public ObservableCollection<DocumentViewModel> Documents
        {
            get
            {
                if (this._documents == null)
                    this._documents = new ObservableCollection<DocumentViewModel>();

                return _documents;
            }
        }
        private ObservableCollection<DocumentViewModel> _documents;

        /// <summary>
        /// 编辑器工厂字典，通过后缀名找到EditorFactory，然后通过EditorFactory来创建对应的编辑器实例；
        /// </summary>
        private Dictionary<String, IViEditorFactory> DEditorFactorys = new Dictionary<string, IViEditorFactory>();

        /// <summary>
        /// 根据扩展名获取对应的 EditorFactory；
        /// </summary>
        private IViEditorFactory GetEditorFactory(String extention)
        {
            extention = Path.GetExtension(extention);
            if (String.IsNullOrEmpty(extention))
                return null;

            String key = extention.ToUpper();
            // 如果已经有，则直接返回；
            IViEditorFactory factory = this.DEditorFactorys.ContainsKey(key) ? this.DEditorFactorys[key] : null;
            if (factory != null)
                return factory;

            factory = ViShellManager.GetEditorFactory(key);
            if (factory != null)
                this.DEditorFactorys[key] = factory;

            return factory;
        }

        public void ShowStartPage()
        {
            String file = "Start Page";
            if (this.startPage == null)
                return;

            DocumentViewModel pane = this.GetDocModel(file);
            if (pane == null)
            {
                pane = new DocumentViewModel(file, this.startPage, this);
                //this.Shell.AddDocPane(pane);
                this.AddDocModel(pane);
            }
            pane.IsSelected = true;
        }

        public DocumentViewModel GetStartPageModel()
        {
            String file = "Start Page";
            return this.GetDocModel(file);
        }

        public DocumentViewModel GetDocModel(String fileName)
        {
            if (String.IsNullOrEmpty(fileName))
                return null;

            foreach (var item in this.Documents)
            {
                if (String.Compare(fileName, item.FilePath, true) == 0)
                    return item;
            }

            return null;
        }
        
        public DocumentViewModel GetDocModel(IViEditor editor)
        {
            return editor == null ? null : this.GetDocModel(editor.FileName);
        }

        public void AddDocModel(IViEditor editor)
        {
            if (editor == null)
                return;

            var model = this.GetDocModel(editor);
            if (model != null)
                return;

            this.AddDocModel(new DocumentViewModel(editor, this));
        }

        public void AddDocModel(DocumentViewModel model)
        {
            if (this.Documents.Contains(model))
                return;

            this.Documents.Add(model);
            model.IsActive = true;
            model.IsSelected = true;
        }

        public bool OpenDocument(string fileName)
        {
            // 如果文件不存在，则退出；
            if (!File.Exists(fileName))
                return false;

            // 2、判断当前文件是否已经打开，如果已经打开，则将其状态置为Active；
            DocumentViewModel model = this.GetDocModel(fileName);
            if (model != null)
            {
                model.IsSelected = true;
                model.IsActive = true;
                return true;
            }
            
            // 3、目标文档未打开，则获取对应的工厂实例，并创建相关视图对象；
            IViEditorFactory factory = this.GetEditorFactory(fileName);
            if (factory == null)
                return false;
            IViEditor editor = factory.CreateEditorInstance();
            // 4、加载相关数据；
            editor.Load(fileName);

            this.AddDocModel(new DocumentViewModel(editor, this));
            return true;
        }

        public bool CloseDocument(string fileName)
        {
            return this.CloseDocument(this.GetDocModel(fileName));
        }

        public bool CloseDocument(DocumentViewModel model)
        {
            return this.Documents.Remove(model);
        }

        public void CloseAllDocuments()
        {
            this.CloseAllButThis((DocumentViewModel)null);
        }

        public void CloseAllButThis(string fileName)
        {
            DocumentViewModel model = this.GetDocModel(fileName);
            this.CloseAllButThis(model);
        }

        public void CloseAllButThis(DocumentViewModel model)
        {
            DocumentViewModel startPage = this.GetStartPageModel();
            foreach (var item in this.Documents.ToList())
            {
                if (item == startPage)
                    continue;
                if (item == model)
                    continue;

                this.CloseDocument(item);
            }
        }
    }
}

using DothanTech.ViGET.ViService;
using Microsoft.Win32;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DothanTech.ViGET.Shell.ViewModels
{
    public class DocumentViewModel : PaneViewModel
    {
        #region Editor

        private IViEditor editor;
        private ViDocManager docManager;

        #endregion

        public DocumentViewModel(IViEditor editor, ViDocManager docManager)
        {
            this.editor = editor;
            this.FilePath = editor.FileName;
            this.Title = this.FileName;
            this.Control = editor.Control;
            this.docManager = docManager;
        }

        public DocumentViewModel(String file, Control control, ViDocManager docManager)
        {
            this.FilePath = file;
            this.Title = this.FileName;
            this.Control = control;
            this.docManager = docManager;
        }

        #region FilePath
        private string _filePath = null;
        public string FilePath
        {
            get { return _filePath; }
            set
            {
                if (_filePath != value)
                {
                    _filePath = value;
                    RaisePropertyChanged("FilePath");
                    RaisePropertyChanged("FileName");
                    RaisePropertyChanged("Title");

                    if (File.Exists(_filePath))
                    {
                        ContentId = _filePath;
                    }
                }
            }
        }

        public string FileName
        {
            get
            {
                if (FilePath == null)
                    return "Noname" + (IsDirty ? "*" : "");

                return System.IO.Path.GetFileName(FilePath) + (IsDirty ? "*" : "");
            }
        }

        #endregion

        public Control Control
        {
            get { return _control; }
            set { 
                _control = value;
                this.RaisePropertyChanged("Control");
            }
        }
        private Control _control;


        #region IsDirty

        private bool _isDirty = false;
        public bool IsDirty
        {
            get { return _isDirty; }
            set
            {
                if (_isDirty != value)
                {
                    _isDirty = value;
                    RaisePropertyChanged("IsDirty");
                    RaisePropertyChanged("FileName");
                }
            }
        }

        #endregion

        #region SaveCommand
        DelegateCommand _saveCommand = null;
        public DelegateCommand SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                {
                    _saveCommand = new DelegateCommand(() =>
                    {
                        this.editor.Save(null);
                    }, () => this.IsDirty);
                }

                return _saveCommand;
            }
        }

        #endregion

        #region SaveAsCommand
        DelegateCommand _saveAsCommand = null;
        public DelegateCommand SaveAsCommand
        {
            get
            {
                if (_saveAsCommand == null)
                {
                    _saveAsCommand = new DelegateCommand(() =>
                    {
                        var dlg = new SaveFileDialog();
                        if (dlg.ShowDialog().GetValueOrDefault())
                        {
                            this.editor.Save(dlg.SafeFileName);
                        }
                    }, () => {
                        if (this.editor == null)
                            return false;

                        return true;
                    });
                }

                return _saveAsCommand;
            }
        }

        #endregion

        #region CloseCommand
        DelegateCommand _closeCommand = null;
        public DelegateCommand CloseCommand
        {
            get
            {
                if (_closeCommand == null)
                {
                    _closeCommand = new DelegateCommand(() =>
                    {
                        this.editor.CloseCommand.Execute(null);
                    }, () =>
                    {
                        if (this.editor == null || this.editor.CloseCommand == null)
                            return false;

                        return this.editor.CloseCommand.CanExecute(null);
                    });
                }

                return _closeCommand;
            }
        }

        #endregion

    }
}

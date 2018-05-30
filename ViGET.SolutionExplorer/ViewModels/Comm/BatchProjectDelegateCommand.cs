using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Dothan.Manager
{
    public class BatchProjectDelegateCommand : ICommand
    {
        #region life cycle

        public Action ExecuteMethod { get; set; }
        public Func<bool> CanExecuteMethod { get; set; }

        public BatchProjectDelegateCommand(Action executeMethod)
            : this(executeMethod, null)
        {
        }

        public BatchProjectDelegateCommand(Action executeMethod, Func<bool> canExecuteMethod)
        {
            this.ExecuteMethod = executeMethod;
            this.CanExecuteMethod = canExecuteMethod;
        }

        #endregion

        public bool CanExecute(object parameter)
        {
            if (this.CanExecuteMethod == null)
                return true;

            return this.CanExecuteMethod();
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            if (this.ExecuteMethod == null)
                return;

            this.ExecuteMethod();
        }

    }
}

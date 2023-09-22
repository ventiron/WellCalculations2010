using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace WellCalculations2010.Commands
{
    internal class SimpleCommand : ICommand
    {
        private Action<object> _execute;
        private Func<object, bool> _canExecute;

        public SimpleCommand(Action<object> _execute, Func<object, bool> _canExecute = null)
        {
            this._execute = _execute;
            this._canExecute = _canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object par) { return this._canExecute == null || _canExecute(par); }

        public void Execute(object par)
        {
            this._execute(par);
        }
    }
}

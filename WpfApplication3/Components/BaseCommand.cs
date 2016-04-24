using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WpfApplication3.Components
{
    public class BaseCommand : ICommand
    {
        private Action _exec;
        private Func<bool> _canExec;


        public BaseCommand(Action exec, Func<bool> canExec = null)
        {
            _exec = exec;
            _canExec = canExec;
        }

        public virtual bool CanExecute(object parameter)
        {
            if (_canExec == null) return true;
            return _canExec.Invoke();
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public virtual void Execute(object parameter)
        {
            _exec.Invoke();
        }
    }
}

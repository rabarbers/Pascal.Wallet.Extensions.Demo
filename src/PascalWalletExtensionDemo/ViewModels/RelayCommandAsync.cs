using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PascalWalletExtensionDemo.ViewModels
{
    public class RelayCommandAsync : ICommand
    {
        readonly Func<Task> _task;
        readonly Func<object, Task> _execute;
        readonly Predicate<object> _canExecute;

        public RelayCommandAsync(Func<object, Task> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public RelayCommandAsync(Func<Task> execute, Predicate<object> canExecute = null)
        {
            _task = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public async void Execute(object parameter)
        {
            if(_task != null)
            {
                await _task();
            }
            if(_execute != null)
            {
                await _execute(parameter);
            }
        }
    }
}

using System;
using System.Diagnostics;
using System.Windows.Input;

namespace PascalWalletExtensionDemo.ViewModels
{
    public class RelayCommand : ICommand
    {
        readonly Action _executeWithoutParameter;
        readonly Action<object> _execute;
        readonly Predicate<object> _canExecute;

        public RelayCommand(Action execute, Predicate<object> canExecute = null)
        {
            _executeWithoutParameter = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }
        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
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
        public void Execute(object parameter)
        {
            if(_executeWithoutParameter != null)
            {
                _executeWithoutParameter();
            }
            if(_execute != null)
            {
                _execute(parameter);
            }
        }
    }
}

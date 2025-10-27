using System.Windows.Input;

namespace PrevisionalAccountManager.Utils
{
    public class RelayCommand(Action execute, Func<bool>? canExecute = null)
        : RelayCommand<object>((o) => { execute(); }, (o) => canExecute?.Invoke() ?? true)
    { }

    public class RelayCommand<T>(Action<T> execute, Func<T?, bool>? canExecute = null) : ICommand
    {
        public event EventHandler? CanExecuteChanged {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object? parameter)
        {
            return canExecute == null || canExecute((T)parameter!);
        }

        public void Execute(object? parameter)
        {
            execute((T)parameter!);
        }
    }
}
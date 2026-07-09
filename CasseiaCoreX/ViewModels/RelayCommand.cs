using System;
using System.Windows.Input;

namespace CasseiaCoreX.ViewModels
{
    /// <summary>
    /// 实现 ICommand
    /// </summary>
    /// 
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;

        public void Execute(object parameter) => _execute();

        public void RaiseCanExecuteChanged()
        {
            // 手动触发 CanExecute 重新评估
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
using System;
using System.Windows.Input;

namespace DayKindRegistration.Common
{
    /// <summary>
    /// ICommand を実装し、ViewModel から View のアクションを実行するためのリレーコマンドクラスです。
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="execute">実行するアクション</param>
        /// <param name="canExecute">実行可能かどうかを判定するロジック (省略時は常にtrue)</param>
        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        /// <summary>
        /// コマンドが実行可能かどうかを判定します。
        /// </summary>
        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        /// <summary>
        /// コマンドを実行します。
        /// </summary>
        public void Execute(object? parameter)
        {
            _execute(parameter);
        }
    }
}

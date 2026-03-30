using System;
using System.Windows.Input;

namespace TimeZoneRegistration.Common
{
    /// <summary>
    /// ICommandの実装クラス (デリゲートによるコマンド実行)
    /// </summary>
    public class DelegateCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public DelegateCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// 実行可能状態の変更イベント
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        /// <summary>
        /// コマンドが実行可能か判定
        /// </summary>
        public bool CanExecute(object? parameter) => _canExecute == null || _canExecute();

        /// <summary>
        /// コマンドを実行
        /// </summary>
        public void Execute(object? parameter) => _execute();
    }
}

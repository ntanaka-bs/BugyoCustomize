using System.Windows.Input;

namespace AttendanceSystem.Common
{
    /// <summary>
    /// UI（ボタン等）からのコマンド要求をデリゲート（メソッド）へ転送する実装クラスです。
    /// </summary>
    public class DelegateCommand : ICommand
    {
        private readonly Action _execute;      // 実行する処理本体
        private readonly Func<bool>? _canExecute; // 実行可能かどうかを判定する関数

        /// <summary>
        /// コマンドの実行可否状態が変化したときに発生するイベント
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        /// <summary>
        /// コンストラクタ。実行する処理と実行可否判定関数をセットします。
        /// </summary>
        /// <param name="execute">実行するアクション</param>
        /// <param name="canExecute">実行可能判定を返すデリゲート（省略可）</param>
        public DelegateCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// コマンドが実行可能かどうかを判断します。
        /// </summary>
        public bool CanExecute(object? parameter) => _canExecute == null || _canExecute();

        /// <summary>
        /// コマンドを実行します。
        /// </summary>
        public void Execute(object? parameter) => _execute();
    }
}

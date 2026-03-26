using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace UnitPriceRegistration.ViewModels
{
    /// <summary>
    /// INotifyPropertyChanged実装ViewModel用基底クラス
    /// </summary>
    public class ViewModelBase : INotifyPropertyChanged
    {
        /// <summary>
        /// プロパティ値変更イベント
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// プロパティ変更イベント発行
        /// </summary>
        /// <param name="propertyName">変更されたプロパティ名</param>
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// フィールド値更新・変更通知
        /// </summary>
        /// <typeparam name="T">プロパティ型</typeparam>
        /// <param name="storage">更新対象のバックフィールド</param>
        /// <param name="value">新しい値</param>
        /// <param name="propertyName">プロパティ名</param>
        /// <returns>変更有：true、同一値：false</returns>
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(storage, value)) return false;
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    /// <summary>
    /// ICommand実装Actionラッパークラス
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;

        /// <summary>
        /// コマンド初期化
        /// </summary>
        /// <param name="execute">実行処理</param>
        /// <param name="canExecute">実行可否判定ロジック</param>
        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// 実行可否状態変化トリガー
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// 実行可否判定
        /// </summary>
        public bool CanExecute(object? parameter) => _canExecute == null || _canExecute(parameter);

        /// <summary>
        /// 処理実行
        /// </summary>
        public void Execute(object? parameter) => _execute(parameter);
    }
}

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace JobRegistration.Common
{
    /// <summary>
    /// 全ての ViewModel の基底となるクラスです。
    /// プロパティ変更通知 (INotifyPropertyChanged) の共通実装を提供します。
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        /// <summary>
        /// プロパティ値が変更されたときに発生するイベント
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// フィールドの値を更新し、変更があった場合は PropertyChanged イベントを発生させます。
        /// </summary>
        /// <typeparam name="T">プロパティの型</typeparam>
        /// <param name="storage">プロパティ値を保持するフィールドへの参照</param>
        /// <param name="value">新しい値</param>
        /// <param name="propertyName">プロパティ名（呼び出し元のプロパティ名が自動で入ります）</param>
        /// <returns>値に変更があった場合は true、それ以外は false</returns>
        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(storage, value)) return false;
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// 指定されたプロパティの変更通知イベントを発行します。
        /// </summary>
        /// <param name="propertyName">プロパティ名</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

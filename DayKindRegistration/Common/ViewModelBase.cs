using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DayKindRegistration.Common
{
    /// <summary>
    /// ViewModelの基底クラスです。
    /// プロパティ変更通知 (INotifyPropertyChanged) の基本機能を実装します。
    /// </summary>
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// プロパティ値が変更されたことを通知します。
        /// </summary>
        /// <param name="propertyName">変更されたプロパティ名 (自動取得)</param>
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// バッキングフィールドの値を更新し、変更がある場合は通知を行います。
        /// </summary>
        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(backingStore, value))
                return false;

            backingStore = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}

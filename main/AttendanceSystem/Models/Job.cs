using System;
using System.ComponentModel;

namespace AttendanceSystem.Models
{
    /// <summary>
    /// 職種の一つのエンティティ（データ行）を表すモデルクラスです。
    /// INotifyPropertyChanged を実装しており、プロパティ変更をUIに通知します。
    /// </summary>
    public class Job : INotifyPropertyChanged
    {
        private int _jobId;                 // 内部管理用ID
        private int? _jobCode;              // 職種コード
        private string? _jobName;           // 職種名称
        private string? _jobAbbreviationName; // 職種略称

        /// <summary>
        /// データベース上の主キー (C_JobID)。自動採番されます。
        /// </summary>
        public int C_JobID
        {
            get => _jobId;
            set
            {
                _jobId = value;
                OnPropertyChanged(nameof(C_JobID));
            }
        }

        /// <summary>
        /// 職種コード。一意の識別番号として使用されます。
        /// </summary>
        public int? C_JobCode
        {
            get => _jobCode;
            set
            {
                _jobCode = value;
                OnPropertyChanged(nameof(C_JobCode));
            }
        }

        /// <summary>
        /// 職種の正式名称。
        /// </summary>
        public string? C_JobName
        {
            get => _jobName;
            set
            {
                _jobName = value;
                OnPropertyChanged(nameof(C_JobName));
            }
        }

        /// <summary>
        /// 職種の略称。画面表示や帳票などで使用されます。
        /// </summary>
        public string? C_JobAbbreviationName
        {
            get => _jobAbbreviationName;
            set
            {
                _jobAbbreviationName = value;
                OnPropertyChanged(nameof(C_JobAbbreviationName));
            }
        }

        /// <summary>
        /// プロパティ値が変更されたときに発生するイベント
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// プロパティ変更通知を発行します。
        /// </summary>
        /// <param name="propertyName">プロパティ名</param>
        protected virtual void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

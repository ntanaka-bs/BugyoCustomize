using System;
using System.ComponentModel;

namespace Obc.Bs.Windows.UI.SKI.JobRegistration.Model
{
    /// <summary>
    /// 職種情報を表すモデル
    /// </summary>
    public class Job : INotifyPropertyChanged
    {
        private int _jobId;
        private int? _jobCode;
        private string? _jobName;
        private string? _jobAbbreviationName;

        /// <summary>
        /// 職種ID (内部管理用)
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
        /// 職種コード
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
        /// 職種名称
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
        /// 職種略称
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

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

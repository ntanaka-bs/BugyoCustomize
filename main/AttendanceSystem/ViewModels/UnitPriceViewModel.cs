using System;
using AttendanceSystem.Common;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using AttendanceSystem.Models;

namespace AttendanceSystem.ViewModels
{
    /// <summary>
    /// メイン画面用ViewModel。検索条件・明細リスト・各種コマンドを保持
    /// </summary>
    public class UnitPriceViewModel : ViewModelBase
    {
        public event Action? RequestClose;
        private readonly UnitPriceRepository _repository;

        /// <summary>
        /// コンストラクタ。リポジトリ・各種コマンド初期化およびDB構成確認
        /// </summary>
        public UnitPriceViewModel()
        {
            _repository = new UnitPriceRepository();
            SearchCommand = new RelayCommand(ExecuteSearch);
            CancelCommand = new RelayCommand(ExecuteCancel, CanExecuteCancel);
            DeleteRowCommand = new RelayCommand(ExecuteDeleteRow, CanExecuteDeleteRow);
            F12Command = new RelayCommand(ExecuteF12);

            Details.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(HasData));
                OnPropertyChanged(nameof(F7Text));
                OnPropertyChanged(nameof(F10Text));
                OnPropertyChanged(nameof(F12Text));
                CommandManager.InvalidateRequerySuggested();
            };

            try
            {
                UnitPriceDatabaseHelper.EnsureTablesCreated();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(MessageConfig.ErrorSaveFailed, ex.Message), MessageConfig.TitleError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string _jobCode = string.Empty;
        public string JobCode
        {
            get => _jobCode;
            set
            {
                if (SetProperty(ref _jobCode, value)) UpdateJobName();
            }
        }

        private string _jobName = string.Empty;
        public string JobName { get => _jobName; set => SetProperty(ref _jobName, value); }

        private string _dayKindCode = string.Empty;
        public string DayKindCode
        {
            get => _dayKindCode;
            set
            {
                if (SetProperty(ref _dayKindCode, value)) UpdateDayKindName();
            }
        }

        private string _dayKindName = string.Empty;
        public string DayKindName { get => _dayKindName; set => SetProperty(ref _dayKindName, value); }

        private string _timeZoneCode = string.Empty;
        public string TimeZoneCode
        {
            get => _timeZoneCode;
            set
            {
                if (SetProperty(ref _timeZoneCode, value)) UpdateTimeZoneName();
            }
        }

        private string _timeZoneName = string.Empty;
        public string TimeZoneName { get => _timeZoneName; set => SetProperty(ref _timeZoneName, value); }

        public ObservableCollection<UnitPriceDetail> Details { get; } = new ObservableCollection<UnitPriceDetail>();

        private UnitPriceDetail? _selectedDetail;
        public UnitPriceDetail? SelectedDetail { get => _selectedDetail; set => SetProperty(ref _selectedDetail, value); }

        public bool HasData => Details.Any();

        public string F7Text => HasData ? MessageConfig.BtnDeleteRow : "";
        public string F10Text => HasData ? MessageConfig.BtnCancel : "";
        public string F12Text => HasData ? MessageConfig.BtnRegister : MessageConfig.BtnClose;

        public ICommand SearchCommand { get; }
        public ICommand F12Command { get; }
        public ICommand CancelCommand { get; }
        public ICommand DeleteRowCommand { get; }

        private void UpdateJobName()
        {
            if (int.TryParse(JobCode, out int code))
                JobName = _repository.GetJobName(code);
            else
                JobName = string.Empty;
        }

        private void UpdateDayKindName()
        {
            if (int.TryParse(DayKindCode, out int code))
                DayKindName = _repository.GetDayKindName(code);
            else
                DayKindName = string.Empty;
        }

        private void UpdateTimeZoneName()
        {
            if (int.TryParse(TimeZoneCode, out int code))
                TimeZoneName = _repository.GetTimeZoneName(code);
            else
                TimeZoneName = string.Empty;
        }

        private void ExecuteSearch(object? obj)
        {
            if (string.IsNullOrWhiteSpace(JobCode) || !int.TryParse(JobCode, out int jCode))
            {
                MessageBox.Show(MessageConfig.WarnInvalidCode, MessageConfig.TitleWarning, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int? dCode = int.TryParse(DayKindCode, out int d) ? d : (int?)null;
            int? tCode = int.TryParse(TimeZoneCode, out int t) ? t : (int?)null;

            try
            {
                var list = _repository.GetDetailData(jCode, dCode, tCode);
                Details.Clear();
                foreach (var item in list) Details.Add(item);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(MessageConfig.ErrorSaveFailed, ex.Message), MessageConfig.TitleError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteF12(object? obj)
        {
            if (HasData)
            {
                ExecuteSave();
            }
            else
            {
                RequestClose?.Invoke();
            }
        }

        private void ExecuteSave()
        {
            try
            {
                foreach (var detail in Details)
                {
                    if (detail.UnitPrice > 0 || detail.UnitPriceID > 0)
                    {
                        _repository.SaveUnitPriceData(detail);
                    }
                }
                
                MessageBox.Show(MessageConfig.InfoSuccessSave, MessageConfig.TitleSuccess, MessageBoxButton.OK, MessageBoxImage.Information);
                ExecuteSearch(null!); 
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(MessageConfig.ErrorSaveFailed, ex.Message), MessageConfig.TitleError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanExecuteCancel(object? obj) => HasData;

        private void ExecuteCancel(object? obj)
        {
            Details.Clear();
            JobCode = ""; JobName = "";
            DayKindCode = ""; DayKindName = "";
            TimeZoneCode = ""; TimeZoneName = "";
        }

        private bool CanExecuteDeleteRow(object? obj) => HasData && SelectedDetail != null && SelectedDetail.UnitPriceID > 0;

        private void ExecuteDeleteRow(object? obj)
        {
            if (SelectedDetail == null || SelectedDetail.UnitPriceID <= 0) return;

            var res = MessageBox.Show(string.Format(MessageConfig.ConfirmDelete, MessageConfig.BtnDeleteRow), MessageConfig.TitleConfirm, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.Yes)
            {
                try
                {
                    _repository.DeleteUnitPriceData(SelectedDetail.UnitPriceID);
                    ExecuteSearch(null!); 
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format(MessageConfig.ErrorDeleteFailed, ex.Message), MessageConfig.TitleError, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}

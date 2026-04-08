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
    /// メイン画面用ViewModel
    /// </summary>
    public class TimeZoneViewModel : ViewModelBase
    {
        private readonly TimeZoneRepository _repository;
        private TimeZoneModel? _selectedTimeZone;
        private TimeZoneModel? _highlightedTimeZone;
        private int? _inputCode;
        private string? _inputName;
        private string? _inputAbbreviation;
        private int? _inputOrderCode;
        private bool _isCodeEnabled = true;
        private bool _isInputStarted;

        public ObservableCollection<TimeZoneModel> TimeZones { get; } = new ObservableCollection<TimeZoneModel>();

        public TimeZoneModel? HighlightedTimeZone
        {
            get => _highlightedTimeZone;
            set => SetProperty(ref _highlightedTimeZone, value);
        }

        public bool IsActionButtonsVisible => _isInputStarted;

        public string SaveOrCloseButtonText => _isInputStarted ? MessageConfig.BtnRegister : MessageConfig.BtnClose;

        public event Action? RequestClose;

        public TimeZoneModel? SelectedTimeZone
        {
            get => _selectedTimeZone;
            set
            {
                if (SetProperty(ref _selectedTimeZone, value) && _selectedTimeZone != null)
                {
                    HighlightedTimeZone = _selectedTimeZone;
                    _inputCode = _selectedTimeZone.C_TimeZoneCode;
                    _inputName = _selectedTimeZone.C_TimeZoneName;
                    _inputAbbreviation = _selectedTimeZone.C_TimeZoneAbbreviationName;
                    _inputOrderCode = _selectedTimeZone.C_OrderCode;
                    
                    OnPropertyChanged(nameof(InputCode));
                    OnPropertyChanged(nameof(InputName));
                    OnPropertyChanged(nameof(InputAbbreviation));
                    OnPropertyChanged(nameof(InputOrderCode));

                    IsCodeEnabled = false;
                    UpdateInputStatus(true);
                }
            }
        }

        public int? InputCode
        {
            get => _inputCode;
            set
            {
                if (SetProperty(ref _inputCode, value))
                {
                    UpdateInputStatus(value != null || HasInputValues());
                    if (value != null) CheckAndLoadExistingTimeZone(value.Value);
                }
            }
        }

        public string? InputName
        {
            get => _inputName;
            set
            {
                if (SetProperty(ref _inputName, value))
                {
                    UpdateInputStatus(!string.IsNullOrWhiteSpace(value) || HasInputValues());
                    if (string.IsNullOrWhiteSpace(InputAbbreviation) && !string.IsNullOrWhiteSpace(value))
                    {
                        InputAbbreviation = value.Length > 20 ? value.Substring(0, 20) : value; 
                    }
                }
            }
        }

        public string? InputAbbreviation
        {
            get => _inputAbbreviation;
            set
            {
                if (SetProperty(ref _inputAbbreviation, value))
                {
                    UpdateInputStatus(!string.IsNullOrWhiteSpace(value) || HasInputValues());
                }
            }
        }

        public int? InputOrderCode
        {
            get => _inputOrderCode;
            set
            {
                if (SetProperty(ref _inputOrderCode, value))
                {
                    UpdateInputStatus(value != null || HasInputValues());
                }
            }
        }

        public bool IsCodeEnabled
        {
            get => _isCodeEnabled;
            set => SetProperty(ref _isCodeEnabled, value);
        }

        public TimeZoneViewModel()
        {
            _repository = new TimeZoneRepository();
            SaveCommand = new DelegateCommand(OnSave);
            DeleteCommand = new DelegateCommand(OnDelete);
            CancelCommand = new DelegateCommand(OnCancel);
            DoubleClickCommand = new DelegateCommand(OnDoubleClick);
            ClearFields();
            RefreshList();
        }

        private void OnDoubleClick()
        {
            if (HighlightedTimeZone != null)
            {
                SelectedTimeZone = HighlightedTimeZone;
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand DoubleClickCommand { get; }

        private bool HasInputValues() => InputCode != null || !string.IsNullOrWhiteSpace(InputName) || !string.IsNullOrWhiteSpace(InputAbbreviation);

        private void UpdateInputStatus(bool started)
        {
            if (_isInputStarted != started)
            {
                _isInputStarted = started;
                OnPropertyChanged(nameof(IsActionButtonsVisible));
                OnPropertyChanged(nameof(SaveOrCloseButtonText));
            }
        }

        private void CheckAndLoadExistingTimeZone(int code)
        {
            var existing = TimeZones.FirstOrDefault(t => t.C_TimeZoneCode == code);
            if (existing != null && SelectedTimeZone != existing)
            {
                SelectedTimeZone = existing;
            }
        }

        private void RefreshList()
        {
            TimeZones.Clear();
            foreach (var tz in _repository.GetAllTimeZones())
            {
                TimeZones.Add(tz);
            }
        }

        private void OnSave()
        {
            if (!_isInputStarted)
            {
                RequestClose?.Invoke();
                return;
            }

            if (InputCode == null || string.IsNullOrWhiteSpace(InputName) || string.IsNullOrWhiteSpace(InputAbbreviation))
            {
                MessageBox.Show(MessageConfig.WarnInputRequiredTZ, MessageConfig.TitleInputConfirm, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var tzToSave = new TimeZoneModel
            {
                C_TimeZoneID = SelectedTimeZone?.C_TimeZoneID ?? 0,
                C_TimeZoneCode = InputCode,
                C_TimeZoneName = InputName,
                C_TimeZoneAbbreviationName = InputAbbreviation,
                C_OrderCode = InputOrderCode
            };

            var result = MessageBox.Show(MessageConfig.ConfirmSave, MessageConfig.TitleConfirm, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                _repository.Save(tzToSave);
                MessageBox.Show(MessageConfig.InfoSuccessSave, MessageConfig.TitleInfo, MessageBoxButton.OK, MessageBoxImage.Information);
                RefreshList();
                ClearFields(); 
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(MessageConfig.ErrorSaveFailed, ex.Message), MessageConfig.TitleError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnDelete()
        {
            if (SelectedTimeZone == null) return;

            var result = MessageBox.Show(string.Format(MessageConfig.ConfirmDelete, "時間帯区分"), MessageConfig.TitleDeleteConfirm, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    int delResult = _repository.Delete(SelectedTimeZone.C_TimeZoneID);
                    if (delResult == 1) 
                    {
                        MessageBox.Show(MessageConfig.InfoSuccessDelete, MessageConfig.TitleInfo, MessageBoxButton.OK, MessageBoxImage.Information);
                        RefreshList();
                        ClearFields();
                    }
                    else if (delResult == 2) 
                    {
                        MessageBox.Show(MessageConfig.WarnUsedInUnitPrice, MessageConfig.TitleInfo, MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else if (delResult == 3) 
                    {
                        MessageBox.Show(MessageConfig.WarnUsedInAttendance, MessageConfig.TitleInfo, MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format(MessageConfig.ErrorDeleteFailed, ex.Message), MessageConfig.TitleError, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OnCancel()
        {
            if (_isInputStarted)
            {
                var result = MessageBox.Show(MessageConfig.ConfirmCancel, MessageConfig.TitleConfirm, MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result != MessageBoxResult.Yes) return;
            }
            ClearFields();
        }

        private void ClearFields()
        {
            _inputCode = null;
            _inputName = null;
            _inputAbbreviation = null;
            _inputOrderCode = 1; 
            _isCodeEnabled = true;
            _selectedTimeZone = null;

            OnPropertyChanged(nameof(InputCode));
            OnPropertyChanged(nameof(InputName));
            OnPropertyChanged(nameof(InputAbbreviation));
            OnPropertyChanged(nameof(InputOrderCode));
            OnPropertyChanged(nameof(IsCodeEnabled));
            OnPropertyChanged(nameof(SelectedTimeZone));

            UpdateInputStatus(false);
        }
    }
}

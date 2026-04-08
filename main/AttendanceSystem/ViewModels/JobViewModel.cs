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
    /// メイン画面の表示ロジックとデータバインディングを管理する ViewModel クラスです。
    /// </summary>
    public class JobViewModel : ViewModelBase
    {
        private readonly JobRepository _repository; // データアクセス用リポジトリ
        private Job? _selectedJob;                 // 現在選択されている職種
        private int? _inputCode;                   // 入力エリア：コード
        private string? _inputName;                // 入力エリア：名称
        private string? _inputAbbreviation;        // 入力エリア：略称
        private bool _isCodeEnabled = true;        // コード入力欄の活性状態
        private bool _isInputStarted;              // 入力開始または選択中フラグ

        /// <summary>
        /// 画面に表示する職種一覧のコレクション
        /// </summary>
        public ObservableCollection<Job> Jobs { get; } = new ObservableCollection<Job>();

        /// <summary>
        /// アクションボタン（削除・中止）を活性化するかどうか
        /// </summary>
        public bool IsActionButtonsEnabled => _isInputStarted;

        /// <summary>
        /// 登録ボタンに表示するテキスト
        /// </summary>
        public string SaveOrCloseButtonText => _isInputStarted ? MessageConfig.BtnRegister : MessageConfig.BtnClose;

        /// <summary>
        /// ウィンドウを閉じる要求を外部（UI層）に通知するためのイベント
        /// </summary>
        public event Action? RequestClose;

        /// <summary>
        /// 一覧（DataGrid）で現在選択されている職種項目。
        /// </summary>
        public Job? SelectedJob
        {
            get => _selectedJob;
            set
            {
                if (SetProperty(ref _selectedJob, value))
                {
                    if (_selectedJob != null)
                    {
                        _inputCode = _selectedJob.C_JobCode;
                        _inputName = _selectedJob.C_JobName;
                        _inputAbbreviation = _selectedJob.C_JobAbbreviationName;
                        
                        OnPropertyChanged(nameof(InputCode));
                        OnPropertyChanged(nameof(InputName));
                        OnPropertyChanged(nameof(InputAbbreviation));

                        IsCodeEnabled = false;
                        UpdateInputStatus(true);
                    }
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
                    UpdateInputStatus(value != null || !string.IsNullOrWhiteSpace(InputName) || !string.IsNullOrWhiteSpace(InputAbbreviation));
                    if (value != null)
                    {
                        CheckAndLoadExistingJob(value.Value);
                    }
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
                    UpdateInputStatus(!string.IsNullOrWhiteSpace(value) || InputCode != null || !string.IsNullOrWhiteSpace(InputAbbreviation));
                    if (string.IsNullOrWhiteSpace(InputAbbreviation) && !string.IsNullOrWhiteSpace(value))
                    {
                        InputAbbreviation = value.Length > 6 ? value.Substring(0, 6) : value;
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
                    UpdateInputStatus(!string.IsNullOrWhiteSpace(value) || InputCode != null || !string.IsNullOrWhiteSpace(InputName));
                }
            }
        }

        public bool IsCodeEnabled
        {
            get => _isCodeEnabled;
            set => SetProperty(ref _isCodeEnabled, value);
        }

        private void UpdateInputStatus(bool started)
        {
            if (_isInputStarted != started)
            {
                _isInputStarted = started;
                OnPropertyChanged(nameof(IsActionButtonsEnabled));
                OnPropertyChanged(nameof(SaveOrCloseButtonText));
            }
        }

        private void CheckAndLoadExistingJob(int code)
        {
            var existing = Jobs.FirstOrDefault(j => j.C_JobCode == code);
            if (existing != null && SelectedJob != existing)
            {
                SelectedJob = existing;
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand DoubleClickCommand { get; }

        public JobViewModel()
        {
            _repository = new JobRepository();
            SaveCommand = new DelegateCommand(OnSave);
            DeleteCommand = new DelegateCommand(OnDelete);
            CancelCommand = new DelegateCommand(OnCancel);
            DoubleClickCommand = new DelegateCommand(() => { });
            RefreshList();
        }

        private void RefreshList()
        {
            Jobs.Clear();
            foreach (var job in _repository.GetAllJobs())
            {
                Jobs.Add(job);
            }
        }

        private void OnSave()
        {
            if (!_isInputStarted)
            {
                RequestClose?.Invoke();
                return;
            }

            if (InputCode == null || string.IsNullOrWhiteSpace(InputName))
            {
                MessageBox.Show(MessageConfig.WarnInputRequired, MessageConfig.TitleInputConfirm, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var jobToSave = new Job
            {
                C_JobID = SelectedJob?.C_JobID ?? 0,
                C_JobCode = InputCode,
                C_JobName = InputName,
                C_JobAbbreviationName = InputAbbreviation
            };

            var result = MessageBox.Show(MessageConfig.ConfirmSave, MessageConfig.TitleConfirm, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                _repository.Save(jobToSave);
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
            if (SelectedJob == null) return;

            var result = MessageBox.Show(string.Format(MessageConfig.ConfirmDelete, $"職種「{SelectedJob.C_JobName}」"), MessageConfig.TitleDeleteConfirm, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _repository.Delete(SelectedJob.C_JobID);
                    MessageBox.Show(MessageConfig.InfoSuccessDelete, MessageConfig.TitleInfo, MessageBoxButton.OK, MessageBoxImage.Information);
                    RefreshList();
                    ClearFields();
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
            _isCodeEnabled = true;
            _selectedJob = null;

            OnPropertyChanged(nameof(InputCode));
            OnPropertyChanged(nameof(InputName));
            OnPropertyChanged(nameof(InputAbbreviation));
            OnPropertyChanged(nameof(IsCodeEnabled));
            OnPropertyChanged(nameof(SelectedJob));

            UpdateInputStatus(false);
        }
    }
}

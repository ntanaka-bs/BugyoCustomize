using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Obc.Bs.Windows.UI.SKI.JobRegistration.Common;
using Obc.Bs.Windows.UI.SKI.JobRegistration.Model;

namespace Obc.Bs.Windows.UI.SKI.JobRegistration.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private readonly JobRepository _repository;
        private Job? _selectedJob;
        private int? _inputCode;
        private string? _inputName;
        private string? _inputAbbreviation;

        public ObservableCollection<Job> Jobs { get; } = new ObservableCollection<Job>();

        /// <summary>
        /// グリッドで選択された職種
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
                        InputCode = _selectedJob.C_JobCode;
                        InputName = _selectedJob.C_JobName;
                        InputAbbreviation = _selectedJob.C_JobAbbreviationName;
                    }
                }
            }
        }

        /// <summary>
        /// 入力欄：コード
        /// </summary>
        public int? InputCode
        {
            get => _inputCode;
            set => SetProperty(ref _inputCode, value);
        }

        /// <summary>
        /// 入力欄：名称
        /// </summary>
        public string? InputName
        {
            get => _inputName;
            set => SetProperty(ref _inputName, value);
        }

        /// <summary>
        /// 入力欄：略称
        /// </summary>
        public string? InputAbbreviation
        {
            get => _inputAbbreviation;
            set => SetProperty(ref _inputAbbreviation, value);
        }

        // コマンド
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand CancelCommand { get; }

        public MainViewModel()
        {
            _repository = new JobRepository();
            
            SaveCommand = new DelegateCommand(OnSave);
            DeleteCommand = new DelegateCommand(OnDelete);
            CancelCommand = new DelegateCommand(OnCancel);

            RefreshList();
        }

        /// <summary>
        /// 一覧を再取得
        /// </summary>
        private void RefreshList()
        {
            Jobs.Clear();
            foreach (var job in _repository.GetAllJobs())
            {
                Jobs.Add(job);
            }
        }

        /// <summary>
        /// F12: 登録処理
        /// </summary>
        private void OnSave()
        {
            if (InputCode == null || string.IsNullOrWhiteSpace(InputName))
            {
                MessageBox.Show("コードと名称を入力してください。", "入力確認", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 同一コードがあるか確認（更新または重複チェック）
            var existing = Jobs.FirstOrDefault(j => j.C_JobCode == InputCode);
            var jobToSave = new Job
            {
                C_JobID = existing?.C_JobID ?? 0,
                C_JobCode = InputCode,
                C_JobName = InputName,
                C_JobAbbreviationName = InputAbbreviation
            };

            try
            {
                _repository.Save(jobToSave);
                RefreshList();
                OnCancel(); // 保存後は入力をクリア
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存に失敗しました：\n{ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// F7: 削除処理
        /// </summary>
        private void OnDelete()
        {
            if (SelectedJob == null)
            {
                MessageBox.Show("削除する項目を選択してください。", "確認", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show($"職種「{SelectedJob.C_JobName}」を削除しますか？", "削除確認", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _repository.Delete(SelectedJob.C_JobID);
                    RefreshList();
                    OnCancel();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"削除に失敗しました：\n{ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// F10: 中止（クリア）処理
        /// </summary>
        private void OnCancel()
        {
            InputCode = null;
            InputName = null;
            InputAbbreviation = null;
            SelectedJob = null;
        }
    }
}

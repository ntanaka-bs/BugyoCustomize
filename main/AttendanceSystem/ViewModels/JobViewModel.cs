using System;
using AttendanceSystem.Common;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using AttendanceSystem.Common;
using AttendanceSystem.Models;
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

        /// <summary>
        /// 画面に表示する職種一覧のコレクション
        /// </summary>
        public ObservableCollection<Job> Jobs { get; } = new ObservableCollection<Job>();

        // --- 新機能用プロパティ ---
        private bool _isInputStarted; // 入力開始または選択中フラグ

        /// <summary>
        /// アクションボタン（削除・中止）を活性化するかどうか
        /// 何かを入力中、または一覧でデータを選択しているときに true になります。
        /// </summary>
        public bool IsActionButtonsEnabled => _isInputStarted;

        /// <summary>
        /// 登録ボタンに表示するテキスト
        /// 初期状態では「閉じる」、入力中・選択中は「登録」になります。
        /// </summary>
        public string SaveOrCloseButtonText => _isInputStarted ? "登録" : "閉じる";

        /// <summary>
        /// ウィンドウを閉じる要求を外部（UI層）に通知するためのイベント
        /// </summary>
        public event Action? RequestClose;

        /// <summary>
        /// 一覧（DataGrid）で現在選択されている職種項目。
        /// 選択が変更されると、その内容を入力エリアに反映します。
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
                        // 選択された行のデータを入力フィールドにコピー
                        _inputCode = _selectedJob.C_JobCode;
                        _inputName = _selectedJob.C_JobName;
                        _inputAbbreviation = _selectedJob.C_JobAbbreviationName;
                        
                        // プロパティ変更を通知
                        OnPropertyChanged(nameof(InputCode));
                        OnPropertyChanged(nameof(InputName));
                        OnPropertyChanged(nameof(InputAbbreviation));

                        // 既存データの修正時はコード（主キーの一部）の変更を禁止し、入力開始状態にする
                        IsCodeEnabled = false;
                        UpdateInputStatus(true);
                    }
                }
            }
        }

        /// <summary>
        /// 画面入力エリアの「コード」値。
        /// 入力されると「入力開始」状態になり、存在するコードなら自動的にそのデータを読み込みます。
        /// </summary>
        public int? InputCode
        {
            get => _inputCode;
            set
            {
                if (SetProperty(ref _inputCode, value))
                {
                    // 値が入れば入力開始とみなす
                    UpdateInputStatus(value != null || !string.IsNullOrWhiteSpace(InputName) || !string.IsNullOrWhiteSpace(InputAbbreviation));
                    
                    // 存在するコードかチェック（自動編集モード）
                    if (value != null)
                    {
                        CheckAndLoadExistingJob(value.Value);
                    }
                }
            }
        }

        /// <summary>
        /// 画面入力エリア of the 「名称」値。
        /// 名称が入力された際、略称が空であれば自動的に先頭6文字を略称として補完します。
        /// </summary>
        public string? InputName
        {
            get => _inputName;
            set
            {
                if (SetProperty(ref _inputName, value))
                {
                    UpdateInputStatus(!string.IsNullOrWhiteSpace(value) || InputCode != null || !string.IsNullOrWhiteSpace(InputAbbreviation));

                    // 略称が未入力の場合のみ、名称から自動セット
                    if (string.IsNullOrWhiteSpace(InputAbbreviation) && !string.IsNullOrWhiteSpace(value))
                    {
                        // DB定義の最大6文字に合わせて切り出し
                        InputAbbreviation = value.Length > 6 ? value.Substring(0, 6) : value;
                    }
                }
            }
        }

        /// <summary>
        /// 画面入力エリアの「略称」値
        /// </summary>
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

        /// <summary>
        /// コード入力欄の活性・非活性状態を切り替えます。
        /// </summary>
        public bool IsCodeEnabled
        {
            get => _isCodeEnabled;
            set => SetProperty(ref _isCodeEnabled, value);
        }

        /// <summary>
        /// 入力状態（ボタンのテキストや表示制御）を更新します。
        /// </summary>
        private void UpdateInputStatus(bool started)
        {
            if (_isInputStarted != started)
            {
                _isInputStarted = started;
                OnPropertyChanged(nameof(IsActionButtonsEnabled));
                OnPropertyChanged(nameof(SaveOrCloseButtonText));
            }
        }

        /// <summary>
        /// 入力されたコードが既に存在するか確認し、あれば自動的に読み込みます。
        /// </summary>
        private void CheckAndLoadExistingJob(int code)
        {
            var existing = Jobs.FirstOrDefault(j => j.C_JobCode == code);
            if (existing != null && SelectedJob != existing)
            {
                SelectedJob = existing;
            }
        }

        // 画面上のボタン等から呼び出されるコマンド
        public ICommand SaveCommand { get; }   // 登録・更新 または 閉じる (F12)
        public ICommand DeleteCommand { get; } // 削除 (F7)
        public ICommand CancelCommand { get; } // 中止・クリア (F10)
        public ICommand DoubleClickCommand { get; } // DataGridダブルクリック用

        /// <summary>
        /// コンストラクタ。リポジトリの初期化とコマンドのセットアップを行います。
        /// </summary>
        public JobViewModel()
        {
            _repository = new JobRepository();
            
            // 各操作メソッドをコマンドとして公開
            SaveCommand = new DelegateCommand(OnSave);
            DeleteCommand = new DelegateCommand(OnDelete);
            CancelCommand = new DelegateCommand(OnCancel);
            DoubleClickCommand = new DelegateCommand(() => { /* SelectedJob の setter で処理されるため空でOK */ });

            // 起動時にデータを取得して一覧に表示
            RefreshList();
        }

        /// <summary>
        /// データベースから最新の職種一覧を取得し、画面上のリストを更新します。
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
        /// 保存（F12）ボタン押下時の処理。
        /// 入力・選択がされていない場合は「閉じる」として機能します。
        /// </summary>
        private void OnSave()
        {
            // 入力・選択がされていない場合は「閉じる」アクション
            if (!_isInputStarted)
            {
                RequestClose?.Invoke();
                return;
            }

            // 必須入力チェック
            if (InputCode == null || string.IsNullOrWhiteSpace(InputName))
            {
                MessageBox.Show("コードと名称を入力してください。", "入力確認", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 保存データ準備
            var jobToSave = new Job
            {
                C_JobID = SelectedJob?.C_JobID ?? 0, // 既存ならIDを引き継ぎ、新規なら0
                C_JobCode = InputCode,
                C_JobName = InputName,
                C_JobAbbreviationName = InputAbbreviation
            };

            // 最終確認ダイアログ
            var result = MessageBox.Show("登録します。よろしいですか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                // リポジトリを通じてDBに保存
                _repository.Save(jobToSave);
                
                MessageBox.Show("登録しました。", "情報", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // 一覧を最新化し、入力をクリア
                RefreshList();
                ClearFields(); 
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存に失敗しました：\n{ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 削除（F7）ボタン押下時の処理。
        /// </summary>
        private void OnDelete()
        {
            if (SelectedJob == null) return;

            // 削除確認
            var result = MessageBox.Show($"職種「{SelectedJob.C_JobName}」を削除しますか？", "削除確認", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _repository.Delete(SelectedJob.C_JobID);
                    MessageBox.Show("削除しました。", "情報", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    RefreshList();
                    ClearFields();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"削除に失敗しました：\n{ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// 中止（F10）ボタン押下時の処理。
        /// </summary>
        private void OnCancel()
        {
            // 入力がある場合は破棄の確認
            if (_isInputStarted)
            {
                var result = MessageBox.Show("入力内容を破棄します。よろしいですか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result != MessageBoxResult.Yes) return;
            }

            ClearFields();
        }

        /// <summary>
        /// 入力フィールドや選択をクリアし、初期状態に戻します。
        /// </summary>
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

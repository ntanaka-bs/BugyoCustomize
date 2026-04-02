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
    /// メイン画面用ViewModel
    /// </summary>
    public class TimeZoneViewModel : ViewModelBase
    {
        /// <summary>リポジトリ</summary>
        private readonly TimeZoneRepository _repository;
        /// <summary>現在修正対象の項目</summary>
        private TimeZoneModel? _selectedTimeZone;
        /// <summary>一覧で選択(ハイライト)されている項目</summary>
        private TimeZoneModel? _highlightedTimeZone;
        /// <summary>入力値: コード</summary>
        private int? _inputCode;
        /// <summary>入力値: 名称</summary>
        private string? _inputName;
        /// <summary>入力値: 略称</summary>
        private string? _inputAbbreviation;
        /// <summary>入力値: 出力順</summary>
        private int? _inputOrderCode;
        /// <summary>コード項目の活性状態</summary>
        private bool _isCodeEnabled = true;
        /// <summary>入力開始フラグ</summary>
        private bool _isInputStarted;

        /// <summary>
        /// 画面一覧用コレクション
        /// </summary>
        public ObservableCollection<TimeZoneModel> TimeZones { get; } = new ObservableCollection<TimeZoneModel>();

        /// <summary>
        /// グリッド上で現在ハイライトされている行
        /// </summary>
        public TimeZoneModel? HighlightedTimeZone
        {
            get => _highlightedTimeZone;
            set => SetProperty(ref _highlightedTimeZone, value);
        }

        /// <summary>
        /// 操作ボタン(登録・中止・削除など)の表示可否
        /// </summary>
        public bool IsActionButtonsVisible => _isInputStarted;

        /// <summary>
        /// F12ボタンの表示テキスト(登録または閉じる)
        /// </summary>
        public string SaveOrCloseButtonText => _isInputStarted ? "登録" : "閉じる";

        /// <summary>
        /// 画面のClose処理呼び出し用イベント
        /// </summary>
        public event Action? RequestClose;

        /// <summary>
        /// 現在編集対象として読み込まれている行
        /// </summary>
        public TimeZoneModel? SelectedTimeZone
        {
            get => _selectedTimeZone;
            set
            {
                if (SetProperty(ref _selectedTimeZone, value) && _selectedTimeZone != null)
                {
                    // ハイライトも同期
                    HighlightedTimeZone = _selectedTimeZone;
                    
                    _inputCode = _selectedTimeZone.C_TimeZoneCode;
                    _inputName = _selectedTimeZone.C_TimeZoneName;
                    _inputAbbreviation = _selectedTimeZone.C_TimeZoneAbbreviationName;
                    _inputOrderCode = _selectedTimeZone.C_OrderCode;
                    
                    OnPropertyChanged(nameof(InputCode));
                    OnPropertyChanged(nameof(InputName));
                    OnPropertyChanged(nameof(InputAbbreviation));
                    OnPropertyChanged(nameof(InputOrderCode));

                    // 修正モード時はキー入力不可
                    IsCodeEnabled = false;
                    UpdateInputStatus(true);
                }
            }
        }

        /// <summary>
        /// 入力: コード
        /// </summary>
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

        /// <summary>
        /// 入力: 名称
        /// </summary>
        public string? InputName
        {
            get => _inputName;
            set
            {
                if (SetProperty(ref _inputName, value))
                {
                    UpdateInputStatus(!string.IsNullOrWhiteSpace(value) || HasInputValues());
                    
                    // 略称が未入力なら名称の先頭20文字を自動設定
                    if (string.IsNullOrWhiteSpace(InputAbbreviation) && !string.IsNullOrWhiteSpace(value))
                    {
                        InputAbbreviation = value.Length > 20 ? value.Substring(0, 20) : value; 
                    }
                }
            }
        }

        /// <summary>
        /// 入力: 略称
        /// </summary>
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

        /// <summary>
        /// 入力: 出力順
        /// </summary>
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

        /// <summary>
        /// 入力欄(コード)の活性状態
        /// </summary>
        public bool IsCodeEnabled
        {
            get => _isCodeEnabled;
            set => SetProperty(ref _isCodeEnabled, value);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
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

        /// <summary>
        /// 一覧ダブルクリック時の処理
        /// </summary>
        private void OnDoubleClick()
        {
            if (HighlightedTimeZone != null)
            {
                SelectedTimeZone = HighlightedTimeZone;
            }
        }

        /// <summary>
        /// 登録コマンド (F12)
        /// </summary>
        public ICommand SaveCommand { get; }

        /// <summary>
        /// 削除コマンド (F7)
        /// </summary>
        public ICommand DeleteCommand { get; }

        /// <summary>
        /// キャンセルコマンド (F10)
        /// </summary>
        public ICommand CancelCommand { get; }

        /// <summary>
        /// ダブルクリックコマンド
        /// </summary>
        public ICommand DoubleClickCommand { get; }

        /// <summary>
        /// 入力有無の判定
        /// </summary>
        private bool HasInputValues() => InputCode != null || !string.IsNullOrWhiteSpace(InputName) || !string.IsNullOrWhiteSpace(InputAbbreviation);

        /// <summary>
        /// ボタン表示・テキスト状態の更新
        /// </summary>
        private void UpdateInputStatus(bool started)
        {
            if (_isInputStarted != started)
            {
                _isInputStarted = started;
                OnPropertyChanged(nameof(IsActionButtonsVisible));
                OnPropertyChanged(nameof(SaveOrCloseButtonText));
            }
        }

        /// <summary>
        /// コード入力時に既存データを自動読込
        /// </summary>
        private void CheckAndLoadExistingTimeZone(int code)
        {
            var existing = TimeZones.FirstOrDefault(t => t.C_TimeZoneCode == code);
            if (existing != null && SelectedTimeZone != existing)
            {
                SelectedTimeZone = existing;
            }
        }

        /// <summary>
        /// DB一覧を再読込して画面に反映
        /// </summary>
        private void RefreshList()
        {
            TimeZones.Clear();
            foreach (var tz in _repository.GetAllTimeZones())
            {
                TimeZones.Add(tz);
            }
        }

        /// <summary>
        /// 登録処理
        /// </summary>
        private void OnSave()
        {
            // 入力中ではない場合は画面を閉じる
            if (!_isInputStarted)
            {
                RequestClose?.Invoke();
                return;
            }

            // 未入力チェック
            if (InputCode == null || string.IsNullOrWhiteSpace(InputName) || string.IsNullOrWhiteSpace(InputAbbreviation))
            {
                MessageBox.Show("必須項目（コード、名称、略称）を入力してください", "時間帯区分登録", MessageBoxButton.OK, MessageBoxImage.Warning);
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

            var result = MessageBox.Show("登録します。よろしいですか？", "時間帯区分登録", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                // DB保存実行
                _repository.Save(tzToSave);
                MessageBox.Show("登録しました", "時間帯区分登録", MessageBoxButton.OK, MessageBoxImage.Information);
                RefreshList();
                ClearFields(); 
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存に失敗しました:\n{ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 削除処理
        /// </summary>
        private void OnDelete()
        {
            if (SelectedTimeZone == null) return;

            var result = MessageBox.Show("削除します。よろしいですか？", "時間帯区分登録", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // DB削除実行
                    int delResult = _repository.Delete(SelectedTimeZone.C_TimeZoneID);
                    if (delResult == 1) // 成功
                    {
                        MessageBox.Show("削除しました", "時間帯区分登録", MessageBoxButton.OK, MessageBoxImage.Information);
                        RefreshList();
                        ClearFields();
                    }
                    else if (delResult == 2) // 他テーブルで使用中
                    {
                        MessageBox.Show("このコードは単価登録にて使用中のため、削除できません", "時間帯区分登録", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else if (delResult == 3) // 他テーブルで使用中
                    {
                        MessageBox.Show("このコードは勤怠データにて使用中のため、削除できません", "時間帯区分登録", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"削除に失敗しました:\n{ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// 破棄処理
        /// </summary>
        private void OnCancel()
        {
            if (_isInputStarted)
            {
                var result = MessageBox.Show("入力内容を破棄します。よろしいですか？", "時間帯区分登録", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result != MessageBoxResult.Yes) return;
            }
            ClearFields();
        }

        /// <summary>
        /// プロパティ・画面状態のクリア
        /// </summary>
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

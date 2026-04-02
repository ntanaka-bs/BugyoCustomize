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
    /// 日種類登録画面のロジックを担当する ViewModel クラスです。
    /// 画面の表示状態管理、コマンドの実行、バリデーション等を実装します。
    /// </summary>
    public class DayKindRegistrationViewModel : ViewModelBase
    {
        public event Action? RequestClose;
        private readonly DayKindDatabaseHelper _db;
        
        // 入力フィールドのバッキングフィールド
        private string _codeText = string.Empty;
        private string _nameText = string.Empty;
        private string _abbreviationText = string.Empty;
        private bool _isCodeEnabled = true;
        private DayKind? _selectedDayKind;
        private ObservableCollection<DayKind> _dayKinds = new();
        
        private string _registerButtonText = "閉じる";
        private bool _isDeleteCancelEnabled = false;
        private bool _isEditMode = false;

        /// <summary>
        /// コンストラクタ。DBヘルパーの初期化とコマンドの生成、初期データのロードを行います。
        /// </summary>
        public DayKindRegistrationViewModel()
        {
            _db = new DayKindDatabaseHelper();
            
            // 各コマンドの生成と、実行可否判定ロジックの紐付け
            DeleteCommand = new RelayCommand(ExecuteDelete, CanExecuteDelete);
            CancelCommand = new RelayCommand(ExecuteCancel);
            RegisterCommand = new RelayCommand(ExecuteRegister);
            SelectCommand = new RelayCommand(ExecuteSelect);

            LoadData(); // 初期データの読み込み
        }

        #region バインディングプロパティ (Properties)

        /// <summary>
        /// 日種類コード（画面入力値）
        /// </summary>
        public string CodeText
        {
            get => _codeText;
            set
            {
                // 数字のみを受け付ける（0-99の範囲を想定）
                string numericValue = new string(value.Where(char.IsDigit).ToArray());
                if (numericValue.Length > 2) numericValue = numericValue.Substring(0, 2);
                
                if (SetProperty(ref _codeText, numericValue))
                {
                    OnPropertyChanged(nameof(FormattedCodeText));
                    // 2桁入力された場合、またはフォーカスが外れたタイミングでコード確定処理を行う
                    if (_codeText.Length == 2)
                    {
                        OnCodeConfirmed();
                    }
                }
            }
        }

        /// <summary>
        /// コード確定時の処理（存在チェックとモード切り替え）
        /// </summary>
        private void OnCodeConfirmed()
        {
            if (string.IsNullOrWhiteSpace(CodeText)) return;
            if (!int.TryParse(CodeText, out int code)) return;

            var existing = _db.GetDayKindByCode(code);
            if (existing != null)
            {
                // 既存データがある場合は編集モード
                LoadSelectedDayKind(existing);
            }
            else
            {
                // 新規データの場合も、コード入力後は他フィールドへ
                IsCodeEnabled = false;
                IsEditMode = true;
            }
        }

        /// <summary>
        /// 表示用にゼロ埋めされたコード（2桁）
        /// </summary>
        public string FormattedCodeText
        {
            get
            {
                if (int.TryParse(CodeText, out int code))
                    return code.ToString("D2");
                return CodeText;
            }
        }

        /// <summary>
        /// 日種類名称
        /// </summary>
        public string NameText
        {
            get => _nameText;
            set
            {
                if (SetProperty(ref _nameText, value))
                {
                    // 略称が空の場合、名称から自動的に略称を設定（既存仕様の再現）
                    if (string.IsNullOrWhiteSpace(AbbreviationText) && !string.IsNullOrWhiteSpace(value))
                    {
                        // 最大10文字（10バイト相当）を切り出してセット
                        AbbreviationText = value.Length > 10 ? value.Substring(0, 10) : value;
                    }
                }
            }
        }

        /// <summary>
        /// 日種類略称
        /// </summary>
        public string AbbreviationText
        {
            get => _abbreviationText;
            set => SetProperty(ref _abbreviationText, value);
        }

        /// <summary>
        /// コード入力欄の活性・非活性状態（修正モード時は非活性にする）
        /// </summary>
        public bool IsCodeEnabled
        {
            get => _isCodeEnabled;
            set => SetProperty(ref _isCodeEnabled, value);
        }

        /// <summary>
        /// 一覧リスト (DataGrid) のデータソース
        /// </summary>
        public ObservableCollection<DayKind> DayKinds
        {
            get => _dayKinds;
            set => SetProperty(ref _dayKinds, value);
        }

        /// <summary>
        /// リストで現在選択されている項目
        /// </summary>
        public DayKind? SelectedDayKind
        {
            get => _selectedDayKind;
            set
            {
                if (SetProperty(ref _selectedDayKind, value))
                {
                    if (value != null)
                    {
                        LoadSelectedDayKind(value); // 選択された項目を入力欄に反映
                    }
                    else
                    {
                        // 選択解除（または新規行フォーカス時）はフォームをリセット
                        ResetForm();
                    }
                }
            }
        }

        /// <summary>
        /// 登録ボタンのテキスト（"登録" または "閉じる"）
        /// </summary>
        public string RegisterButtonText
        {
            get => _registerButtonText;
            set => SetProperty(ref _registerButtonText, value);
        }

        /// <summary>
        /// 削除・中止ボタンの活性状態
        /// </summary>
        public bool IsDeleteCancelEnabled
        {
            get => _isDeleteCancelEnabled;
            set => SetProperty(ref _isDeleteCancelEnabled, value);
        }

        /// <summary>
        /// 編集/入力モードかどうか（ボタンの活性切替に使用）
        /// </summary>
        public bool IsEditMode
        {
            get => _isEditMode;
            set
            {
                if (SetProperty(ref _isEditMode, value))
                {
                    RegisterButtonText = value ? "登録" : "閉じる";
                    IsDeleteCancelEnabled = value;
                }
            }
        }

        #endregion

        #region コマンド (Commands)

        public ICommand DeleteCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand RegisterCommand { get; }
        public ICommand SelectCommand { get; }

        #endregion

        #region 実行ロジック (Command Implementation)

        /// <summary>
        /// データの読み込み。DBから最新のリストを取得します。
        /// </summary>
        private void LoadData()
        {
            var list = _db.GetAllDayKinds();
            DayKinds = new ObservableCollection<DayKind>(list);
        }

        /// <summary>
        /// 選択された項目をフォームに表示します（修正モードへ移行）。
        /// </summary>
        private void LoadSelectedDayKind(DayKind dayKind)
        {
            CodeText = dayKind.DayKindCode.ToString();
            NameText = dayKind.DayKindName;
            AbbreviationText = dayKind.DayKindAbbreviationName;
            IsCodeEnabled = false; // 修正時はコード変更不可
            IsEditMode = true;
        }

        /// <summary>
        /// リストのダブルクリックなどで項目を選択した時の処理
        /// </summary>
        private void ExecuteSelect(object? obj)
        {
            if (SelectedDayKind != null)
            {
                LoadSelectedDayKind(SelectedDayKind);
            }
        }

        /// <summary>
        /// 登録処理 (F12) の実行可能判定
        /// </summary>
        private bool CanExecuteRegister(object? obj)
        {
            // コードと名称が入力されていることが条件
            return !string.IsNullOrWhiteSpace(CodeText) && !string.IsNullOrWhiteSpace(NameText);
        }

        /// <summary>
        /// 登録処理 (F12) の本体。テキストが「閉じる」の場合は画面を閉じます。
        /// </summary>
        private void ExecuteRegister(object? obj)
        {
            if (RegisterButtonText == "閉じる")
            {
                // 画面を閉じる
                RequestClose?.Invoke();
                return;
            }

            if (!int.TryParse(CodeText, out int code))
            {
                MessageBox.Show("コードを正しく入力してください。", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(NameText))
            {
                MessageBox.Show("名称を入力してください。", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show("登録します。よろしいですか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var dayKind = new DayKind
                    {
                        DayKindCode = code,
                        DayKindName = NameText,
                        DayKindAbbreviationName = AbbreviationText
                    };

                    _db.SaveDayKind(dayKind);
                    MessageBox.Show("登録しました。", "情報", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    ResetForm();
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"登録処理に失敗しました:\n{ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// 削除処理 (F7) の実行可能判定（修正モード時のみ可能）
        /// </summary>
        private bool CanExecuteDelete(object? obj)
        {
            return !IsCodeEnabled;
        }

        /// <summary>
        /// 削除処理 (F7) の本体。他テーブルでの使用状況を確認した上で実行します。
        /// </summary>
        private void ExecuteDelete(object? obj)
        {
            if (!int.TryParse(CodeText, out int code)) return;

            // 他テーブル（単価・勤怠）で使用されているか確認
            if (_db.IsDayKindUsed(code))
            {
                MessageBox.Show("この日種類は他のデータで使用されているため、削除できません。", "削除不可", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            var result = MessageBox.Show("削除します。よろしいですか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _db.DeleteDayKind(code);
                    MessageBox.Show("削除しました。", "情報", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    ResetForm();
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"削除処理に失敗しました:\n{ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// 中止処理 (F10) の本体。入力内容を破棄し、フォームを初期状態に戻します。
        /// </summary>
        private void ExecuteCancel(object? obj)
        {
            ResetForm();
        }

        /// <summary>
        /// 入力フォームを初期化（空の状態）にします。
        /// </summary>
        private void ResetForm()
        {
            CodeText = string.Empty;
            NameText = string.Empty;
            AbbreviationText = string.Empty;
            IsCodeEnabled = true;
            IsEditMode = false;
            SelectedDayKind = null;
        }

        #endregion
    }
}

using System;
using AttendanceSystem.Common;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using AttendanceSystem.Models;
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
            // リポジトリ初期化
            _repository = new UnitPriceRepository();
            
            // コマンド生成
            SearchCommand = new RelayCommand(ExecuteSearch);
            CancelCommand = new RelayCommand(ExecuteCancel, CanExecuteCancel);
            DeleteRowCommand = new RelayCommand(ExecuteDeleteRow, CanExecuteDeleteRow);
            F12Command = new RelayCommand(ExecuteF12);

            // リスト変更時に表示状態（件数・ボタン名等）を更新
            Details.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(HasData));
                OnPropertyChanged(nameof(F7Text));
                OnPropertyChanged(nameof(F10Text));
                OnPropertyChanged(nameof(F12Text));
                CommandManager.InvalidateRequerySuggested();
            };

            // DB初期化確認
            try
            {
                UnitPriceDatabaseHelper.EnsureTablesCreated();
            }
            catch (Exception ex)
            {
                MessageBox.Show("DB初期化エラー: " + ex.Message, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string _jobCode;
        /// <summary>
        /// 検索条件：職種コード
        /// </summary>
        public string JobCode
        {
            get => _jobCode;
            set
            {
                if (SetProperty(ref _jobCode, value)) UpdateJobName();
            }
        }

        private string _jobName;
        /// <summary>
        /// 検索条件：職種名称（コード入力時に自動取得）
        /// </summary>
        public string JobName { get => _jobName; set => SetProperty(ref _jobName, value); }

        private string _dayKindCode;
        /// <summary>
        /// 検索条件：日種類コード
        /// </summary>
        public string DayKindCode
        {
            get => _dayKindCode;
            set
            {
                if (SetProperty(ref _dayKindCode, value)) UpdateDayKindName();
            }
        }

        private string _dayKindName;
        /// <summary>
        /// 検索条件：日種類名称（コード入力時に自動取得）
        /// </summary>
        public string DayKindName { get => _dayKindName; set => SetProperty(ref _dayKindName, value); }

        private string _timeZoneCode;
        /// <summary>
        /// 検索条件：時間帯コード
        /// </summary>
        public string TimeZoneCode
        {
            get => _timeZoneCode;
            set
            {
                if (SetProperty(ref _timeZoneCode, value)) UpdateTimeZoneName();
            }
        }

        private string _timeZoneName;
        /// <summary>
        /// 検索条件：時間帯名称（コード入力時に自動取得）
        /// </summary>
        public string TimeZoneName { get => _timeZoneName; set => SetProperty(ref _timeZoneName, value); }

        /// <summary>
        /// 一覧表示用単価明細コレクション
        /// </summary>
        public ObservableCollection<UnitPriceDetail> Details { get; } = new ObservableCollection<UnitPriceDetail>();

        private UnitPriceDetail _selectedDetail;
        /// <summary>
        /// DataGrid選択行データ
        /// </summary>
        public UnitPriceDetail SelectedDetail { get => _selectedDetail; set => SetProperty(ref _selectedDetail, value); }

        /// <summary>
        /// データ表示有無フラグ
        /// </summary>
        public bool HasData => Details.Any();

        /// <summary>
        /// F7ボタンテキスト
        /// </summary>
        public string F7Text => HasData ? "行削除" : "";

        /// <summary>
        /// F10ボタンテキスト
        /// </summary>
        public string F10Text => HasData ? "中止" : "";

        /// <summary>
        /// F12ボタンテキスト
        /// </summary>
        public string F12Text => HasData ? "登録" : "閉じる";

        /// <summary>
        /// 検索ボタン実行
        /// </summary>
        public ICommand SearchCommand { get; }
        /// <summary>
        /// F12ボタン実行（データ有：登録、無：閉じる）
        /// </summary>
        public ICommand F12Command { get; }
        /// <summary>
        /// 中止(F10)ボタン実行
        /// </summary>
        public ICommand CancelCommand { get; }
        /// <summary>
        /// 行削除(F7)ボタン実行
        /// </summary>
        public ICommand DeleteRowCommand { get; }

        /// <summary>
        /// 職種コードから職種名称をDB取得・設定
        /// </summary>
        private void UpdateJobName()
        {
            // パース成功ならDB照会、失敗ならクリア
            if (int.TryParse(JobCode, out int code))
                JobName = _repository.GetJobName(code);
            else
                JobName = string.Empty;
        }

        /// <summary>
        /// 日種類コードから日種類名称をDB取得・設定
        /// </summary>
        private void UpdateDayKindName()
        {
            // パース成功ならDB照会、失敗ならクリア
            if (int.TryParse(DayKindCode, out int code))
                DayKindName = _repository.GetDayKindName(code);
            else
                DayKindName = string.Empty;
        }

        /// <summary>
        /// 時間帯コードから時間帯名称をDB取得・設定
        /// </summary>
        private void UpdateTimeZoneName()
        {
            // パース成功ならDB照会、失敗ならクリア
            if (int.TryParse(TimeZoneCode, out int code))
                TimeZoneName = _repository.GetTimeZoneName(code);
            else
                TimeZoneName = string.Empty;
        }

        /// <summary>
        /// 検索実行。入力条件をもとにDBから明細リストを抽出展開
        /// </summary>
        private void ExecuteSearch(object obj)
        {
            // 必須入力確認（職種コード）
            if (string.IsNullOrWhiteSpace(JobCode) || !int.TryParse(JobCode, out int jCode))
            {
                MessageBox.Show("職種コードを正しく入力してください。", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 任意条件のパース
            int? dCode = int.TryParse(DayKindCode, out int d) ? d : (int?)null;
            int? tCode = int.TryParse(TimeZoneCode, out int t) ? t : (int?)null;

            try
            {
                // DBから明細取得
                var list = _repository.GetDetailData(jCode, dCode, tCode);
                
                // 既存明細クリア後、新規追加
                Details.Clear();
                foreach (var item in list) Details.Add(item);
            }
            catch (Exception ex)
            {
                MessageBox.Show("検索エラー: " + ex.Message, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// F12ボタン処理（データ有無で処理分岐）
        /// </summary>
        private void ExecuteF12(object obj)
        {
            // データ有なら登録処理、無なら画面終了処理
            if (HasData)
            {
                ExecuteSave();
            }
            else
            {
                // 終了前のユーザー確認
                var res = MessageBox.Show("画面を閉じますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes)
                {
                    RequestClose?.Invoke();
                }
            }
        }

        /// <summary>
        /// 登録実行。単価・時間等の入力数値をDBへ一括保存・更新
        /// </summary>
        private void ExecuteSave()
        {
            try
            {
                // リストをループしてDBへ保存
                foreach (var detail in Details)
                {
                    // 未設定（新規無入力）はスキップ、入力有または値変更済データのみ処理
                    if (detail.UnitPrice > 0 || detail.UnitPriceID > 0)
                    {
                        _repository.SaveUnitPriceData(detail);
                    }
                }
                
                // 完了報告
                MessageBox.Show("登録が完了しました。", "完了", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // 最新状態へ再検索
                ExecuteSearch(null); 
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存エラー: " + ex.Message, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 中止処理実行可否判定（検索結果が存在する場合に許可）
        /// </summary>
        private bool CanExecuteCancel(object obj) => HasData;

        /// <summary>
        /// 中止処理実行。検索結果および検索条件をクリア
        /// </summary>
        private void ExecuteCancel(object obj)
        {
            // 明細をクリア（F7,F10等連動して非表示となる）
            Details.Clear();
            
            // 検索条件テキストボックス類を空化
            JobCode = ""; JobName = "";
            DayKindCode = ""; DayKindName = "";
            TimeZoneCode = ""; TimeZoneName = "";
        }

        /// <summary>
        /// 行削除処理実行可否判定（選択行が登録済データの場合に許可）
        /// </summary>
        private bool CanExecuteDeleteRow(object obj) => HasData && SelectedDetail != null && SelectedDetail.UnitPriceID > 0;

        /// <summary>
        /// 行削除実行。選択対象の単価レコードをDBから削除
        /// </summary>
        private void ExecuteDeleteRow(object obj)
        {
            if (SelectedDetail == null || SelectedDetail.UnitPriceID <= 0) return;

            // 削除前のユーザー確認
            var res = MessageBox.Show("選択した行の単価データを削除しますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.Yes)
            {
                try
                {
                    // 対象データをDBから物理削除
                    _repository.DeleteUnitPriceData(SelectedDetail.UnitPriceID);
                    
                    // 最新状態へ再検索
                    ExecuteSearch(null); 
                }
                catch (Exception ex)
                {
                    MessageBox.Show("削除エラー: " + ex.Message, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}

using System;
using System.Windows;
using System.Windows.Input;
using AttendanceSystem.Common;
using AttendanceSystem.Models;

namespace AttendanceSystem.ViewModels
{
    /// <summary>
    /// 日毎勤怠チェックリスト画面の ViewModel クラスです。
    /// 印刷・プレビュー対象の期間やフィルタ条件（センター・職種コード等）を保持し、
    /// 「プレビュー」「印刷」「中止」の各コマンドを実装します。
    /// </summary>
    public class DailyAttendanceCheckListViewModel : ViewModelBase
    {
        #region フィールド

        // リスト種別の初期値: 0=チェックリスト、1=注意リスト、2=計算前リスト
        private int _listType = 0;

        // 検索期間（開始・終了）。コンストラクタでシステム前月の値を設定する
        private DateTime _startDate;
        private DateTime _endDate;

        // 各種コード範囲の初期値は空文字（入力なし＝絞り込みなし）
        private string _centerCodeStart = string.Empty;
        private string _centerCodeEnd = string.Empty;
        private string _jobCodeStart = string.Empty;
        private string _jobCodeEnd = string.Empty;
        private string _dayKindCodeStart = string.Empty;
        private string _dayKindCodeEnd = string.Empty;
        private string _timeZoneCodeStart = string.Empty;
        private string _timeZoneCodeEnd = string.Empty;

        // 出務状況フィルタの初期値: 0=全て、1=有、2=無
        private int _attendanceStatus = 0;

        #endregion

        #region プロパティ

        /// <summary>
        /// 出力するリストの種別を表します。
        /// 0: チェックリスト（全体確認用）、1: 注意リスト（エラーフラグ有のみ）、2: 計算前リスト
        /// </summary>
        public int ListType
        {
            get => _listType;
            set => SetProperty(ref _listType, value);
        }

        /// <summary>
        /// 抽出期間の開始日付です。前月月初が初期値として設定されます。
        /// </summary>
        public DateTime StartDate
        {
            get => _startDate;
            set => SetProperty(ref _startDate, value);
        }

        /// <summary>
        /// 抽出期間の終了日付です。前月末日が初期値として設定されます。
        /// </summary>
        public DateTime EndDate
        {
            get => _endDate;
            set => SetProperty(ref _endDate, value);
        }

        /// <summary>センターコード範囲の開始値。空文字の場合は下限なし。</summary>
        public string CenterCodeStart
        {
            get => _centerCodeStart;
            set => SetProperty(ref _centerCodeStart, value);
        }

        /// <summary>センターコード範囲の終了値。空文字の場合は上限なし。</summary>
        public string CenterCodeEnd
        {
            get => _centerCodeEnd;
            set => SetProperty(ref _centerCodeEnd, value);
        }

        /// <summary>職種コード範囲の開始値。</summary>
        public string JobCodeStart
        {
            get => _jobCodeStart;
            set => SetProperty(ref _jobCodeStart, value);
        }

        /// <summary>職種コード範囲の終了値。</summary>
        public string JobCodeEnd
        {
            get => _jobCodeEnd;
            set => SetProperty(ref _jobCodeEnd, value);
        }

        /// <summary>日種類コード範囲の開始値。</summary>
        public string DayKindCodeStart
        {
            get => _dayKindCodeStart;
            set => SetProperty(ref _dayKindCodeStart, value);
        }

        /// <summary>日種類コード範囲の終了値。</summary>
        public string DayKindCodeEnd
        {
            get => _dayKindCodeEnd;
            set => SetProperty(ref _dayKindCodeEnd, value);
        }

        /// <summary>時間帯コード範囲の開始値。</summary>
        public string TimeZoneCodeStart
        {
            get => _timeZoneCodeStart;
            set => SetProperty(ref _timeZoneCodeStart, value);
        }

        /// <summary>時間帯コード範囲の終了値。</summary>
        public string TimeZoneCodeEnd
        {
            get => _timeZoneCodeEnd;
            set => SetProperty(ref _timeZoneCodeEnd, value);
        }

        /// <summary>
        /// 出務状況によるフィルタ条件を表します。
        /// 0: 全て、1: 出務あり、2: 欠務（出務なし）
        /// </summary>
        public int AttendanceStatus
        {
            get => _attendanceStatus;
            set => SetProperty(ref _attendanceStatus, value);
        }

        // =============================================
        // プリンタ設定用プロパティ群
        // =============================================

        // 出力先プリンタ名。初期値は「通常使うプリンター」
        private string _selectedPrinter = "通常使うプリンター";
        /// <summary>印刷先プリンタ名。</summary>
        public string SelectedPrinter
        {
            get => _selectedPrinter;
            set => SetProperty(ref _selectedPrinter, value);
        }

        // 用紙サイズ。初期値は A4
        private string _paperSize = "A4";
        /// <summary>用紙サイズ（例: "A4"）。</summary>
        public string PaperSize
        {
            get => _paperSize;
            set => SetProperty(ref _paperSize, value);
        }

        // 印刷方向フラグ: true=横(Landscape)、false=縦(Portrait)。初期値は縦
        private bool _isLandscape = false;
        /// <summary>true の場合は横向き（Landscape）、false の場合は縦向き（Portrait）で印刷します。</summary>
        public bool IsLandscape
        {
            get => _isLandscape;
            set => SetProperty(ref _isLandscape, value);
        }

        // =============================================
        // 日付大小チェック用エラーフラグ
        // 開始日 > 終了日 の逆転状態のとき true になり、
        // View 側の DataTrigger が両 DatePicker を赤枠で表示する
        // =============================================
        private bool _isDateRangeError = false;
        /// <summary>
        /// 日付範囲のエラー状態です。開始日が終了日より後の場合に true になります。
        /// View 側の DataTrigger により、DatePicker が赤枠でハイライトされます。
        /// </summary>
        public bool IsDateRangeError
        {
            get => _isDateRangeError;
            set => SetProperty(ref _isDateRangeError, value);
        }

        #endregion

        #region コマンド

        /// <summary>「プレビュー」ボタンに対応するコマンド。データを抽出して件数を表示します。</summary>
        public ICommand PreviewCommand { get; }
        /// <summary>「印刷」ボタンに対応するコマンド（現在は未実装）。</summary>
        public ICommand PrintCommand { get; }
        /// <summary>「中止」ボタンに対応するコマンド。画面を閉じます。</summary>
        public ICommand CancelCommand { get; }

        #endregion

        /// <summary>
        /// コンストラクタ。画面の初期値（前月の期間）を設定し、各コマンドを登録します。
        /// </summary>
        public DailyAttendanceCheckListViewModel()
        {
            // 当月1日を算出し、開始日を前月月初・終了日を前月月末に設定する
            var firstDayOfThisMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            _startDate = firstDayOfThisMonth.AddMonths(-1); // 前月月初（例: 本日 2026/04/15 → 2026/03/01）
            _endDate   = firstDayOfThisMonth.AddDays(-1);   // 前月月末（例: 本日 2026/04/15 → 2026/03/31）

            PreviewCommand = new DelegateCommand(ExecutePreview);
            PrintCommand   = new DelegateCommand(ExecutePrint);
            CancelCommand  = new DelegateCommand(ExecuteCancel);
        }

        /// <summary>
        /// 「プレビュー」ボタンが押された時の処理です。
        /// 1. 入力された日付の大小関係を検証します（開始日 ≦ 終了日）。
        /// 2. 検証が通れば、リポジトリ経由でデータベースから勤怠データを抽出します。
        /// 3. 取得件数を一時的にダイアログで表示します（帳票出力機能の実装前の暫定動作）。
        /// </summary>
        private void ExecutePreview()
        {
            // 開始日と終了日の両方が入力済みの場合のみ大小チェックを行う
            // DatePicker の値は常に null でなく DateTime 型で保持されるが、
            // MinValue（0001/01/01）のときは「未入力」とみなす
            bool startEntered = StartDate != DateTime.MinValue;
            bool endEntered   = EndDate   != DateTime.MinValue;

            if (startEntered && endEntered && StartDate > EndDate)
            {
                // 開始日が終了日より後の場合はエラーフラグを立てて両日付欄を赤枠で強調し、処理を中断する
                IsDateRangeError = true;
                MessageBox.Show("開始日付は終了日付以前の日付を入力してください。", "入力エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // チェック正常のためエラーフラグをリセットする（赤枠を解除する）
            IsDateRangeError = false;

            try
            {
                // リポジトリを通じてデータベースから詳細な勤怠データを取得する
                var repo = new DailyAttendanceRepository();
                var dt = repo.GetAttendanceDataDetailed(
                    ListType,
                    StartDate,
                    EndDate,
                    CenterCodeStart, CenterCodeEnd,
                    JobCodeStart, JobCodeEnd,
                    DayKindCodeStart, DayKindCodeEnd,
                    TimeZoneCodeStart, TimeZoneCodeEnd,
                    AttendanceStatus);

                int count = dt.Rows.Count;

                // リスト種別名を日本語に変換してメッセージに表示する
                string listTypeName = ListType switch
                {
                    0 => "チェックリスト",
                    1 => "注意リスト",
                    2 => "計算前リスト",
                    _ => "不明"
                };

                // 現状は抽出結果の概要を表示（本印刷機能は後続で実装）
                MessageBox.Show($"プレビュー抽出を実行しました。\n" +
                                $"期間: {StartDate:yyyy/MM/dd} ～ {EndDate:yyyy/MM/dd}\n" +
                                $"リストタイプ: {listTypeName}\n" +
                                $"抽出データ件数: {count} 件\n" +
                                $"(帳票出力機能で詳細を表示します)",
                                "プレビュー結果", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                // データベースアクセス中に例外が発生した場合はエラーメッセージを表示する
                MessageBox.Show($"データ抽出中にエラーが発生しました。\n{ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 「印刷」ボタンが押された時の処理です。
        /// 現在は印刷機能が未実装のため、通知メッセージを表示します。
        /// </summary>
        private void ExecutePrint()
        {
            // 印刷機能は未実装のため、暫定的に通知メッセージを表示する
            MessageBox.Show("印刷機能は未実装です。", "通知", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        /// <summary>
        /// 「中止」ボタンが押された時の処理です。
        /// この ViewModel を DataContext として持つウィンドウを検索して閉じます。
        /// </summary>
        private void ExecuteCancel()
        {
            // DataContext が自身と一致するウィンドウを探し、閉じる
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext == this)
                {
                    window.Close();
                    break;
                }
            }
        }
    }
}

using System;
using System.Windows;
using System.Windows.Input;
using AttendanceSystem.Common;
using AttendanceSystem.Models;

namespace AttendanceSystem.ViewModels
{
    /// <summary>
    /// 日毎勤怠チェックリスト画面の ViewModel
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
        private string _centerCodeStart = "";
        private string _centerCodeEnd = "";
        private string _jobCodeStart = "";
        private string _jobCodeEnd = "";
        private string _dayKindCodeStart = "";
        private string _dayKindCodeEnd = "";
        private string _timeZoneCodeStart = "";
        private string _timeZoneCodeEnd = "";

        // 出務状況フィルタの初期値: 0=全て、1=有、2=無
        private int _attendanceStatus = 0;
        #endregion

        #region プロパティ
        public int ListType
        {
            get => _listType;
            set => SetProperty(ref _listType, value);
        }

        public DateTime StartDate
        {
            get => _startDate;
            set => SetProperty(ref _startDate, value);
        }

        public DateTime EndDate
        {
            get => _endDate;
            set => SetProperty(ref _endDate, value);
        }

        public string CenterCodeStart
        {
            get => _centerCodeStart;
            set => SetProperty(ref _centerCodeStart, value);
        }

        public string CenterCodeEnd
        {
            get => _centerCodeEnd;
            set => SetProperty(ref _centerCodeEnd, value);
        }

        public string JobCodeStart
        {
            get => _jobCodeStart;
            set => SetProperty(ref _jobCodeStart, value);
        }

        public string JobCodeEnd
        {
            get => _jobCodeEnd;
            set => SetProperty(ref _jobCodeEnd, value);
        }

        public string DayKindCodeStart
        {
            get => _dayKindCodeStart;
            set => SetProperty(ref _dayKindCodeStart, value);
        }

        public string DayKindCodeEnd
        {
            get => _dayKindCodeEnd;
            set => SetProperty(ref _dayKindCodeEnd, value);
        }

        public string TimeZoneCodeStart
        {
            get => _timeZoneCodeStart;
            set => SetProperty(ref _timeZoneCodeStart, value);
        }

        public string TimeZoneCodeEnd
        {
            get => _timeZoneCodeEnd;
            set => SetProperty(ref _timeZoneCodeEnd, value);
        }

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
        public string SelectedPrinter
        {
            get => _selectedPrinter;
            set => SetProperty(ref _selectedPrinter, value);
        }

        // 用紙サイズ。初期値は A4
        private string _paperSize = "A4";
        public string PaperSize
        {
            get => _paperSize;
            set => SetProperty(ref _paperSize, value);
        }

        // 印刷方向フラグ: true=横(Landscape)、false=縦(Portrait)。初期値は縦
        private bool _isLandscape = false;
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
        public bool IsDateRangeError
        {
            get => _isDateRangeError;
            set => SetProperty(ref _isDateRangeError, value);
        }
        #endregion

        #region コマンド
        public ICommand PreviewCommand { get; }
        public ICommand PrintCommand { get; }
        public ICommand CancelCommand { get; }
        #endregion

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
                // リポジトリを通じてデータベースから勤怠データ件数を取得する
                var repo  = new DailyAttendanceRepository();
                int count = repo.GetAttendanceCount(StartDate, EndDate, JobCodeStart, JobCodeEnd, DayKindCodeStart, DayKindCodeEnd);

                // 現状はデータ抽出件数を確認用メッセージボックスで表示する（本印刷機能は実装予定）
                MessageBox.Show($"プレビュー抽出を実行しました。\n" +
                                $"期間: {StartDate:yyyy/MM/dd} ～ {EndDate:yyyy/MM/dd}\n" +
                                $"リストタイプ: {ListType}\n" +
                                $"抽出データ件数: {count} 件\n" +
                                $"(後続の印刷機能等でこのデータを帳票に出力します)",
                                "プレビュー結果", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                // データベースアクセス中に例外が発生した場合はエラーメッセージを表示する
                MessageBox.Show($"データ抽出中にエラーが発生しました。\n{ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecutePrint()
        {
            // 印刷機能は未実装のため、暫定的に通知メッセージを表示する
            MessageBox.Show("印刷機能は未実装です。", "通知", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

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

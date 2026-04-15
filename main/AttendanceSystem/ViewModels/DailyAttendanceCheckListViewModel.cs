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
        private int _listType = 0; // 0: チェックリスト, 1: 注意リスト, 2: 計算前リスト
        private DateTime _startDate = new DateTime(2013, 12, 1);
        private DateTime _endDate = new DateTime(2013, 12, 31);
        private string _centerCodeStart = "最初";
        private string _centerCodeEnd = "最後";
        private string _jobCodeStart = "最初";
        private string _jobCodeEnd = "最後";
        private string _dayKindCodeStart = "最初";
        private string _dayKindCodeEnd = "最後";
        private string _timeZoneCodeStart = "最初";
        private string _timeZoneCodeEnd = "最後";
        private int _attendanceStatus = 0; // 0: 全て, 1: 有, 2: 無
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

        // プリンタ設定用のプロパティ群
        private string _selectedPrinter = "通常使うプリンター";
        public string SelectedPrinter
        {
            get => _selectedPrinter;
            set => SetProperty(ref _selectedPrinter, value);
        }

        private string _paperSize = "A4";
        public string PaperSize
        {
            get => _paperSize;
            set => SetProperty(ref _paperSize, value);
        }

        private bool _isLandscape = false;
        public bool IsLandscape
        {
            get => _isLandscape;
            set => SetProperty(ref _isLandscape, value);
        }
        #endregion

        #region コマンド
        public ICommand PreviewCommand { get; }
        public ICommand PrintCommand { get; }
        public ICommand CancelCommand { get; }
        #endregion

        public DailyAttendanceCheckListViewModel()
        {
            PreviewCommand = new DelegateCommand(ExecutePreview);
            PrintCommand = new DelegateCommand(ExecutePrint);
            CancelCommand = new DelegateCommand(ExecuteCancel);
        }

        private void ExecutePreview()
        {
            try
            {
                var repo = new DailyAttendanceRepository();
                int count = repo.GetAttendanceCount(StartDate, EndDate, JobCodeStart, JobCodeEnd, DayKindCodeStart, DayKindCodeEnd);
                
                // 現状はデータ抽出が成功するかどうかの確認として、取得した件数を表示します
                MessageBox.Show($"プレビュー抽出を実行しました。\n" +
                                $"期間: {StartDate:yyyy/MM/dd} ～ {EndDate:yyyy/MM/dd}\n" +
                                $"リストタイプ: {ListType}\n" +
                                $"抽出データ件数: {count} 件\n" +
                                $"(後続の印刷機能等でこのデータを帳票に出力します)", 
                                "プレビュー結果", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"データ抽出中にエラーが発生しました。\n{ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecutePrint()
        {
            MessageBox.Show("印刷機能は未実装です。", "通知", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void ExecuteCancel()
        {
            // Window を閉じる処理は通常 View 側で行うが、
            // ViewModel から通知を送るか、View のコードビハインドで処理する
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

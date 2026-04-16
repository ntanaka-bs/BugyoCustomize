using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Input;
using AttendanceSystem.Common;
using AttendanceSystem.Models;

namespace AttendanceSystem.ViewModels
{
    /// <summary>
    /// 勤怠データ入力画面の ViewModel クラスです。
    /// 日付・センター・日種類・時間帯の検索条件を管理し、
    /// 勤怠明細の表示・登録・削除を制御します。
    /// </summary>
    public class AttendanceRegistrationViewModel : ViewModelBase
    {
        // データアクセスを担当するリポジトリ
        private readonly AttendanceRegistrationRepository _repository;

        #region 入力プロパティ

        // 勤怠登録対象の日付（初期値は今日）
        private DateTime _attendanceDate = DateTime.Today;
        /// <summary>
        /// 勤怠登録対象の日付。変更するとリストや明細の再表示が必要になります。
        /// </summary>
        public DateTime AttendanceDate
        {
            get => _attendanceDate;
            set => SetProperty(ref _attendanceDate, value);
        }

        // センターコードの入力値（文字列）
        private string _centerCode = string.Empty;
        /// <summary>
        /// センターコード入力欄の値。変更時にマスタを参照して名称を自動表示します。
        /// </summary>
        public string CenterCode
        {
            get => _centerCode;
            set
            {
                if (SetProperty(ref _centerCode, value))
                    OnCenterCodeChanged(); // コード変更時に名称を自動取得
            }
        }

        private string _centerName = string.Empty;
        /// <summary>センターコードに対応する名称（入力補助用の表示専用プロパティ）</summary>
        public string CenterName
        {
            get => _centerName;
            set => SetProperty(ref _centerName, value);
        }

        // センターの内部ID（DB検索用、画面には非表示）
        private int _centerId;

        // 日種類コードの入力値
        private string _dayKindCode = string.Empty;
        /// <summary>
        /// 日種類コード入力欄の値。変更時にマスタを参照して名称を自動表示します。
        /// </summary>
        public string DayKindCode
        {
            get => _dayKindCode;
            set
            {
                if (SetProperty(ref _dayKindCode, value))
                    OnDayKindCodeChanged(); // コード変更時に名称を自動取得
            }
        }

        private string _dayKindName = string.Empty;
        /// <summary>日種類コードに対応する名称（表示専用）</summary>
        public string DayKindName
        {
            get => _dayKindName;
            set => SetProperty(ref _dayKindName, value);
        }

        // 日種類の内部ID（DB検索用）
        private int _dayKindId;

        // 時間帯コードの入力値
        private string _timeZoneCode = string.Empty;
        /// <summary>
        /// 時間帯コード入力欄の値。変更時にマスタを参照して名称を自動表示します。
        /// </summary>
        public string TimeZoneCode
        {
            get => _timeZoneCode;
            set
            {
                if (SetProperty(ref _timeZoneCode, value))
                    OnTimeZoneCodeChanged(); // コード変更時に名称を自動取得
            }
        }

        private string _timeZoneName = string.Empty;
        /// <summary>時間帯コードに対応する名称（表示専用）</summary>
        public string TimeZoneName
        {
            get => _timeZoneName;
            set => SetProperty(ref _timeZoneName, value);
        }

        // 時間帯の内部ID（DB検索用）
        private int _timeZoneId;

        #endregion

        #region グリッドデータ

        // 画面右上の「選択リスト」（指定日付に登録済みの組み合わせ一覧）
        private DataView? _selectionListView;
        /// <summary>
        /// 右上の選択リストグリッドに表示するデータ。
        /// 指定日付に既に登録されているセンター・日種類・時間帯の組み合わせを一覧表示します。
        /// </summary>
        public DataView? SelectionListView
        {
            get => _selectionListView;
            set => SetProperty(ref _selectionListView, value);
        }

        // メイングリッドに表示する勤怠明細行のコレクション
        private ObservableCollection<AttendanceDetailItem> _employeeDetails = new();
        /// <summary>
        /// メイングリッドに表示する勤怠明細リスト。
        /// 「検索」ボタン押下後に、条件に合致する勤怠レコードが格納されます。
        /// </summary>
        public ObservableCollection<AttendanceDetailItem> EmployeeDetails
        {
            get => _employeeDetails;
            set => SetProperty(ref _employeeDetails, value);
        }

        // メイングリッドで選択中の行
        private AttendanceDetailItem? _selectedEmployee;
        /// <summary>メイングリッドで現在選択中の行データ。行削除コマンドの対象になります。</summary>
        public AttendanceDetailItem? SelectedEmployee
        {
            get => _selectedEmployee;
            set => SetProperty(ref _selectedEmployee, value);
        }

        #endregion

        #region コマンド

        /// <summary>「リスト」ボタン (L) に対応するコマンド。選択リストを再取得します。</summary>
        public ICommand ListCommand { get; }
        /// <summary>「検索」ボタン (S) に対応するコマンド。条件に合致する明細を検索してグリッドに表示します。</summary>
        public ICommand SearchCommand { get; }
        /// <summary>「登録」ボタン (F12) に対応するコマンド。グリッドの内容をデータベースに保存します。</summary>
        public ICommand SaveCommand { get; }
        /// <summary>「行削除」ボタン (F7) に対応するコマンド。選択行のデータを削除します。</summary>
        public ICommand DeleteRowCommand { get; }
        /// <summary>「中止」ボタン (F10) に対応するコマンド。入力内容をクリアして初期状態に戻します。</summary>
        public ICommand CancelCommand { get; }

        #endregion

        /// <summary>
        /// コンストラクタ。リポジトリの初期化、コマンドの登録、
        /// および起動時の選択リスト読み込みを行います。
        /// </summary>
        public AttendanceRegistrationViewModel()
        {
            _repository = new AttendanceRegistrationRepository();

            // 各ボタンに対応するコマンドを登録する
            ListCommand = new RelayCommand(ExecuteList);
            SearchCommand = new RelayCommand(ExecuteSearch);
            SaveCommand = new RelayCommand(ExecuteSave);
            // 行削除は選択行がある場合のみ実行可能
            DeleteRowCommand = new RelayCommand(ExecuteDeleteRow, _ => SelectedEmployee != null);
            CancelCommand = new RelayCommand(ExecuteCancel);

            // 画面表示時に選択リストを初期表示する
            ExecuteList(null!);
        }

        #region ロジック

        /// <summary>
        /// センターコード入力欄の値が変わったときに呼ばれます。
        /// マスタテーブルを検索し、対応するセンター名を名称欄に表示します。
        /// 存在しないコードが入力された場合は「存在しません」と表示します。
        /// </summary>
        private void OnCenterCodeChanged()
        {
            var master = _repository.GetCenterByCode(CenterCode);
            if (master != null)
            {
                _centerId = master.Value.id;
                CenterName = master.Value.name;
            }
            else
            {
                // マスタに存在しないコードが入力された場合
                _centerId = 0;
                CenterName = "存在しません";
            }
        }

        /// <summary>
        /// 日種類コード入力欄の値が変わったときに呼ばれます。
        /// マスタテーブルを検索し、対応する日種類名を名称欄に表示します。
        /// </summary>
        private void OnDayKindCodeChanged()
        {
            var master = _repository.GetDayKindByCode(DayKindCode);
            if (master != null)
            {
                _dayKindId = master.Value.id;
                DayKindName = master.Value.name;
            }
            else
            {
                _dayKindId = 0;
                DayKindName = "存在しません";
            }
        }

        /// <summary>
        /// 時間帯コード入力欄の値が変わったときに呼ばれます。
        /// マスタテーブルを検索し、対応する時間帯名を名称欄に表示します。
        /// </summary>
        private void OnTimeZoneCodeChanged()
        {
            var master = _repository.GetTimeZoneByCode(TimeZoneCode);
            if (master != null)
            {
                _timeZoneId = master.Value.id;
                TimeZoneName = master.Value.name;
            }
            else
            {
                _timeZoneId = 0;
                TimeZoneName = "存在しません";
            }
        }

        /// <summary>
        /// 「リスト」ボタンが押されたときの処理です。
        /// 指定日付に登録済みのセンター・日種類・時間帯の組み合わせを
        /// 画面右上の選択リストグリッドに表示します。
        /// </summary>
        private void ExecuteList(object? obj)
        {
            var dt = _repository.GetSelectionList(AttendanceDate);
            SelectionListView = dt.DefaultView;
        }

        /// <summary>
        /// 「検索」ボタンが押されたときの処理です。
        /// センター・日種類・時間帯が正しく入力されていることを確認した後、
        /// 条件に合致する勤怠明細をメイングリッドに表示します。
        /// </summary>
        private void ExecuteSearch(object? obj)
        {
            // センター、日種類、時間帯の内部ID が取得できていることを確認する
            if (_centerId == 0 || _dayKindId == 0 || _timeZoneId == 0)
            {
                MessageBox.Show("センター、日種類、時間帯を正しく入力してください。", "入力エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 条件に合致するレコードを取得してグリッドに表示する
            var dt = _repository.GetRegistrationDetails(AttendanceDate, CenterCode, DayKindCode, TimeZoneCode);
            EmployeeDetails.Clear();
            foreach (DataRow row in dt.Rows)
            {
                // DataRow を ViewModel 用のアイテムクラスに変換してコレクションへ追加する
                var item = new AttendanceDetailItem(row);
                EmployeeDetails.Add(item);
            }
        }

        /// <summary>
        /// 「登録」ボタン (F12) が押されたときの処理です。
        /// 確認ダイアログを表示し、承認された場合はグリッドの全行データを
        /// データベースに保存（UPSERT）します。
        /// </summary>
        private void ExecuteSave(object? obj)
        {
            // 明細が存在しない場合は処理しない
            if (EmployeeDetails.Count == 0) return;

            var result = MessageBox.Show("勤怠データを登録しますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    foreach (var item in EmployeeDetails)
                    {
                        // 各明細行をモデルに変換し、検索条件のキー情報を付加してから保存する
                        var model = item.ToModel();
                        model.C_Date = AttendanceDate;
                        model.C_CenterID = _centerId;
                        model.C_DayKindID = _dayKindId;
                        model.C_TimeZoneID = _timeZoneId;

                        _repository.UpsertAttendance(model);
                    }
                    MessageBox.Show("登録が完了しました。", "完了", MessageBoxButton.OK, MessageBoxImage.Information);
                    // 登録後に選択リストを更新する
                    ExecuteList(null!);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"登録中にエラーが発生しました：{ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// 「行削除」ボタン (F7) が押されたときの処理です。
        /// 確認ダイアログを表示し、承認された場合はグリッドで選択中の行を
        /// データベースから削除します。
        /// </summary>
        private void ExecuteDeleteRow(object? obj)
        {
            // 選択行がない場合は処理しない
            if (SelectedEmployee == null) return;

            var result = MessageBox.Show("選択行のデータを削除しますか？", "削除確認", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // 選択行をモデルに変換し、現在の検索条件キーを付加してから削除する
                    var model = SelectedEmployee.ToModel();
                    model.C_Date = AttendanceDate;
                    model.C_CenterID = _centerId;
                    model.C_DayKindID = _dayKindId;
                    model.C_TimeZoneID = _timeZoneId;

                    _repository.DeleteAttendance(model);
                    // DB 削除成功後にグリッドからも除去する
                    EmployeeDetails.Remove(SelectedEmployee);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"削除中にエラーが発生しました：{ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// 「中止」ボタン (F10) が押されたときの処理です。
        /// すべての入力欄と明細グリッドをクリアして初期状態に戻します。
        /// </summary>
        private void ExecuteCancel(object? obj)
        {
            // 入力欄・名称欄・グリッドをすべてクリアする
            CenterCode = string.Empty;
            DayKindCode = string.Empty;
            TimeZoneCode = string.Empty;
            EmployeeDetails.Clear();
        }

        #endregion
    }

    /// <summary>
    /// 勤怠明細グリッドの1行に対応する表示用クラスです。
    /// DataRow から各プロパティへのマッピングと、TtbAttendance モデルへの変換機能を持ちます。
    /// </summary>
    public class AttendanceDetailItem : ViewModelBase
    {
        /// <summary>職種略称（表示専用）</summary>
        public string JobName { get; set; } = string.Empty;
        /// <summary>職種コード</summary>
        public int JobCode { get; set; }
        /// <summary>職種の内部ID（保存時に使用）</summary>
        public int JobID { get; set; }
        /// <summary>社員氏名（表示専用）</summary>
        public string EmployeeName { get; set; } = string.Empty;
        /// <summary>社員コード</summary>
        public string EmployeeCode { get; set; } = string.Empty;
        /// <summary>社員の内部ID（保存時に使用）</summary>
        public int EmployeeID { get; set; }

        // 出務コード（1: 出務あり、0: 欠務）は編集可能なため変更通知付き
        private int _workingCode;
        /// <summary>出務区分（1: 出務、0: 欠務）。グリッドで直接編集可能です。</summary>
        public int WorkingCode
        {
            get => _workingCode;
            set => SetProperty(ref _workingCode, value);
        }

        /// <summary>出務状況の表示用文字列（○: 出務、×: 欠務）</summary>
        public string WorkingDisplay => WorkingCode == 1 ? "○" : "×";

        /// <summary>超過勤務の開始時刻（HHMM 形式の long 値）</summary>
        public long OverStartTime { get; set; }
        /// <summary>超過勤務の終了時刻（HHMM 形式の long 値）</summary>
        public long OverEndTime { get; set; }
        /// <summary>休憩の開始時刻（HHMM 形式の long 値）</summary>
        public long BreakStartTime { get; set; }
        /// <summary>休憩の終了時刻（HHMM 形式の long 値）</summary>
        public long BreakEndTime { get; set; }
        /// <summary>遅参の開始時刻（HHMM 形式の long 値）</summary>
        public long LateStartTime { get; set; }
        /// <summary>遅参の終了時刻（HHMM 形式の long 値）</summary>
        public long LateEndTime { get; set; }

        /// <summary>単価設定の内部ID</summary>
        public int UnitPriceID { get; set; }
        /// <summary>適用単価</summary>
        public decimal UnitPrice { get; set; }
        /// <summary>基礎時間（分）</summary>
        public int StandardTime { get; set; }

        /// <summary>
        /// DataRow から本クラスのインスタンスを初期化します。
        /// GetRegistrationDetails で取得した行データをグリッドに表示するために使用します。
        /// </summary>
        /// <param name="row">勤怠明細の DataRow</param>
        public AttendanceDetailItem(DataRow row)
        {
            JobName = row["C_JobName"]?.ToString() ?? string.Empty;
            JobCode = Convert.ToInt32(row["C_JobCode"]);
            JobID = Convert.ToInt32(row["C_JobID"]);
            EmployeeName = row["C_EmployeeName"]?.ToString() ?? string.Empty;
            EmployeeCode = row["C_EmployeeCode"]?.ToString() ?? string.Empty;
            EmployeeID = Convert.ToInt32(row["C_EmployeeID"]);
            WorkingCode = Convert.ToInt32(row["C_WorkingCode"]);

            OverStartTime = Convert.ToInt64(row["C_OverStartTime"]);
            OverEndTime = Convert.ToInt64(row["C_OverEndTime"]);
            BreakStartTime = Convert.ToInt64(row["C_BreakStartTime"]);
            BreakEndTime = Convert.ToInt64(row["C_BreakEndTime"]);
            LateStartTime = Convert.ToInt64(row["C_LateStartTime"]);
            LateEndTime = Convert.ToInt64(row["C_LateEndTime"]);

            UnitPriceID = Convert.ToInt32(row["C_UnitPriceID"]);
            UnitPrice = Convert.ToDecimal(row["C_UnitPrice"]);
            StandardTime = Convert.ToInt32(row["C_StandardTime"]);
        }

        /// <summary>
        /// 本クラスのデータを TtbAttendance モデルに変換して返します。
        /// 保存・削除コマンドの実行前に、主キー情報（日付・センター等）を付加してから使用します。
        /// </summary>
        /// <returns>DB 保存用の勤怠データモデル</returns>
        public TtbAttendance ToModel()
        {
            return new TtbAttendance
            {
                C_JobID = this.JobID,
                C_EmployeeID = this.EmployeeID,
                C_WorkingCode = this.WorkingCode,
                C_OverStartTime = this.OverStartTime,
                C_OverEndTime = this.OverEndTime,
                C_BreakStartTime = this.BreakStartTime,
                C_BreakEndTime = this.BreakEndTime,
                C_LateStartTime = this.LateStartTime,
                C_LateEndTime = this.LateEndTime,
                C_UnitPriceID = this.UnitPriceID,
                C_UnitPrice = this.UnitPrice,
                C_StandardTime = this.StandardTime
                // 注意: 超過時間・遅参時間・報酬額などの計算項目は事後計算処理で設定します
            };
        }
    }
}

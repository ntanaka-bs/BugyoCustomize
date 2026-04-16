using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Microsoft.Data.SqlClient;
using AttendanceSystem.Common;

namespace AttendanceSystem.Models
{
    /// <summary>
    /// 日毎勤怠チェックリスト画面のデータアクセスを担当するリポジトリクラスです。
    /// 期間・センター・職種・日種類・時間帯・出務状況などの複合条件で
    /// T_tbAttendance テーブルから勤怠データを抽出します。
    /// </summary>
    public class DailyAttendanceRepository
    {
        private readonly string _connectionString;

        /// <summary>
        /// コンストラクタ。接続文字列の取得とスキーマ整合性の確認を行います。
        /// </summary>
        public DailyAttendanceRepository()
        {
            _connectionString = DatabaseConfig.ConnectionString;
            // スキーマの整合性を確認（T_tbAttendance への列追加等が未済の場合は自動補完する）
            new AttendanceDatabaseHelper();
        }

        /// <summary>
        /// 指定された期間と職種コード範囲で、勤怠データの件数を取得します。
        /// チェックリスト画面の「プレビュー」前に件数を確認するために使用します。
        /// </summary>
        /// <param name="start">期間の開始日付</param>
        /// <param name="end">期間の終了日付</param>
        /// <param name="jobStart">職種コード範囲の開始値（空文字の場合は下限なし）</param>
        /// <param name="jobEnd">職種コード範囲の終了値（空文字の場合は上限なし）</param>
        /// <returns>条件に合致するレコードの件数</returns>
        public int GetAttendanceCount(DateTime start, DateTime end, string jobStart, string jobEnd, string dayKindStart, string dayKindEnd)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                // 基本条件: 指定期間内のデータを対象とする
                var sql = new StringBuilder(@"
                    SELECT COUNT(*) 
                    FROM T_tbAttendance A
                    LEFT JOIN T_tbJob J ON A.C_JobID = J.C_JobID
                    LEFT JOIN T_tbDayKind D ON A.C_DayKindID = D.C_DayKindID
                    WHERE A.C_Date BETWEEN @Start AND @End");

                // コード範囲が指定されている場合のみ WHERE 句に絞り込み条件を追加する
                AppendCodeRangeFilter(sql, "J.C_JobCode", jobStart, jobEnd, "@JobStart", "@JobEnd");
                AppendCodeRangeFilter(sql, "D.C_DayKindCode", dayKindStart, dayKindEnd, "@DayKindStart", "@DayKindEnd");

                var cmd = new SqlCommand(sql.ToString(), conn);
                cmd.Parameters.AddWithValue("@Start", start);
                cmd.Parameters.AddWithValue("@End", end);
                AddCodeParameters(cmd, "@JobStart", jobStart, "@JobEnd", jobEnd);
                AddCodeParameters(cmd, "@DayKindStart", dayKindStart, "@DayKindEnd", dayKindEnd);

                conn.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        /// <summary>
        /// 指定された複合条件で詳細な勤怠データを取得します。
        /// チェックリスト・注意リスト・計算前リストのプレビューおよび印刷に使用します。
        /// リスト種別（listType）によって取得するカラムと抽出条件が変わります。
        /// </summary>
        /// <param name="listType">
        /// 出力するリストの種別。
        /// 0: チェックリスト（勤怠の詳細時刻・超過・遅参情報を含む）、
        /// 1: 注意リスト（T_tbAttendanceImport からエラーフラグ有のみ取得）、
        /// 2: 計算前リスト（チェックリストと同様のカラム構成）
        /// </param>
        /// <param name="start">期間の開始日付</param>
        /// <param name="end">期間の終了日付</param>
        /// <param name="centerStart">センターコードの開始値</param>
        /// <param name="centerEnd">センターコードの終了値</param>
        /// <param name="jobStart">職種コードの開始値</param>
        /// <param name="jobEnd">職種コードの終了値</param>
        /// <param name="dayKindStart">日種類コードの開始値</param>
        /// <param name="dayKindEnd">日種類コードの終了値</param>
        /// <param name="timeZoneStart">時間帯コードの開始値</param>
        /// <param name="timeZoneEnd">時間帯コードの終了値</param>
        /// <param name="attendanceStatus">出務状況フィルタ（0: 全て、1: 出務あり、2: 欠務）</param>
        /// <returns>条件に合致する勤怠データの DataTable</returns>
        public DataTable GetAttendanceDataDetailed(
            int listType,
            DateTime start,
            DateTime end,
            string centerStart, string centerEnd,
            string jobStart, string jobEnd,
            string dayKindStart, string dayKindEnd,
            string timeZoneStart, string timeZoneEnd,
            int attendanceStatus)
        {
            var dt = new DataTable();
            using (var conn = new SqlConnection(_connectionString))
            {
                var sql = new StringBuilder();

                // リスト種別に応じて参照するテーブルを切り替える
                // listType=1（注意リスト）は T_tbAttendanceImport（CSV取込データ）を使用する
                string tableName = (listType == 1) ? "T_tbAttendanceImport" : "T_tbAttendance";

                // 全リスト共通のカラムを SELECT する
                sql.AppendLine("SELECT");
                sql.AppendLine("    C.C_CenterName AS [センター],");
                sql.AppendLine("    A.C_Date AS [日付],");
                sql.AppendLine("    E.C_EmployeeCode AS [職員コード],");
                sql.AppendLine("    E.C_EmployeeName AS [職員名],");
                sql.AppendLine("    J.C_JobAbbreviationName AS [職種],");
                sql.AppendLine("    D.C_DayKindAbbreviationName AS [日種類],");
                sql.AppendLine("    T.C_TimeZoneAbbreviationName AS [時間帯],");
                sql.AppendLine("    A.C_UnitPrice AS [単価],");
                sql.AppendLine("    CASE WHEN A.C_WorkingCode = 1 THEN '○' ELSE '×' END AS [出務],");
                sql.AppendLine("    A.C_StandardTime AS [基礎時間],");

                if (listType == 0 || listType == 2) // チェックリスト or 計算前リスト
                {
                    // 超過・遅参・報酬額などの詳細時刻情報を追加する
                    sql.AppendLine("    A.C_OverStartTime AS [超過開始],");
                    sql.AppendLine("    A.C_OverEndTime AS [超過終了],");
                    sql.AppendLine("    A.C_BreakStartTime AS [休憩開始],");
                    sql.AppendLine("    A.C_BreakEndTime AS [休憩終了],");
                    sql.AppendLine("    A.C_OverTime AS [超過],");
                    sql.AppendLine("    A.C_OverMoney AS [超過額],");
                    sql.AppendLine("    A.C_LateStartTime AS [遅参開始],");
                    sql.AppendLine("    A.C_LateEndTime AS [遅参終了],");
                    sql.AppendLine("    A.C_LateTime AS [遅参],");
                    sql.AppendLine("    A.C_LateMoney AS [遅参額],");
                    sql.AppendLine("    A.C_PaymentMoney AS [報酬額]");
                }
                else if (listType == 1) // 注意リスト
                {
                    // 注意リストはエラー内容のみを表示する
                    sql.AppendLine("    A.C_Err AS [注意点]");
                }

                // FROM 句と JOIN 句を構築する
                sql.AppendLine($"FROM {tableName} A");
                sql.AppendLine("LEFT JOIN T_tbEmployee E ON A.C_EmployeeID = E.C_EmployeeID");
                sql.AppendLine("LEFT JOIN T_tbCenter C ON A.C_CenterID = C.C_CenterID");
                sql.AppendLine("LEFT JOIN T_tbJob J ON A.C_JobID = J.C_JobID");
                sql.AppendLine("LEFT JOIN T_tbDayKind D ON A.C_DayKindID = D.C_DayKindID");
                sql.AppendLine("LEFT JOIN T_tbTimeZone T ON A.C_TimeZoneID = T.C_TimeZoneID");
                sql.AppendLine("WHERE A.C_Date BETWEEN @Start AND @End");

                if (listType == 1)
                {
                    // 注意リストはエラーフラグ（C_ErrFlg）が 1 のレコードのみ対象とする
                    sql.AppendLine("AND A.C_ErrFlg = 1");
                }

                // 各コード範囲フィルタを動的に追加する（空文字の場合は追加しない）
                AppendCodeRangeFilter(sql, "C.C_CenterCode", centerStart, centerEnd, "@CenterStart", "@CenterEnd");
                AppendCodeRangeFilter(sql, "J.C_JobCode", jobStart, jobEnd, "@JobStart", "@JobEnd");
                AppendCodeRangeFilter(sql, "D.C_DayKindCode", dayKindStart, dayKindEnd, "@DayKindStart", "@DayKindEnd");
                AppendCodeRangeFilter(sql, "T.C_TimeZoneCode", timeZoneStart, timeZoneEnd, "@TimeZoneStart", "@TimeZoneEnd");

                // 出務状況フィルタを追加する（0=全て は条件なし）
                if (attendanceStatus == 1) sql.AppendLine("AND A.C_WorkingCode = 1");  // 出務あり
                else if (attendanceStatus == 2) sql.AppendLine("AND A.C_WorkingCode = 0"); // 欠務

                sql.AppendLine("ORDER BY A.C_Date, C.C_CenterCode, T.C_TimeZoneCode, J.C_JobCode, E.C_EmployeeCode");

                var cmd = new SqlCommand(sql.ToString(), conn);
                cmd.Parameters.AddWithValue("@Start", start);
                cmd.Parameters.AddWithValue("@End", end);
                AddCodeParameters(cmd, "@CenterStart", centerStart, "@CenterEnd", centerEnd);
                AddCodeParameters(cmd, "@JobStart", jobStart, "@JobEnd", jobEnd);
                AddCodeParameters(cmd, "@DayKindStart", dayKindStart, "@DayKindEnd", dayKindEnd);
                AddCodeParameters(cmd, "@TimeZoneStart", timeZoneStart, "@TimeZoneEnd", timeZoneEnd);

                var adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
            }
            return dt;
        }

        /// <summary>
        /// SQL の WHERE 句に数値コードの範囲フィルタを動的に追加するヘルパーメソッドです。
        /// start または end が空文字の場合、その境界条件は SQL に追加しません。
        /// </summary>
        /// <param name="sql">追加先の SQL 文字列</param>
        /// <param name="columnName">絞り込みを行うカラム名</param>
        /// <param name="start">コード範囲の開始値（空文字の場合は下限なし）</param>
        /// <param name="end">コード範囲の終了値（空文字の場合は上限なし）</param>
        /// <param name="startParam">開始値のパラメータ名</param>
        /// <param name="endParam">終了値のパラメータ名</param>
        private void AppendCodeRangeFilter(StringBuilder sql, string columnName, string start, string end, string startParam, string endParam)
        {
            if (!string.IsNullOrEmpty(start)) sql.AppendLine($"AND {columnName} >= {startParam}");
            if (!string.IsNullOrEmpty(end)) sql.AppendLine($"AND {columnName} <= {endParam}");
        }

        /// <summary>
        /// SqlCommand にコード範囲のパラメータを追加するヘルパーメソッドです。
        /// 値が空文字の場合はパラメータを追加しません（AppendCodeRangeFilter と対で使用）。
        /// </summary>
        private void AddCodeParameters(SqlCommand cmd, string startParam, string start, string endParam, string end)
        {
            if (!string.IsNullOrEmpty(start)) cmd.Parameters.AddWithValue(startParam, start);
            if (!string.IsNullOrEmpty(end)) cmd.Parameters.AddWithValue(endParam, end);
        }

        /// <summary>
        /// 期間と職種コード範囲を指定して勤怠データを取得する簡易版メソッドです。
        /// 内部では GetAttendanceDataDetailed を呼び出します。
        /// </summary>
        public DataTable GetAttendanceData(DateTime start, DateTime end, string jobStart, string jobEnd)
        {
            return GetAttendanceDataDetailed(0, start, end, "", "", jobStart, jobEnd, "", "", "", "", 0);
        }
    }
}

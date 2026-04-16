using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Microsoft.Data.SqlClient;
using AttendanceSystem.Common;

namespace AttendanceSystem.Models
{
    /// <summary>
    /// 日毎勤怠データの検索・抽出を行うリポジトリ
    /// </summary>
    public class DailyAttendanceRepository
    {
        private readonly string _connectionString;

        public DailyAttendanceRepository()
        {
            _connectionString = DatabaseConfig.ConnectionString;
            // スキーマの整合性を確認
            new AttendanceDatabaseHelper();
        }

        /// <summary>
        /// 条件を指定して、勤怠データの件数を抽出します。
        /// </summary>
        public int GetAttendanceCount(DateTime start, DateTime end, string jobStart, string jobEnd, string dayKindStart, string dayKindEnd)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var sql = new StringBuilder(@"
                    SELECT COUNT(*) 
                    FROM T_tbAttendance A
                    LEFT JOIN T_tbJob J ON A.C_JobID = J.C_JobID
                    LEFT JOIN T_tbDayKind D ON A.C_DayKindID = D.C_DayKindID
                    WHERE A.C_Date BETWEEN @Start AND @End");

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
        /// 条件を指定して、詳細な勤怠データを取得します（プレビュー・印刷用）。
        /// </summary>
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
                string tableName = (listType == 1) ? "T_tbAttendanceImport" : "T_tbAttendance";

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
                    sql.AppendLine("    A.C_Err AS [注意点]");
                }

                sql.AppendLine($"FROM {tableName} A");
                sql.AppendLine("LEFT JOIN T_tbEmployee E ON A.C_EmployeeID = E.C_EmployeeID");
                sql.AppendLine("LEFT JOIN T_tbCenter C ON A.C_CenterID = C.C_CenterID");
                sql.AppendLine("LEFT JOIN T_tbJob J ON A.C_JobID = J.C_JobID");
                sql.AppendLine("LEFT JOIN T_tbDayKind D ON A.C_DayKindID = D.C_DayKindID");
                sql.AppendLine("LEFT JOIN T_tbTimeZone T ON A.C_TimeZoneID = T.C_TimeZoneID");
                sql.AppendLine("WHERE A.C_Date BETWEEN @Start AND @End");

                if (listType == 1) // 注意リストはエラーフラグが立っているもののみ
                {
                    sql.AppendLine("AND A.C_ErrFlg = 1");
                }

                // 範囲フィルタの追加
                AppendCodeRangeFilter(sql, "C.C_CenterCode", centerStart, centerEnd, "@CenterStart", "@CenterEnd");
                AppendCodeRangeFilter(sql, "J.C_JobCode", jobStart, jobEnd, "@JobStart", "@JobEnd");
                AppendCodeRangeFilter(sql, "D.C_DayKindCode", dayKindStart, dayKindEnd, "@DayKindStart", "@DayKindEnd");
                AppendCodeRangeFilter(sql, "T.C_TimeZoneCode", timeZoneStart, timeZoneEnd, "@TimeZoneStart", "@TimeZoneEnd");

                // 出務状況フィルタ
                if (attendanceStatus == 1) sql.AppendLine("AND A.C_WorkingCode = 1");
                else if (attendanceStatus == 2) sql.AppendLine("AND A.C_WorkingCode = 0");

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

        private void AppendCodeRangeFilter(StringBuilder sql, string columnName, string start, string end, string startParam, string endParam)
        {
            if (!string.IsNullOrEmpty(start)) sql.AppendLine($"AND {columnName} >= {startParam}");
            if (!string.IsNullOrEmpty(end)) sql.AppendLine($"AND {columnName} <= {endParam}");
        }

        private void AddCodeParameters(SqlCommand cmd, string startParam, string start, string endParam, string end)
        {
            if (!string.IsNullOrEmpty(start)) cmd.Parameters.AddWithValue(startParam, start);
            if (!string.IsNullOrEmpty(end)) cmd.Parameters.AddWithValue(endParam, end);
        }

        public DataTable GetAttendanceData(DateTime start, DateTime end, string jobStart, string jobEnd)
        {
            return GetAttendanceDataDetailed(0, start, end, "", "", jobStart, jobEnd, "", "", "", "", 0);
        }
    }
}

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
        }

        /// <summary>
        /// 条件を指定して、勤怠データの件数や内容を抽出します（現在はモック実装の側面を含む）
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

                if (!string.IsNullOrEmpty(jobStart) && jobStart != "最初")
                {
                    sql.Append(" AND J.C_JobCode >= @JobStart");
                }
                if (!string.IsNullOrEmpty(jobEnd) && jobEnd != "最後")
                {
                    sql.Append(" AND J.C_JobCode <= @JobEnd");
                }
                // 他のフィルタ（時間帯、センター等）も同様に拡張可能

                var cmd = new SqlCommand(sql.ToString(), conn);
                cmd.Parameters.AddWithValue("@Start", start);
                cmd.Parameters.AddWithValue("@End", end);
                
                if (sql.ToString().Contains("@JobStart")) cmd.Parameters.AddWithValue("@JobStart", jobStart);
                if (sql.ToString().Contains("@JobEnd")) cmd.Parameters.AddWithValue("@JobEnd", jobEnd);

                conn.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        /// <summary>
        /// 詳細データを取得する（プレビュー用）
        /// </summary>
        public DataTable GetAttendanceData(DateTime start, DateTime end, string jobStart, string jobEnd)
        {
            var dt = new DataTable();
            using (var conn = new SqlConnection(_connectionString))
            {
                var sql = new StringBuilder(@"
                    SELECT 
                        A.C_Date AS [日付],
                        E.C_EmployeeName AS [氏名],
                        J.C_JobName AS [職種],
                        D.C_DayKindName AS [日種類],
                        T.C_TimeZoneName AS [時間帯],
                        A.C_UnitPrice AS [単価],
                        A.C_OverTime AS [残業時間]
                    FROM T_tbAttendance A
                    LEFT JOIN T_tbEmployee E ON A.C_EmployeeID = E.C_EmployeeID
                    LEFT JOIN T_tbJob J ON A.C_JobID = J.C_JobID
                    LEFT JOIN T_tbDayKind D ON A.C_DayKindID = D.C_DayKindID
                    LEFT JOIN T_tbTimeZone T ON A.C_TimeZoneID = T.C_TimeZoneID
                    WHERE A.C_Date BETWEEN @Start AND @End");

                // 動的な条件構築（本来は全項目対応するが、まずは基本項目）
                if (jobStart != "最初") sql.Append(" AND J.C_JobCode >= @JobStart");
                if (jobEnd != "最後") sql.Append(" AND J.C_JobCode <= @JobEnd");

                var cmd = new SqlCommand(sql.ToString(), conn);
                cmd.Parameters.AddWithValue("@Start", start);
                cmd.Parameters.AddWithValue("@End", end);
                if (jobStart != "最初") cmd.Parameters.AddWithValue("@JobStart", jobStart);
                if (jobEnd != "最後") cmd.Parameters.AddWithValue("@JobEnd", jobEnd);

                var adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
            }
            return dt;
        }
    }
}

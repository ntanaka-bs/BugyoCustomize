using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using AttendanceSystem.Models;

namespace AttendanceSystem.Models
{
    /// <summary>
    /// データベース基本操作（CRUD処理）を提供するリポジトリ
    /// </summary>
    public class UnitPriceRepository
    {
        /// <summary>
        /// 指定職種コードの職種名を取得
        /// </summary>
        /// <param name="jobCode">職種コード</param>
        /// <returns>職種名。見つからない場合は空文字</returns>
        public string GetJobName(int jobCode)
        {
            using (var conn = UnitPriceDatabaseHelper.GetConnection())
            {
                var cmd = new SqlCommand("SELECT TOP 1 C_JobName FROM T_tbJob WHERE C_JobCode = @JobCode", conn);
                cmd.Parameters.AddWithValue("@JobCode", jobCode);
                conn.Open();
                var result = cmd.ExecuteScalar();
                return result != null ? result.ToString() : string.Empty;
            }
        }

        /// <summary>
        /// 指定日種類コードの日種類略称を取得
        /// </summary>
        /// <param name="dayKindCode">日種類コード</param>
        /// <returns>日種類略称。見つからない場合は空文字</returns>
        public string GetDayKindName(int dayKindCode)
        {
            using (var conn = UnitPriceDatabaseHelper.GetConnection())
            {
                var cmd = new SqlCommand("SELECT TOP 1 C_DayKindAbbreviationName FROM T_tbDayKind WHERE C_DayKindCode = @DayKindCode", conn);
                cmd.Parameters.AddWithValue("@DayKindCode", dayKindCode);
                conn.Open();
                var result = cmd.ExecuteScalar();
                return result != null ? result.ToString() : string.Empty;
            }
        }

        /// <summary>
        /// 指定時間帯コードの時間帯略称を取得
        /// </summary>
        /// <param name="timeZoneCode">時間帯コード</param>
        /// <returns>時間帯略称。見つからない場合は空文字</returns>
        public string GetTimeZoneName(int timeZoneCode)
        {
            using (var conn = UnitPriceDatabaseHelper.GetConnection())
            {
                var cmd = new SqlCommand("SELECT TOP 1 C_TimeZoneAbbreviationName FROM T_tbTimeZone WHERE C_TimeZoneCode = @TimeZoneCode", conn);
                cmd.Parameters.AddWithValue("@TimeZoneCode", timeZoneCode);
                conn.Open();
                var result = cmd.ExecuteScalar();
                return result != null ? result.ToString() : string.Empty;
            }
        }

        /// <summary>
        /// 条件を元に明細リスト取得（マスタ総当りに設定済単価を外部結合）
        /// </summary>
        /// <param name="jobCode">職種コード</param>
        /// <param name="dayKindCode">日種類コード（任意）</param>
        /// <param name="timeZoneCode">時間帯コード（任意）</param>
        /// <returns>画面表示用明細データリスト</returns>
        public List<UnitPriceDetail> GetDetailData(int jobCode, int? dayKindCode, int? timeZoneCode)
        {
            var list = new List<UnitPriceDetail>();
            using (var conn = UnitPriceDatabaseHelper.GetConnection())
            {
                var sql = @"
                    SELECT 
                        MST.C_JobID,
                        MST.C_DayKindID,
                        MST.C_DayKindCode,
                        MST.C_DayKindAbbreviationName,
                        MST.C_TimeZoneID,
                        MST.C_TimeZoneCode,
                        MST.C_TimeZoneAbbreviationName,
                        ISNULL(U.C_UnitPriceID, 0) AS C_UnitPriceID,
                        ISNULL(U.C_UnitPrice, 0) AS C_UnitPrice,
                        ISNULL(U.C_StandardTime, 0) AS C_BasicTime,
                        ISNULL(U.C_MinutePrice, 0) AS C_MinutePrice,
                        ISNULL(U.C_StartTime, 0) AS C_StartTime,
                        ISNULL(U.C_EndTime, 0) AS C_EndTime,
                        ISNULL(U.C_BreakStartTime, 0) AS C_BreakStartTime,
                        ISNULL(U.C_BreakEndTime, 0) AS C_BreakEndTime
                    FROM 
                    ( 
                        SELECT 
                            T_tbJob.C_JobID,
                            T_tbDayKind.C_DayKindID,
                            T_tbDayKind.C_DayKindCode,
                            T_tbDayKind.C_DayKindAbbreviationName,
                            T_tbTimeZone.C_TimeZoneID,
                            T_tbTimeZone.C_TimeZoneCode,
                            T_tbTimeZone.C_TimeZoneAbbreviationName
                        FROM 
                            T_tbJob, T_tbDayKind, T_tbTimeZone
                        WHERE 
                            T_tbJob.C_JobCode = @JobCode
                            " + (dayKindCode.HasValue ? " AND T_tbDayKind.C_DayKindCode = @DayKindCode " : "") + @"
                            " + (timeZoneCode.HasValue ? " AND T_tbTimeZone.C_TimeZoneCode = @TimeZoneCode " : "") + @"
                    ) AS MST 
                    LEFT JOIN T_tbUnitPrice U
                        ON MST.C_JobID = U.C_JobID 
                        AND MST.C_DayKindID = U.C_DayKindID 
                        AND MST.C_TimeZoneID = U.C_TimeZoneID 
                    ORDER BY 
                        MST.C_DayKindCode, MST.C_TimeZoneCode";

                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@JobCode", jobCode);
                if (dayKindCode.HasValue) cmd.Parameters.AddWithValue("@DayKindCode", dayKindCode.Value);
                if (timeZoneCode.HasValue) cmd.Parameters.AddWithValue("@TimeZoneCode", timeZoneCode.Value);

                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var detail = new UnitPriceDetail
                        {
                            JobID = Convert.ToInt32(reader["C_JobID"]),
                            DayKindID = Convert.ToInt32(reader["C_DayKindID"]),
                            DayKindCode = reader["C_DayKindCode"].ToString(),
                            DayKindName = reader["C_DayKindAbbreviationName"].ToString(),
                            TimeZoneID = Convert.ToInt32(reader["C_TimeZoneID"]),
                            TimeZoneCode = reader["C_TimeZoneCode"].ToString(),
                            TimeZoneName = reader["C_TimeZoneAbbreviationName"].ToString(),
                            UnitPriceID = Convert.ToInt32(reader["C_UnitPriceID"]),
                            UnitPrice = Convert.ToInt32(reader["C_UnitPrice"]),
                            BasicTime = Convert.ToInt32(reader["C_BasicTime"]),
                            MinutePrice = Convert.ToInt32(reader["C_MinutePrice"]),
                            StartTime = Convert.ToInt32(reader["C_StartTime"]),
                            EndTime = Convert.ToInt32(reader["C_EndTime"]),
                            BreakStartTime = Convert.ToInt32(reader["C_BreakStartTime"]),
                            BreakEndTime = Convert.ToInt32(reader["C_BreakEndTime"])
                        };
                        list.Add(detail);
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// 1件の単価設定レコードをDBへ保存（登録・更新）
        /// </summary>
        /// <param name="detail">単価明細データ</param>
        /// <returns>正常完了でtrue</returns>
        public bool SaveUnitPriceData(UnitPriceDetail detail)
        {
            using (var conn = UnitPriceDatabaseHelper.GetConnection())
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        string checkSql = "SELECT TOP 1 C_UnitPriceID FROM T_tbUnitPrice WHERE C_UnitPriceID = @UnitPriceID";
                        var checkCmd = new SqlCommand(checkSql, conn, trans);
                        checkCmd.Parameters.AddWithValue("@UnitPriceID", detail.UnitPriceID);
                        var exists = checkCmd.ExecuteScalar() != null;

                        if (exists)
                        {
                            string updateSql = @"
                                UPDATE T_tbUnitPrice SET 
                                    C_UnitPrice = @UnitPrice,
                                    C_StandardTime = @BasicTime,
                                    C_MinutePrice = @MinutePrice,
                                    C_StartTime = @StartTime,
                                    C_EndTime = @EndTime,
                                    C_BreakStartTime = @BreakStartTime,
                                    C_BreakEndTime = @BreakEndTime
                                WHERE C_UnitPriceID = @UnitPriceID";
                            var updateCmd = new SqlCommand(updateSql, conn, trans);
                            SetUpdateParameters(updateCmd, detail);
                            updateCmd.ExecuteNonQuery();
                        }
                        else
                        {
                            string insertSql = @"
                                INSERT INTO T_tbUnitPrice 
                                (C_JobID, C_DayKindID, C_TimeZoneID, C_UnitPrice, C_StandardTime, C_MinutePrice, C_StartTime, C_EndTime, C_BreakStartTime, C_BreakEndTime)
                                VALUES
                                (@JobID, @DayKindID, @TimeZoneID, @UnitPrice, @BasicTime, @MinutePrice, @StartTime, @EndTime, @BreakStartTime, @BreakEndTime)";
                            var insertCmd = new SqlCommand(insertSql, conn, trans);
                            SetInsertParameters(insertCmd, detail);
                            insertCmd.ExecuteNonQuery();
                        }

                        trans.Commit();
                        return true;
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }

        private void SetUpdateParameters(SqlCommand cmd, UnitPriceDetail detail)
        {
            cmd.Parameters.AddWithValue("@UnitPriceID", detail.UnitPriceID);
            cmd.Parameters.AddWithValue("@UnitPrice", detail.UnitPrice);
            cmd.Parameters.AddWithValue("@BasicTime", detail.BasicTime);
            cmd.Parameters.AddWithValue("@MinutePrice", detail.MinutePrice);
            cmd.Parameters.AddWithValue("@StartTime", detail.StartTime);
            cmd.Parameters.AddWithValue("@EndTime", detail.EndTime);
            cmd.Parameters.AddWithValue("@BreakStartTime", detail.BreakStartTime);
            cmd.Parameters.AddWithValue("@BreakEndTime", detail.BreakEndTime);
        }

        private void SetInsertParameters(SqlCommand cmd, UnitPriceDetail detail)
        {
            cmd.Parameters.AddWithValue("@JobID", detail.JobID);
            cmd.Parameters.AddWithValue("@DayKindID", detail.DayKindID);
            cmd.Parameters.AddWithValue("@TimeZoneID", detail.TimeZoneID);
            cmd.Parameters.AddWithValue("@UnitPrice", detail.UnitPrice);
            cmd.Parameters.AddWithValue("@BasicTime", detail.BasicTime);
            cmd.Parameters.AddWithValue("@MinutePrice", detail.MinutePrice);
            cmd.Parameters.AddWithValue("@StartTime", detail.StartTime);
            cmd.Parameters.AddWithValue("@EndTime", detail.EndTime);
            cmd.Parameters.AddWithValue("@BreakStartTime", detail.BreakStartTime);
            cmd.Parameters.AddWithValue("@BreakEndTime", detail.BreakEndTime);
        }

        /// <summary>
        /// 指定単価IDが勤怠データ（T_tbAttendance）で使用済か確認
        /// </summary>
        /// <param name="unitPriceId">単価設定ID</param>
        /// <returns>使用中ならtrue</returns>
        public bool HasAttendanceData(int unitPriceId)
        {
            using (var conn = UnitPriceDatabaseHelper.GetConnection())
            {
                var cmd = new SqlCommand("SELECT TOP 1 1 FROM T_tbAttendance WHERE C_UnitPriceID = @UnitPriceID", conn);
                cmd.Parameters.AddWithValue("@UnitPriceID", unitPriceId);
                conn.Open();
                return cmd.ExecuteScalar() != null;
            }
        }

        /// <summary>
        /// 指定単価レコードをDBから削除（勤怠データ使用時は例外発生）
        /// </summary>
        /// <param name="unitPriceId">単価設定ID</param>
        /// <returns>削除成功でtrue</returns>
        public bool DeleteUnitPriceData(int unitPriceId)
        {
            if (HasAttendanceData(unitPriceId))
            {
                throw new Exception("勤怠データが存在するため、削除できません。");
            }

            using (var conn = UnitPriceDatabaseHelper.GetConnection())
            {
                var cmd = new SqlCommand("DELETE FROM T_tbUnitPrice WHERE C_UnitPriceID = @UnitPriceID", conn);
                cmd.Parameters.AddWithValue("@UnitPriceID", unitPriceId);
                conn.Open();
                int result = cmd.ExecuteNonQuery();
                return result > 0;
            }
        }
    }
}

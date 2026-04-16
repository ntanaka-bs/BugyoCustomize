using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Microsoft.Data.SqlClient;
using AttendanceSystem.Common;

namespace AttendanceSystem.Models
{
    /// <summary>
    /// 勤怠データ入力画面のデータアクセスを担当するリポジトリクラスです。
    /// 勤怠データの検索・登録・更新・削除と、マスタ情報の参照をすべて担当します。
    /// </summary>
    public class AttendanceRegistrationRepository
    {
        private readonly string _connectionString;

        /// <summary>
        /// コンストラクタ。接続文字列の取得とデータベーススキーマの確認を行います。
        /// 起動時に T_tbAttendance への新規列追加など、スキーマ更新が必要な場合は自動で対応します。
        /// </summary>
        public AttendanceRegistrationRepository()
        {
            _connectionString = DatabaseConfig.ConnectionString;
            // スキーマの整合性を確認（通常勤務関連列の追加など自動マイグレーション）
            new AttendanceDatabaseHelper();
        }

        /// <summary>
        /// 指定された日付に登録されているセンター・日種類・時間帯の組み合わせ一覧を取得します。
        /// 画面右上の「選択リスト」グリッドの表示データとして使用されます。
        /// </summary>
        /// <param name="date">対象日付</param>
        /// <returns>センター・日種類・時間帯の組み合わせを含む DataTable</returns>
        public DataTable GetSelectionList(DateTime date)
        {
            var dt = new DataTable();
            using (var conn = new SqlConnection(_connectionString))
            {
                var sql = new StringBuilder();
                sql.AppendLine("SELECT ");
                sql.AppendLine("    C.C_CenterCode, ");
                sql.AppendLine("    C.C_CenterName, ");
                sql.AppendLine("    D.C_DayKindCode, ");
                sql.AppendLine("    D.C_DayKindAbbreviationName AS C_DayKindName, ");
                sql.AppendLine("    T.C_TimeZoneCode, ");
                sql.AppendLine("    T.C_TimeZoneAbbreviationName AS C_TimeZoneName ");
                sql.AppendLine("FROM T_tbAttendance A ");
                sql.AppendLine("INNER JOIN T_tbCenter C ON A.C_CenterID = C.C_CenterID ");
                sql.AppendLine("INNER JOIN T_tbDayKind D ON A.C_DayKindID = D.C_DayKindID ");
                sql.AppendLine("INNER JOIN T_tbTimeZone T ON A.C_TimeZoneID = T.C_TimeZoneID ");
                sql.AppendLine("WHERE A.C_Date = @Date ");
                sql.AppendLine("GROUP BY C.C_CenterCode, C.C_CenterName, D.C_DayKindCode, D.C_DayKindAbbreviationName, T.C_TimeZoneCode, T.C_TimeZoneAbbreviationName ");
                sql.AppendLine("ORDER BY C.C_CenterCode, D.C_DayKindCode, T.C_TimeZoneCode ");

                var cmd = new SqlCommand(sql.ToString(), conn);
                cmd.Parameters.AddWithValue("@Date", date);

                var adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
            }
            return dt;
        }

        /// <summary>
        /// 指定された日付・センター・日種類・時間帯の条件に合致する勤怠明細を取得します。
        /// 「検索」ボタン押下時にメイングリッドへ表示するデータを返します。
        /// 各コードが空文字の場合は、そのコードによる絞り込みは行いません。
        /// </summary>
        /// <param name="date">対象日付</param>
        /// <param name="centerCode">センターコード（空文字の場合は絞り込みなし）</param>
        /// <param name="dayKindCode">日種類コード（空文字の場合は絞り込みなし）</param>
        /// <param name="timeZoneCode">時間帯コード（空文字の場合は絞り込みなし）</param>
        /// <returns>勤怠明細レコードを含む DataTable</returns>
        public DataTable GetRegistrationDetails(DateTime date, string centerCode, string dayKindCode, string timeZoneCode)
        {
            var dt = new DataTable();
            using (var conn = new SqlConnection(_connectionString))
            {
                var sql = new StringBuilder();
                sql.AppendLine("SELECT ");
                sql.AppendLine("    J.C_JobCode, ");
                sql.AppendLine("    J.C_JobAbbreviationName AS C_JobName, ");
                sql.AppendLine("    E.C_EmployeeCode, ");
                sql.AppendLine("    E.C_EmployeeName, ");
                sql.AppendLine("    A.* ");
                sql.AppendLine("FROM T_tbAttendance A ");
                sql.AppendLine("INNER JOIN T_tbCenter C ON A.C_CenterID = C.C_CenterID ");
                sql.AppendLine("INNER JOIN T_tbJob J ON A.C_JobID = J.C_JobID ");
                sql.AppendLine("INNER JOIN T_tbDayKind D ON A.C_DayKindID = D.C_DayKindID ");
                sql.AppendLine("INNER JOIN T_tbTimeZone T ON A.C_TimeZoneID = T.C_TimeZoneID ");
                sql.AppendLine("INNER JOIN T_tbEmployee E ON A.C_EmployeeID = E.C_EmployeeID ");
                sql.AppendLine("WHERE A.C_Date = @Date ");
                
                if (!string.IsNullOrEmpty(centerCode)) sql.AppendLine("AND C.C_CenterCode = @CenterCode ");
                if (!string.IsNullOrEmpty(dayKindCode)) sql.AppendLine("AND D.C_DayKindCode = @DayKindCode ");
                if (!string.IsNullOrEmpty(timeZoneCode)) sql.AppendLine("AND T.C_TimeZoneCode = @TimeZoneCode ");

                sql.AppendLine("ORDER BY J.C_JobCode, E.C_EmployeeCode ");

                var cmd = new SqlCommand(sql.ToString(), conn);
                cmd.Parameters.AddWithValue("@Date", date);
                if (!string.IsNullOrEmpty(centerCode)) cmd.Parameters.AddWithValue("@CenterCode", centerCode);
                if (!string.IsNullOrEmpty(dayKindCode)) cmd.Parameters.AddWithValue("@DayKindCode", dayKindCode);
                if (!string.IsNullOrEmpty(timeZoneCode)) cmd.Parameters.AddWithValue("@TimeZoneCode", timeZoneCode);

                var adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
            }
            return dt;
        }

        /// <summary>
        /// 勤怠データを保存します（UPSERT）。
        /// 主キー（日付・センター・職種・日種類・時間帯・社員）が一致するレコードが存在する場合は更新、
        /// 存在しない場合は新規挿入を行います。失敗した場合はロールバックして例外を再スローします。
        /// </summary>
        /// <param name="item">保存する勤怠データモデル</param>
        public void UpsertAttendance(TtbAttendance item)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        var checkSql = "SELECT COUNT(*) FROM T_tbAttendance WHERE C_Date = @Date AND C_CenterID = @CenterID AND C_JobID = @JobID AND C_DayKindID = @DayKindID AND C_TimeZoneID = @TimeZoneID AND C_EmployeeID = @EmployeeID";
                        var checkCmd = new SqlCommand(checkSql, conn, trans);
                        AddPrimaryKeyParams(checkCmd, item);

                        bool exists = (int)checkCmd.ExecuteScalar() > 0;

                        string sql;
                        if (exists)
                        {
                            sql = @"UPDATE T_tbAttendance SET 
                                    C_UnitPriceID = @UnitPriceID, C_WorkingCode = @WorkingCode, C_UnitPrice = @UnitPrice, 
                                    C_StandardTime = @StandardTime, C_MinutePrice = @MinutePrice, 
                                    C_OverStartTime = @OverStartTime, C_OverEndTime = @OverEndTime, 
                                    C_BreakStartTime = @BreakStartTime, C_BreakEndTime = @BreakEndTime, 
                                    C_OverTime = @OverTime, C_OverMoney = @OverMoney, 
                                    C_LateStartTime = @LateStartTime, C_LateEndTime = @LateEndTime, 
                                    C_LateTime = @LateTime, C_LateMoney = @LateMoney, C_PaymentMoney = @PaymentMoney,
                                    C_UsualDutiesCode = @UsualDutiesCode, C_UsualDutiesStartTime = @UsualDutiesStartTime,
                                    C_UsualDutiesEndTime = @UsualDutiesEndTime, C_UsualDutiesRestStartTime = @UsualDutiesRestStartTime,
                                    C_UsualDutiesRestEndTime = @UsualDutiesRestEndTime, C_UsualDutiesTime = @UsualDutiesTime,
                                    C_UsualDutiesMoney = @UsualDutiesMoney
                                    WHERE C_Date = @Date AND C_CenterID = @CenterID AND C_JobID = @JobID AND C_DayKindID = @DayKindID AND C_TimeZoneID = @TimeZoneID AND C_EmployeeID = @EmployeeID";
                        }
                        else
                        {
                            sql = @"INSERT INTO T_tbAttendance (
                                    C_Date, C_CenterID, C_JobID, C_DayKindID, C_TimeZoneID, C_EmployeeID, 
                                    C_UnitPriceID, C_WorkingCode, C_UnitPrice, C_StandardTime, C_MinutePrice, 
                                    C_OverStartTime, C_OverEndTime, C_BreakStartTime, C_BreakEndTime, 
                                    C_OverTime, C_OverMoney, C_LateStartTime, C_LateEndTime, 
                                    C_LateTime, C_LateMoney, C_PaymentMoney,
                                    C_UsualDutiesCode, C_UsualDutiesStartTime, C_UsualDutiesEndTime, 
                                    C_UsualDutiesRestStartTime, C_UsualDutiesRestEndTime, C_UsualDutiesTime, C_UsualDutiesMoney
                                    ) VALUES (
                                    @Date, @CenterID, @JobID, @DayKindID, @TimeZoneID, @EmployeeID, 
                                    @UnitPriceID, @WorkingCode, @UnitPrice, @StandardTime, @MinutePrice, 
                                    @OverStartTime, @OverEndTime, @BreakStartTime, @BreakEndTime, 
                                    @OverTime, @OverMoney, @LateStartTime, @LateEndTime, 
                                    @LateTime, @LateMoney, @PaymentMoney,
                                    @UsualDutiesCode, @UsualDutiesStartTime, @UsualDutiesEndTime, 
                                    @UsualDutiesRestStartTime, @UsualDutiesRestEndTime, @UsualDutiesTime, @UsualDutiesMoney)";
                        }

                        var cmd = new SqlCommand(sql, conn, trans);
                        AddPrimaryKeyParams(cmd, item);
                        AddDataParams(cmd, item);

                        cmd.ExecuteNonQuery();
                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }

        private void AddPrimaryKeyParams(SqlCommand cmd, TtbAttendance item)
        {
            cmd.Parameters.AddWithValue("@Date", item.C_Date);
            cmd.Parameters.AddWithValue("@CenterID", item.C_CenterID);
            cmd.Parameters.AddWithValue("@JobID", item.C_JobID);
            cmd.Parameters.AddWithValue("@DayKindID", item.C_DayKindID);
            cmd.Parameters.AddWithValue("@TimeZoneID", item.C_TimeZoneID);
            cmd.Parameters.AddWithValue("@EmployeeID", item.C_EmployeeID);
        }

        private void AddDataParams(SqlCommand cmd, TtbAttendance item)
        {
            cmd.Parameters.AddWithValue("@UnitPriceID", item.C_UnitPriceID);
            cmd.Parameters.AddWithValue("@WorkingCode", item.C_WorkingCode);
            cmd.Parameters.AddWithValue("@UnitPrice", item.C_UnitPrice);
            cmd.Parameters.AddWithValue("@StandardTime", item.C_StandardTime);
            cmd.Parameters.AddWithValue("@MinutePrice", item.C_MinutePrice);
            cmd.Parameters.AddWithValue("@OverStartTime", item.C_OverStartTime);
            cmd.Parameters.AddWithValue("@OverEndTime", item.C_OverEndTime);
            cmd.Parameters.AddWithValue("@BreakStartTime", item.C_BreakStartTime);
            cmd.Parameters.AddWithValue("@BreakEndTime", item.C_BreakEndTime);
            cmd.Parameters.AddWithValue("@OverTime", item.C_OverTime);
            cmd.Parameters.AddWithValue("@OverMoney", item.C_OverMoney);
            cmd.Parameters.AddWithValue("@LateStartTime", item.C_LateStartTime);
            cmd.Parameters.AddWithValue("@LateEndTime", item.C_LateEndTime);
            cmd.Parameters.AddWithValue("@LateTime", item.C_LateTime);
            cmd.Parameters.AddWithValue("@LateMoney", item.C_LateMoney);
            cmd.Parameters.AddWithValue("@PaymentMoney", item.C_PaymentMoney);
            cmd.Parameters.AddWithValue("@UsualDutiesCode", item.C_UsualDutiesCode);
            cmd.Parameters.AddWithValue("@UsualDutiesStartTime", item.C_UsualDutiesStartTime);
            cmd.Parameters.AddWithValue("@UsualDutiesEndTime", item.C_UsualDutiesEndTime);
            cmd.Parameters.AddWithValue("@UsualDutiesRestStartTime", item.C_UsualDutiesRestStartTime);
            cmd.Parameters.AddWithValue("@UsualDutiesRestEndTime", item.C_UsualDutiesRestEndTime);
            cmd.Parameters.AddWithValue("@UsualDutiesTime", item.C_UsualDutiesTime);
            cmd.Parameters.AddWithValue("@UsualDutiesMoney", item.C_UsualDutiesMoney);
        }

        /// <summary>
        /// 指定された主キー条件に一致する勤怠データを1件削除します。
        /// （日付・センター・職種・日種類・時間帯・社員が一致する行を削除します）
        /// </summary>
        /// <param name="item">削除対象の勤怠データモデル</param>
        public void DeleteAttendance(TtbAttendance item)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var sql = "DELETE FROM T_tbAttendance WHERE C_Date = @Date AND C_CenterID = @CenterID AND C_JobID = @JobID AND C_DayKindID = @DayKindID AND C_TimeZoneID = @TimeZoneID AND C_EmployeeID = @EmployeeID";
                var cmd = new SqlCommand(sql, conn);
                AddPrimaryKeyParams(cmd, item);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        #region マスタ情報取得メソッド

        /// <summary>
        /// センターコードからセンターの内部IDと名称を取得します。
        /// 入力欄のコードが変更された際に名称を自動表示するために使用します。
        /// </summary>
        /// <param name="code">センターコード（文字列）</param>
        /// <returns>（内部ID, 名称）のタプル。見つからない場合は null。</returns>
        public (int id, string name)? GetCenterByCode(string code)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var sql = "SELECT C_CenterID, C_CenterName FROM T_tbCenter WHERE C_CenterCode = @Code";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Code", code);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    // レコードが見つかった場合は ID と名称のタプルを返す
                    if (reader.Read()) return ((int)reader["C_CenterID"], reader["C_CenterName"]?.ToString() ?? string.Empty);
                }
            }
            return null;
        }

        /// <summary>
        /// 職種コードから職種の内部IDと略称を取得します。
        /// </summary>
        public (int id, string name)? GetJobByCode(string code)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var sql = "SELECT C_JobID, C_JobAbbreviationName FROM T_tbJob WHERE C_JobCode = @Code";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Code", code);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read()) return ((int)reader["C_JobID"], reader["C_JobAbbreviationName"]?.ToString() ?? string.Empty);
                }
            }
            return null;
        }

        /// <summary>
        /// 日種類コードから日種類の内部IDと略称を取得します。
        /// </summary>
        public (int id, string name)? GetDayKindByCode(string code)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var sql = "SELECT C_DayKindID, C_DayKindAbbreviationName FROM T_tbDayKind WHERE C_DayKindCode = @Code";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Code", code);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read()) return ((int)reader["C_DayKindID"], reader["C_DayKindAbbreviationName"]?.ToString() ?? string.Empty);
                }
            }
            return null;
        }

        /// <summary>
        /// 時間帯コードから時間帯の内部IDと略称を取得します。
        /// </summary>
        public (int id, string name)? GetTimeZoneByCode(string code)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var sql = "SELECT C_TimeZoneID, C_TimeZoneAbbreviationName FROM T_tbTimeZone WHERE C_TimeZoneCode = @Code";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Code", code);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read()) return ((int)reader["C_TimeZoneID"], reader["C_TimeZoneAbbreviationName"]?.ToString() ?? string.Empty);
                }
            }
            return null;
        }

        /// <summary>
        /// 社員コードから社員の内部IDと氏名を取得します。
        /// </summary>
        public (int id, string name)? GetEmployeeByCode(string code)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var sql = "SELECT C_EmployeeID, C_EmployeeName FROM T_tbEmployee WHERE C_EmployeeCode = @Code";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Code", code);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read()) return ((int)reader["C_EmployeeID"], reader["C_EmployeeName"]?.ToString() ?? string.Empty);
                }
            }
            return null;
        }

        /// <summary>
        /// 職種・日種類・時間帯の組み合わせから単価設定を取得します。
        /// 検索結果が存在しない場合は null を返します。
        /// </summary>
        /// <param name="jobId">職種の内部ID</param>
        /// <param name="dayKindId">日種類の内部ID</param>
        /// <param name="timeZoneId">時間帯の内部ID</param>
        /// <returns>単価設定モデル。見つからない場合は null。</returns>
        public TtbUnitPrice? GetUnitPrice(int jobId, int dayKindId, int timeZoneId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var sql = "SELECT * FROM T_tbUnitPrice WHERE C_JobID = @JobID AND C_DayKindID = @DayKindID AND C_TimeZoneID = @TimeZoneID";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@JobID", jobId);
                cmd.Parameters.AddWithValue("@DayKindID", dayKindId);
                cmd.Parameters.AddWithValue("@TimeZoneID", timeZoneId);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new TtbUnitPrice
                        {
                            C_UnitPriceID = (int)reader["C_UnitPriceID"],
                            C_UnitPrice = Convert.ToInt32(reader["C_UnitPrice"]),
                            C_BasicTime = Convert.ToInt32(reader["C_StandardTime"]), // DBのカラム名は C_StandardTime
                            C_MinutePrice = Convert.ToInt32(reader["C_MinutePrice"])
                        };
                    }
                }
            }
            // 単価設定が登録されていない場合は null を返す
            return null;
        }

        #endregion
    }
}

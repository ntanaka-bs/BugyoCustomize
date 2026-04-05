using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace AttendanceSystem.Models
{
    /// <summary>
    /// 時間帯区分のデータアクセス層
    /// </summary>
    public class TimeZoneRepository
    {
        /// <summary>
        /// DB接続文字列
        /// </summary>
        private readonly string _connectionString = AttendanceSystem.Common.DatabaseConfig.ConnectionString;

        /// <summary>
        /// 初期化 (テーブル作成確認)
        /// </summary>
        public TimeZoneRepository()
        {
            EnsureTableCreated();
        }

        /// <summary>
        /// T_tbTimeZoneテーブルの存在確認と作成(DBマイグレーション)
        /// </summary>
        private void EnsureTableCreated()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                
                string checkTableSql = "SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[T_tbTimeZone]') AND type in (N'U')";
                using (var checkCmd = new SqlCommand(checkTableSql, connection))
                {
                    if (checkCmd.ExecuteScalar() == null)
                    {
                        string createSql = @"
CREATE TABLE [dbo].[T_tbTimeZone](
    [C_TimeZoneID] [int] IDENTITY(1,1) NOT NULL,
    [C_TimeZoneCode] [int] NOT NULL,
    [C_TimeZoneName] [nvarchar](100) NOT NULL,
    [C_TimeZoneAbbreviationName] [nvarchar](50) NOT NULL,
    [C_OrderCode] [int] NOT NULL DEFAULT 0,
    CONSTRAINT [PK_T_tbTimeZone] PRIMARY KEY CLUSTERED ([C_TimeZoneID] ASC)
)";
                        using (var createCmd = new SqlCommand(createSql, connection))
                        {
                            createCmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        string checkColumnSql = "SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('T_tbTimeZone') AND name = 'C_OrderCode'";
                        using (var colCmd = new SqlCommand(checkColumnSql, connection))
                        {
                            if (colCmd.ExecuteScalar() == null)
                            {
                                string alterSql = "ALTER TABLE T_tbTimeZone ADD C_OrderCode int NOT NULL DEFAULT 0";
                                using (var alterCmd = new SqlCommand(alterSql, connection))
                                {
                                    alterCmd.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 全時間帯区分データを取得
        /// </summary>
        public List<TimeZoneModel> GetAllTimeZones()
        {
            var list = new List<TimeZoneModel>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = "SELECT C_TimeZoneID, C_TimeZoneCode, C_TimeZoneName, C_TimeZoneAbbreviationName, C_OrderCode FROM T_tbTimeZone ORDER BY C_TimeZoneCode";
                using (var command = new SqlCommand(sql, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new TimeZoneModel
                        {
                            C_TimeZoneID = (int)reader["C_TimeZoneID"],
                            C_TimeZoneCode = reader["C_TimeZoneCode"] != DBNull.Value ? (int)reader["C_TimeZoneCode"] : null,
                            C_TimeZoneName = reader["C_TimeZoneName"]?.ToString(),
                            C_TimeZoneAbbreviationName = reader["C_TimeZoneAbbreviationName"]?.ToString(),
                            C_OrderCode = reader["C_OrderCode"] != DBNull.Value ? (int)reader["C_OrderCode"] : null
                        });
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// データの登録・更新
        /// </summary>
        public void Save(TimeZoneModel tz)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql;
                if (tz.C_TimeZoneID == 0)
                {
                    sql = "INSERT INTO T_tbTimeZone (C_TimeZoneCode, C_TimeZoneName, C_TimeZoneAbbreviationName, C_OrderCode) VALUES (@Code, @Name, @Abbrev, @Order)";
                }
                else
                {
                    sql = "UPDATE T_tbTimeZone SET C_TimeZoneCode = @Code, C_TimeZoneName = @Name, C_TimeZoneAbbreviationName = @Abbrev, C_OrderCode = @Order WHERE C_TimeZoneID = @ID";
                }

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@ID", tz.C_TimeZoneID);
                    command.Parameters.AddWithValue("@Code", (object?)tz.C_TimeZoneCode ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Name", (object?)tz.C_TimeZoneName ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Abbrev", (object?)tz.C_TimeZoneAbbreviationName ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Order", (object?)tz.C_OrderCode ?? DBNull.Value);
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// 指定されたIDのデータ削除 (戻り値: 1=成功, 2=単価登録使用中, 3=勤怠データ使用中)
        /// </summary>
        public int Delete(int tzId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                try
                {
                    string checkSql1 = "SELECT TOP 1 1 FROM T_tbUnitPrice WHERE C_TimeZoneID = @ID";
                    using (var cmd = new SqlCommand(checkSql1, connection))
                    {
                        cmd.Parameters.AddWithValue("@ID", tzId);
                        if (cmd.ExecuteScalar() != null) return 2;
                    }
                }
                catch { }

                try
                {
                    string checkSql2 = "SELECT TOP 1 1 FROM T_tbAttendance WHERE C_TimeZoneID = @ID";
                    using (var cmd = new SqlCommand(checkSql2, connection))
                    {
                        cmd.Parameters.AddWithValue("@ID", tzId);
                        if (cmd.ExecuteScalar() != null) return 3;
                    }
                }
                catch { }

                string sql = "DELETE FROM T_tbTimeZone WHERE C_TimeZoneID = @ID";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@ID", tzId);
                    command.ExecuteNonQuery();
                }
                return 1;
            }
        }
    }
}

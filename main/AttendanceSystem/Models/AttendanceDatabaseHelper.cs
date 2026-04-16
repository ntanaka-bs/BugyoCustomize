using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using AttendanceSystem.Common;

namespace AttendanceSystem.Models
{
    /// <summary>
    /// 勤怠関連テーブルの存在チェックと自動作成、およびスキーマ更新を担当するヘルパークラスです。
    /// </summary>
    public class AttendanceDatabaseHelper
    {
        private readonly string _connectionString;

        public AttendanceDatabaseHelper()
        {
            _connectionString = DatabaseConfig.ConnectionString;
            EnsureAttendanceImportTableExists();
            UpdateAttendanceTableSchema();
        }

        /// <summary>
        /// T_tbAttendanceImport テーブルの存在を確認し、存在しない場合は作成します。
        /// </summary>
        private void EnsureAttendanceImportTableExists()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string checkSql = "SELECT 1 FROM sysobjects WHERE name='T_tbAttendanceImport' AND xtype='U'";
                using (var checkCmd = new SqlCommand(checkSql, conn))
                {
                    if (checkCmd.ExecuteScalar() == null)
                    {
                        string createSql = @"
                            CREATE TABLE [dbo].[T_tbAttendanceImport](
                                [C_RowID] [int] IDENTITY(1,1) NOT NULL,
                                [C_Date] [datetime] NOT NULL DEFAULT '1900/01/01',
                                [C_CenterID] [int] NOT NULL DEFAULT 0,
                                [C_JobID] [int] NOT NULL DEFAULT 0,
                                [C_DayKindID] [int] NOT NULL DEFAULT 0,
                                [C_TimeZoneID] [int] NOT NULL DEFAULT 0,
                                [C_EmployeeID] [int] NOT NULL DEFAULT 0,
                                [C_UnitPriceID] [int] NOT NULL DEFAULT 0,
                                [C_WorkingCode] [tinyint] NOT NULL DEFAULT 0,
                                [C_UnitPrice] [decimal](16,0) NOT NULL DEFAULT 0,
                                [C_StandardTime] [smallint] NOT NULL DEFAULT 0,
                                [C_MinutePrice] [decimal](16,0) NOT NULL DEFAULT 0,
                                [C_ErrFlg] [tinyint] NOT NULL DEFAULT 0,
                                [C_Err] [char](70) NOT NULL DEFAULT SPACE(70),
                                CONSTRAINT [PK_T_tbAttendanceImport] PRIMARY KEY NONCLUSTERED ([C_RowID])
                            )";
                        using (var cmd = new SqlCommand(createSql, conn))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// T_tbAttendance テーブルに必要な列が存在するか確認し、不足している場合は追加します。
        /// </summary>
        private void UpdateAttendanceTableSchema()
        {
            var columnsToAdd = new Dictionary<string, string>
            {
                { "C_LateStartTime", "bigint NOT NULL DEFAULT 0" },
                { "C_LateEndTime", "bigint NOT NULL DEFAULT 0" },
                { "C_LateTime", "smallint NOT NULL DEFAULT 0" },
                { "C_LateMoney", "decimal(16,0) NOT NULL DEFAULT 0" },
                { "C_PaymentMoney", "decimal(16,0) NOT NULL DEFAULT 0" },
                { "C_UsualDutiesCode", "tinyint NOT NULL DEFAULT 0" },
                { "C_UsualDutiesStartTime", "bigint NOT NULL DEFAULT 0" },
                { "C_UsualDutiesEndTime", "bigint NOT NULL DEFAULT 0" },
                { "C_UsualDutiesRestStartTime", "bigint NOT NULL DEFAULT 0" },
                { "C_UsualDutiesRestEndTime", "bigint NOT NULL DEFAULT 0" },
                { "C_UsualDutiesTime", "smallint NOT NULL DEFAULT 0" },
                { "C_UsualDutiesMoney", "decimal(16,0) NOT NULL DEFAULT 0" }
            };

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                foreach (var col in columnsToAdd)
                {
                    string checkColSql = $"SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('T_tbAttendance') AND name = '{col.Key}'";
                    using (var checkCmd = new SqlCommand(checkColSql, conn))
                    {
                        if (checkCmd.ExecuteScalar() == null)
                        {
                            string addColSql = $"ALTER TABLE T_tbAttendance ADD [{col.Key}] {col.Value}";
                            using (var addCmd = new SqlCommand(addColSql, conn))
                            {
                                addCmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
        }
    }
}

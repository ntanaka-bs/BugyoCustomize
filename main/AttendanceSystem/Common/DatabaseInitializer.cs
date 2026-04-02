using System;
using System.Data.SqlClient;

namespace AttendanceSystem.Common
{
    /// <summary>
    /// データベースのテーブル初期化（自動生成）を行うクラスです。
    /// アプリケーション起動時に呼び出し、対象のテーブルが存在しない場合は Create します。
    /// </summary>
    public static class DatabaseInitializer
    {
        public static void EnsureTablesCreated()
        {
            try
            {
                using (var conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();

                    // 1. 日種類テーブル (T_tbDayKind) の作成
                    // C_DayKindID: 自動採番の主キー
                    // C_DayKindCode: 日種類コード
                    // C_DayKindName: 名称
                    // C_DayKindAbbreviationName: 略称
                    string createDayKindTable = @"
                        IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[T_tbDayKind]') AND type in (N'U'))
                        BEGIN
                            CREATE TABLE [dbo].[T_tbDayKind] (
                                [C_DayKindID] [int] IDENTITY(1,1) NOT NULL,
                                [C_DayKindCode] [int] NOT NULL,
                                [C_DayKindName] [nvarchar](20) NULL,
                                [C_DayKindAbbreviationName] [nvarchar](10) NULL,
                                CONSTRAINT [PK_T_tbDayKind] PRIMARY KEY CLUSTERED ([C_DayKindID] ASC)
                            );
                            -- 初期データが存在しない場合は作成することも可能ですが、ここではテーブル枠のみ作成します
                        END";
                    using (var cmd = new SqlCommand(createDayKindTable, conn)) cmd.ExecuteNonQuery();

                    // 2. 職種テーブル (T_tbJob) の作成
                    string createJobTable = @"
                        IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[T_tbJob]') AND type in (N'U'))
                        BEGIN
                            CREATE TABLE [dbo].[T_tbJob](
                                [C_JobID] [int] IDENTITY(1,1) NOT NULL,
                                [C_JobCode] [int] NULL,
                                [C_JobName] [nvarchar](40) NULL,
                                [C_JobAbbreviationName] [nvarchar](6) NULL,
                                CONSTRAINT [PK_T_tbJob] PRIMARY KEY CLUSTERED ([C_JobID] ASC)
                            );
                        END";
                    using (var cmd = new SqlCommand(createJobTable, conn)) cmd.ExecuteNonQuery();

                    // 3. 時間帯テーブル (T_tbTimeZone) の作成
                    string createTimeZoneTable = @"
                        IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[T_tbTimeZone]') AND type in (N'U'))
                        BEGIN
                            CREATE TABLE [dbo].[T_tbTimeZone](
                                [C_TimeZoneID] [int] IDENTITY(1,1) NOT NULL,
                                [C_TimeZoneCode] [int] NULL,
                                [C_TimeZoneName] [nvarchar](100) NULL,
                                [C_TimeZoneAbbreviationName] [nvarchar](50) NULL,
                                [C_OrderCode] [int] NOT NULL DEFAULT 0,
                                CONSTRAINT [PK_T_tbTimeZone] PRIMARY KEY CLUSTERED ([C_TimeZoneID] ASC)
                            );
                        END";
                    using (var cmd = new SqlCommand(createTimeZoneTable, conn)) cmd.ExecuteNonQuery();

                    // 4. 単価管理テーブル (T_tbUnitPrice) の作成
                    // 各種IDをもたせて時給や時間条件を保持するテーブル
                    string createUnitPriceTable = @"
                        IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[T_tbUnitPrice]') AND type in (N'U'))
                        BEGIN
                            CREATE TABLE [dbo].[T_tbUnitPrice] (
                                [C_UnitPriceID] [int] IDENTITY(1,1) NOT NULL,
                                [C_JobID] [int] NOT NULL,
                                [C_DayKindID] [int] NOT NULL,
                                [C_TimeZoneID] [int] NOT NULL,
                                [C_UnitPrice] [int] DEFAULT 0,
                                [C_BasicTime] [int] DEFAULT 0,
                                [C_MinutePrice] [int] DEFAULT 0,
                                [C_StartTime] [int] DEFAULT 0,
                                [C_EndTime] [int] DEFAULT 0,
                                [C_BreakStartTime] [int] DEFAULT 0,
                                [C_BreakEndTime] [int] DEFAULT 0,
                                CONSTRAINT [PK_T_tbUnitPrice] PRIMARY KEY CLUSTERED ([C_UnitPriceID] ASC)
                            );
                        END";
                    using (var cmd = new SqlCommand(createUnitPriceTable, conn)) cmd.ExecuteNonQuery();

                    // 5. 勤怠テーブル (T_tbAttendance) のダミー作成（他テーブルからの参照チェック用）
                    string createAttendanceTable = @"
                        IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[T_tbAttendance]') AND type in (N'U'))
                        BEGIN
                            CREATE TABLE [dbo].[T_tbAttendance] (
                                [C_AttendanceID] [int] IDENTITY(1,1) NOT NULL,
                                [C_UnitPriceID] [int] NULL,
                                [C_DayKindID] [int] NULL,
                                [C_TimeZoneID] [int] NULL,
                                CONSTRAINT [PK_T_tbAttendance] PRIMARY KEY CLUSTERED ([C_AttendanceID] ASC)
                            );
                        END";
                    using (var cmd = new SqlCommand(createAttendanceTable, conn)) cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                // テーブル作成時のエラーを出力
                System.Diagnostics.Debug.WriteLine($"テーブル生成時にエラーが発生しました: {ex.Message}");
            }
        }
    }
}

using System;
using System.Data.SqlClient;

namespace AttendanceSystem.Models
{
    public static class UnitPriceDatabaseHelper
    {
        public static SqlConnection GetConnection()
        {
            return new SqlConnection(AttendanceSystem.Common.DatabaseConfig.ConnectionString);
        }

        public static void EnsureTablesCreated()
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();

                    // T_tbJob
                    string createJobTable = @"
                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'T_tbJob')
                        BEGIN
                            CREATE TABLE T_tbJob (
                                C_JobID INT IDENTITY(1,1) PRIMARY KEY,
                                C_JobCode INT NOT NULL,
                                C_JobName NVARCHAR(100) NOT NULL
                            );
                            -- Insert test data
                            INSERT INTO T_tbJob (C_JobCode, C_JobName) VALUES (99, 'テスト職種');
                        END";
                    using (var cmd = new SqlCommand(createJobTable, conn)) cmd.ExecuteNonQuery();

                    // T_tbDayKind
                    string createDayKindTable = @"
                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'T_tbDayKind')
                        BEGIN
                            CREATE TABLE T_tbDayKind (
                                C_DayKindID INT IDENTITY(1,1) PRIMARY KEY,
                                C_DayKindCode INT NOT NULL,
                                C_DayKindName NVARCHAR(100) NOT NULL,
                                C_DayKindAbbreviationName NVARCHAR(50) NOT NULL
                            );
                            -- Insert test data
                            INSERT INTO T_tbDayKind (C_DayKindCode, C_DayKindName, C_DayKindAbbreviationName) 
                            VALUES (99, 'テスト日種類', 'テスト日種略');
                        END";
                    using (var cmd = new SqlCommand(createDayKindTable, conn)) cmd.ExecuteNonQuery();

                    // T_tbTimeZone
                    string createTimeZoneTable = @"
                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'T_tbTimeZone')
                        BEGIN
                            CREATE TABLE T_tbTimeZone (
                                C_TimeZoneID INT IDENTITY(1,1) PRIMARY KEY,
                                C_TimeZoneCode INT NOT NULL,
                                C_TimeZoneName NVARCHAR(100) NOT NULL,
                                C_TimeZoneAbbreviationName NVARCHAR(50) NOT NULL
                            );
                            -- Insert test data
                            INSERT INTO T_tbTimeZone (C_TimeZoneCode, C_TimeZoneName, C_TimeZoneAbbreviationName) 
                            VALUES (99, 'テスト時間帯', 'テスト時間略');
                        END";
                    using (var cmd = new SqlCommand(createTimeZoneTable, conn)) cmd.ExecuteNonQuery();

                    // T_tbUnitPrice
                    string createUnitPriceTable = @"
                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'T_tbUnitPrice')
                        BEGIN
                            CREATE TABLE T_tbUnitPrice (
                                C_UnitPriceID INT IDENTITY(1,1) PRIMARY KEY,
                                C_JobID INT NOT NULL,
                                C_DayKindID INT NOT NULL,
                                C_TimeZoneID INT NOT NULL,
                                C_UnitPrice INT DEFAULT 0,
                                C_BasicTime INT DEFAULT 0,
                                C_MinutePrice INT DEFAULT 0,
                                C_StartTime INT DEFAULT 0,
                                C_EndTime INT DEFAULT 0,
                                C_BreakStartTime INT DEFAULT 0,
                                C_BreakEndTime INT DEFAULT 0
                            );
                        END";
                    using (var cmd = new SqlCommand(createUnitPriceTable, conn)) cmd.ExecuteNonQuery();

                    // T_tbAttendance
                    string createAttendanceTable = @"
                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'T_tbAttendance')
                        BEGIN
                            CREATE TABLE T_tbAttendance (
                                C_AttendanceID INT IDENTITY(1,1) PRIMARY KEY,
                                C_UnitPriceID INT NOT NULL
                            );
                        END";
                    using (var cmd = new SqlCommand(createAttendanceTable, conn)) cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                // In a real app we'd log this. We just let it silently fail if there's no DB access yet.
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
    }
}

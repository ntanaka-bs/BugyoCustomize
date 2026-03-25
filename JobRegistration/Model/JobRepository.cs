using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Obc.Bs.Windows.UI.SKI.JobRegistration.Model;

namespace Obc.Bs.Windows.UI.SKI.JobRegistration.Model
{
    /// <summary>
    /// 職種データのデータアクセスを担当するリポジトリクラス
    /// </summary>
    public class JobRepository
    {
        // 接続文字列 (ユーザー指定の設定)
        private readonly string _connectionString = "Server=AOKADA-PC\\SQLEXPRESS;Database=TestDB;User ID=sa;Password=Sqlserver2022;TrustServerCertificate=True";

        public JobRepository()
        {
            // 起動時にテーブルが存在するか確認し、なければ作成する
            EnsureTableCreated();
        }

        /// <summary>
        /// 職種テーブル（T_tbJob）の存在を確認し、存在しない場合は自動でCREATEする
        /// </summary>
        private void EnsureTableCreated()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = @"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[T_tbJob]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[T_tbJob](
        [C_JobID] [int] IDENTITY(1,1) NOT NULL,
        [C_JobCode] [int] NULL,
        [C_JobName] [nvarchar](40) NULL,
        [C_JobAbbreviationName] [nvarchar](6) NULL,
        CONSTRAINT [PK_T_tbJob] PRIMARY KEY CLUSTERED ([C_JobID] ASC)
    )
END";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// 全ての職種データを取得する
        /// </summary>
        public List<Job> GetAllJobs()
        {
            var jobs = new List<Job>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = "SELECT C_JobID, C_JobCode, C_JobName, C_JobAbbreviationName FROM T_tbJob ORDER BY C_JobCode";
                using (var command = new SqlCommand(sql, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            jobs.Add(new Job
                            {
                                C_JobID = (int)reader["C_JobID"],
                                C_JobCode = reader["C_JobCode"] as int?,
                                C_JobName = reader["C_JobName"] as string,
                                C_JobAbbreviationName = reader["C_JobAbbreviationName"] as string
                            });
                        }
                    }
                }
            }
            return jobs;
        }

        /// <summary>
        /// データを登録または更新する
        /// </summary>
        public void Save(Job job)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql;
                if (job.C_JobID == 0)
                {
                    // 新規登録
                    sql = "INSERT INTO T_tbJob (C_JobCode, C_JobName, C_JobAbbreviationName) VALUES (@JobCode, @JobName, @AbbrevName)";
                }
                else
                {
                    // 更新 (コードが重複しない前提、またはIDによる更新)
                    sql = "UPDATE T_tbJob SET C_JobCode = @JobCode, C_JobName = @JobName, C_JobAbbreviationName = @AbbrevName WHERE C_JobID = @JobID";
                }

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@JobID", job.C_JobID);
                    command.Parameters.AddWithValue("@JobCode", (object?)job.C_JobCode ?? DBNull.Value);
                    command.Parameters.AddWithValue("@JobName", (object?)job.C_JobName ?? DBNull.Value);
                    command.Parameters.AddWithValue("@AbbrevName", (object?)job.C_JobAbbreviationName ?? DBNull.Value);
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// データを削除する
        /// </summary>
        public void Delete(int jobId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = "DELETE FROM T_tbJob WHERE C_JobID = @JobID";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@JobID", jobId);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}

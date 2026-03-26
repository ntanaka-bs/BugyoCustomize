using System.Data.SqlClient;

namespace JobRegistration.Model
{
    /// <summary>
    /// 職種データのデータアクセスを担当するリポジトリクラスです。
    /// SQL Server への接続と、T_tbJob テーブルの作成・操作（CRUD）を行います。
    /// </summary>
    public class JobRepository
    {
        // データベース接続文字列（ユーザー環境のSQL Server Expressインスタンスを指定）
        private readonly string _connectionString = "Server=AOKADA-PC\\SQLEXPRESS;Database=SKI_AttendanceDB;User ID=sa;Password=Sqlserver2022;TrustServerCertificate=True";

        /// <summary>
        /// コンストラクタ。クラス生成時にデータベースの準備状態を確認します。
        /// </summary>
        public JobRepository()
        {
            // 起動時にテーブルが存在するか確認し、なければ作成または修復する
            EnsureTableCreated();
        }

        /// <summary>
        /// 職種テーブル（T_tbJob）の準備を行います。
        /// 1. テーブルが存在しない場合は、IDENTITY（自動採番）付きで新規作成します。
        /// 2. 既に存在する場合でも、C_JobID 列が IDENTITY 設定になっていない場合は、
        ///    データを保持したままテーブル構造を修正（マイグレーション）します。
        /// </summary>
        private void EnsureTableCreated()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                
                // テーブルの存在チェック
                string checkTableSql = "SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[T_tbJob]') AND type in (N'U')";
                using (var checkCmd = new SqlCommand(checkTableSql, connection))
                {
                    if (checkCmd.ExecuteScalar() == null)
                    {
                        // テーブルがなければ作成
                        string createSql = @"
CREATE TABLE [dbo].[T_tbJob](
    [C_JobID] [int] IDENTITY(1,1) NOT NULL, -- 自動採番のプライマリキー
    [C_JobCode] [int] NULL,                -- 職種コード
    [C_JobName] [nvarchar](40) NULL,       -- 職種名称
    [C_JobAbbreviationName] [nvarchar](6) NULL, -- 職種略称
    CONSTRAINT [PK_T_tbJob] PRIMARY KEY CLUSTERED ([C_JobID] ASC)
)";
                        using (var createCmd = new SqlCommand(createSql, connection))
                        {
                            createCmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        // テーブルがある場合、C_JobID が自動採番設定か確認
                        string checkIdentitySql = "SELECT is_identity FROM sys.columns WHERE object_id = OBJECT_ID('T_tbJob') AND name = 'C_JobID'";
                        using (var identityCmd = new SqlCommand(checkIdentitySql, connection))
                        {
                            object? result = identityCmd.ExecuteScalar();
                            if (result != null && Convert.ToInt32(result) == 0)
                            {
                                // IDENTITY設定でない場合は、正しい構造に作り替える（マイグレーション）
                                string migrateSql = @"
BEGIN TRANSACTION; -- 一連の操作をトランザクション化
IF OBJECT_ID('T_tbJob_Tmp', 'U') IS NOT NULL DROP TABLE T_tbJob_Tmp;

-- 正しい構造（IDENTITYあり）のテンポラリテーブルを作成
CREATE TABLE [dbo].[T_tbJob_Tmp](
    [C_JobID] [int] IDENTITY(1,1) NOT NULL,
    [C_JobCode] [int] NULL,
    [C_JobName] [nvarchar](40) NULL,
    [C_JobAbbreviationName] [nvarchar](6) NULL,
    CONSTRAINT [PK_T_tbJob_Tmp] PRIMARY KEY CLUSTERED ([C_JobID] ASC)
);

-- 既存データを流し込む（IDENTITY_INSERT を使用してID値を維持）
SET IDENTITY_INSERT [dbo].[T_tbJob_Tmp] ON;
INSERT INTO [dbo].[T_tbJob_Tmp] ([C_JobID], [C_JobCode], [C_JobName], [C_JobAbbreviationName])
SELECT [C_JobID], [C_JobCode], [C_JobName], [C_JobAbbreviationName] FROM [T_tbJob];
SET IDENTITY_INSERT [dbo].[T_tbJob_Tmp] OFF;

-- 古いテーブルを削除し、テンポラリを本番用に名称変更
DROP TABLE [T_tbJob];
EXEC sp_rename 'T_tbJob_Tmp', 'T_tbJob';
EXEC sp_rename 'PK_T_tbJob_Tmp', 'PK_T_tbJob';

COMMIT TRANSACTION;";
                                using (var migrateCmd = new SqlCommand(migrateSql, connection))
                                {
                                    migrateCmd.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 全ての職種データをデータベースから取得し、リスト形式で返します。
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
        /// 職種データを保存（新規登録または既存更新）します。
        /// </summary>
        /// <param name="job">保存する職種オブジェクト</param>
        public void Save(Job job)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql;
                if (job.C_JobID == 0)
                {
                    // IDが0なら新規登録：IDENTITY項目以外の3列を挿入
                    sql = "INSERT INTO T_tbJob (C_JobCode, C_JobName, C_JobAbbreviationName) VALUES (@JobCode, @JobName, @AbbrevName)";
                }
                else
                {
                    // IDがあれば更新：該当IDのデータ内容を書き換える
                    sql = "UPDATE T_tbJob SET C_JobCode = @JobCode, C_JobName = @JobName, C_JobAbbreviationName = @AbbrevName WHERE C_JobID = @JobID";
                }

                using (var command = new SqlCommand(sql, connection))
                {
                    // ヌル許容型への対応を含めたパラメータセット
                    command.Parameters.AddWithValue("@JobID", job.C_JobID);
                    command.Parameters.AddWithValue("@JobCode", (object?)job.C_JobCode ?? DBNull.Value);
                    command.Parameters.AddWithValue("@JobName", (object?)job.C_JobName ?? DBNull.Value);
                    command.Parameters.AddWithValue("@AbbrevName", (object?)job.C_JobAbbreviationName ?? DBNull.Value);
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// 指定されたIDの職種データをデータベースから削除します。
        /// </summary>
        /// <param name="jobId">削除対象の内部管理ID(C_JobID)</param>
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

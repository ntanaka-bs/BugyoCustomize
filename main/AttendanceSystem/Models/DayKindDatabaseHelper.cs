using AttendanceSystem.Models;
using System.Data.SqlClient;

namespace AttendanceSystem.Models
{
    /// <summary>
    /// データベース（SQL Server）へのアクセスを担当するヘルパークラスです。
    /// テーブルの自動作成、CRUD（記録・取得・削除）操作、および他テーブルの参照チェックを実装しています。
    /// </summary>
    public class DayKindDatabaseHelper
    {
        // データベース接続文字列（外部ファイルから取得）
        private readonly string _connectionString;

        /// <summary>
        /// コンストラクタ。起動時にテーブルの存在チェックと必要に応じた作成・修正を行います。
        /// </summary>
        public DayKindDatabaseHelper()
        {
            _connectionString = AttendanceSystem.Common.DatabaseConfig.ConnectionString;
            EnsureTableExists();
            SeedInitialData(); // サンプルデータの投入
        }

        /// <summary>
        /// テーブルが空の場合、初期サンプルデータを投入します。
        /// </summary>
        private void SeedInitialData()
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                string checkSql = "SELECT COUNT(1) FROM T_tbDayKind";
                using (var cmd = new SqlCommand(checkSql, conn))
                {
                    if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                    {
                        string insertSql = @"
                            INSERT INTO T_tbDayKind (C_DayKindCode, C_DayKindName, C_DayKindAbbreviationName) VALUES 
                            (1, '平日', '平'),
                            (2, '土曜', '土'),
                            (3, '日曜', '日'),
                            (4, '祝日', '祝'),
                            (5, '振替休日', '振休');";
                        using (var insertCmd = new SqlCommand(insertSql, conn))
                        {
                            insertCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }



        /// <summary>
        /// SQL Server への接続オブジェクトを取得します。
        /// </summary>
        private SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        /// <summary>
        /// 日種類登録用テーブル (T_tbDayKind) の存在を確認し、存在しない場合は作成します。
        /// また、既存のテーブルが自動採番 (IDENTITY) 設定になっていない場合は、データを保持したまま修正します。
        /// </summary>
        private void EnsureTableExists()
        {
            using var conn = GetConnection();
            conn.Open();

            // テーブルが存在するか確認
            string checkTableSql = "SELECT 1 FROM sysobjects WHERE name='T_tbDayKind' AND xtype='U'";
            using var checkCmd = new SqlCommand(checkTableSql, conn);
            if (checkCmd.ExecuteScalar() == null)
            {
                // テーブルが存在しない場合、新規作成（C_DayKindID を主キー、IDENTITY に設定）
                string createSql = @"
                    CREATE TABLE [dbo].[T_tbDayKind](
                        [C_DayKindID] [int] IDENTITY(1,1) NOT NULL,
                        [C_DayKindCode] [int] NOT NULL,
                        [C_DayKindName] [nvarchar](20) NULL,
                        [C_DayKindAbbreviationName] [nvarchar](10) NULL,
                        CONSTRAINT [PK_T_tbDayKind] PRIMARY KEY CLUSTERED ([C_DayKindCode] ASC)
                    )";
                using var cmd = new SqlCommand(createSql, conn);
                cmd.ExecuteNonQuery();
            }
            else
            {
                // テーブルが存在する場合、C_DayKindID が自動採番 (is_identity) 設定か確認
                string checkIdentitySql = "SELECT is_identity FROM sys.columns WHERE object_id = OBJECT_ID('T_tbDayKind') AND name = 'C_DayKindID'";
                using var identityCmd = new SqlCommand(checkIdentitySql, conn);
                object result = identityCmd.ExecuteScalar();
                if (result != null && Convert.ToInt32(result) == 0)
                {
                    // 自動採番設定になっていない場合、データを一時テーブルに避難させて再作成（マイグレーション）
                    try
                    {
                        string migrateSql = @"
                            BEGIN TRANSACTION;
                            IF OBJECT_ID('T_tbDayKind_Tmp', 'U') IS NOT NULL DROP TABLE T_tbDayKind_Tmp;
                            
                            -- 1. 自動採番付きのテンポラリテーブルを作成
                            CREATE TABLE [dbo].[T_tbDayKind_Tmp](
                                [C_DayKindID] [int] IDENTITY(1,1) NOT NULL,
                                [C_DayKindCode] [int] NOT NULL,
                                [C_DayKindName] [nvarchar](20) NULL,
                                [C_DayKindAbbreviationName] [nvarchar](10) NULL,
                                CONSTRAINT [PK_T_tbDayKind_Tmp] PRIMARY KEY CLUSTERED ([C_DayKindCode] ASC)
                            );

                            -- 2. 既存のデータを IDENTITY_INSERT を利用して移行
                            SET IDENTITY_INSERT [dbo].[T_tbDayKind_Tmp] ON;
                            INSERT INTO [dbo].[T_tbDayKind_Tmp] ([C_DayKindID], [C_DayKindCode], [C_DayKindName], [C_DayKindAbbreviationName])
                            SELECT [C_DayKindID], [C_DayKindCode], [C_DayKindName], [C_DayKindAbbreviationName] FROM [T_tbDayKind];
                            SET IDENTITY_INSERT [dbo].[T_tbDayKind_Tmp] OFF;

                            -- 3. 旧テーブルを削除し、テンポラリテーブルを正式名称に変更
                            DROP TABLE [T_tbDayKind];
                            EXEC sp_rename 'T_tbDayKind_Tmp', 'T_tbDayKind';
                            EXEC sp_rename 'PK_T_tbDayKind_Tmp', 'PK_T_tbDayKind';
                            
                            COMMIT TRANSACTION;";
                        using var migrateCmd = new SqlCommand(migrateSql, conn);
                        migrateCmd.ExecuteNonQuery();
                    }
                    catch
                    {
                        // エラー時は呼び出し元に例外をスロー
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// 全ての日種類データを取得します。
        /// </summary>
        public List<DayKind> GetAllDayKinds()
        {
            var list = new List<DayKind>();
            using (var conn = GetConnection())
            {
                conn.Open();
                string sql = "SELECT C_DayKindCode, C_DayKindName, C_DayKindAbbreviationName FROM T_tbDayKind ORDER BY C_DayKindCode";
                using var cmd = new SqlCommand(sql, conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new DayKind
                    {
                        DayKindCode = reader.GetInt32(0),
                        DayKindName = reader.IsDBNull(1) ? "" : reader.GetString(1),
                        DayKindAbbreviationName = reader.IsDBNull(2) ? "" : reader.GetString(2)
                    });
                }
            }
            return list;
        }

        /// <summary>
        /// 日種類データを保存（新規作成または更新）します。
        /// </summary>
        /// <param name="dayKind">保存対象のモデル</param>
        public void SaveDayKind(DayKind dayKind)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using var transaction = conn.BeginTransaction();
                try
                {
                    // 既に同じコードが存在するか確認
                    string checkSql = "SELECT COUNT(1) FROM T_tbDayKind WHERE C_DayKindCode = @Code";
                    using var checkCmd = new SqlCommand(checkSql, conn, transaction);
                    checkCmd.Parameters.AddWithValue("@Code", dayKind.DayKindCode);
                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (count > 0)
                    {
                        // 存在する場合は UPDATE（名称と略称を更新）
                        string updateSql = "UPDATE T_tbDayKind SET C_DayKindName = @Name, C_DayKindAbbreviationName = @Abbrev WHERE C_DayKindCode = @Code";
                        using var updateCmd = new SqlCommand(updateSql, conn, transaction);
                        updateCmd.Parameters.AddWithValue("@Name", dayKind.DayKindName ?? (object)DBNull.Value);
                        updateCmd.Parameters.AddWithValue("@Abbrev", dayKind.DayKindAbbreviationName ?? (object)DBNull.Value);
                        updateCmd.Parameters.AddWithValue("@Code", dayKind.DayKindCode);
                        updateCmd.ExecuteNonQuery();
                    }
                    else
                    {
                        // 存在しない場合は INSERT（新規追加）
                        string insertSql = "INSERT INTO T_tbDayKind (C_DayKindCode, C_DayKindName, C_DayKindAbbreviationName) VALUES (@Code, @Name, @Abbrev)";
                        using var insertCmd = new SqlCommand(insertSql, conn, transaction);
                        insertCmd.Parameters.AddWithValue("@Code", dayKind.DayKindCode);
                        insertCmd.Parameters.AddWithValue("@Name", dayKind.DayKindName ?? (object)DBNull.Value);
                        insertCmd.Parameters.AddWithValue("@Abbrev", dayKind.DayKindAbbreviationName ?? (object)DBNull.Value);
                        insertCmd.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// 指定されたコードを持つ日種類データを取得します。
        /// </summary>
        /// <param name="code">取得対象の日種類コード</param>
        /// <returns>該当するデータがある場合は DayKind オブジェクト、ない場合は null</returns>
        public DayKind GetDayKindByCode(int code)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                string sql = "SELECT C_DayKindCode, C_DayKindName, C_DayKindAbbreviationName FROM T_tbDayKind WHERE C_DayKindCode = @Code";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Code", code);
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return new DayKind
                    {
                        DayKindCode = reader.GetInt32(0),
                        DayKindName = reader.IsDBNull(1) ? "" : reader.GetString(1),
                        DayKindAbbreviationName = reader.IsDBNull(2) ? "" : reader.GetString(2)
                    };
                }
            }
            return null;
        }

        /// <summary>
        /// 日種類データを削除します。
        /// </summary>
        /// <param name="dayKindCode">削除する日種類コード</param>
        public void DeleteDayKind(int dayKindCode)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                string sql = "DELETE FROM T_tbDayKind WHERE C_DayKindCode = @Code";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Code", dayKindCode);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// 指定された日種類が他のテーブル（単価管理・勤怠管理等）で使用されているかチェックします。
        /// </summary>
        /// <param name="dayKindCode">対象の日種類コード</param>
        /// <returns>使用中の場合は true、それ以外は false</returns>
        public bool IsDayKindUsed(int dayKindCode)
        {
            using (var conn = GetConnection())
            {
                conn.Open();

                // 1. C_DayKindCode から内部 ID (C_DayKindID) を取得
                int? dayKindId = null;
                string getIdSql = "SELECT C_DayKindID FROM T_tbDayKind WHERE C_DayKindCode = @Code";
                using (var idCmd = new SqlCommand(getIdSql, conn))
                {
                    idCmd.Parameters.AddWithValue("@Code", dayKindCode);
                    var result = idCmd.ExecuteScalar();
                    if (result != null) dayKindId = Convert.ToInt32(result);
                }

                // ID が見つからない場合は（あり得ないが）使用されていないとみなす
                if (dayKindId == null) return false;

                // 2. 内部 ID を使用して他テーブルでの使用状況をチェック

                // 単価管理テーブルでの使用チェック（テーブルが存在する場合のみ）
                if (TableExists(conn, "T_tbUnitPrice"))
                {
                    string sql = "SELECT COUNT(1) FROM T_tbUnitPrice WHERE C_DayKindID = @ID";
                    using var cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@ID", dayKindId.Value);
                    if (Convert.ToInt32(cmd.ExecuteScalar()) > 0) return true;
                }

                // 勤怠管理テーブルでの使用チェック（テーブルが存在する場合のみ）
                if (TableExists(conn, "T_tbAttendance"))
                {
                    string sql = "SELECT COUNT(1) FROM T_tbAttendance WHERE C_DayKindID = @ID";
                    using var cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@ID", dayKindId.Value);
                    if (Convert.ToInt32(cmd.ExecuteScalar()) > 0) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// データベース内に指定されたテーブル名が存在するかどうかを確認します。
        /// </summary>
        private bool TableExists(SqlConnection conn, string tableName)
        {
            string sql = "SELECT 1 FROM sysobjects WHERE name = @TableName AND xtype = 'U'";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@TableName", tableName);
            return cmd.ExecuteScalar() != null;
        }
    }
}

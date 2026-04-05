using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace AttendanceSystem.Common
{
    /// <summary>
    /// データベース設定を保持する静的クラスです。
    /// 指定されたSQL Serverの接続情報を一元管理します。
    /// </summary>
    public static class DatabaseConfig
    {
        /// <summary>
        /// データベースの接続文字列
        /// </summary>
        public static readonly string ConnectionString;

        /// <summary>
        /// appsettings.json および appsettings.local.json から接続文字列を読み込みます。
        /// </summary>
        static DatabaseConfig()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);

            IConfiguration configuration = builder.Build();
            ConnectionString = configuration.GetConnectionString("DefaultConnection") 
                               ?? throw new InvalidOperationException("DefaultConnection is not configured.");
        }
    }
}

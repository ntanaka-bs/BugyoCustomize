using System;

namespace AttendanceSystem.Common
{
    /// <summary>
    /// データベース設定を保持する静的クラスです。
    /// 指定されたSQL Serverの接続情報を一元管理します。
    /// </summary>
    public static class DatabaseConfig
    {
        // サーバー名：AOKADA-PC\SQLEXPRESS
        // ログイン：sa
        // パスワード：Sqlserver2022
        // 接続するDB：SKI_AttendanceDB
        public static readonly string ConnectionString = @"Server=AOKADA-PC\SQLEXPRESS;Database=SKI_AttendanceDB;User Id=sa;Password=Sqlserver2022;TrustServerCertificate=True;";
    }
}

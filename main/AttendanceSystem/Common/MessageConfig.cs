using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace AttendanceSystem.Common
{
    /// <summary>
    /// アプリケーションで使用するメッセージ設定を保持する静的クラスです。
    /// appsettings.json からメッセージ文字列を読み込みます。
    /// </summary>
    public static class MessageConfig
    {
        private static readonly IConfiguration _configuration;

        static MessageConfig()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);

            _configuration = builder.Build();
        }

        private static string GetMessage(string key) => _configuration[$"Messages:{key}"] ?? string.Empty;

        // 確認メッセージ
        public static string ConfirmSave => GetMessage("ConfirmSave");
        public static string ConfirmDelete => GetMessage("ConfirmDelete");
        public static string ConfirmCancel => GetMessage("ConfirmCancel");
        public static string ConfirmClose => GetMessage("ConfirmClose");

        // 情報メッセージ
        public static string InfoSuccessSave => GetMessage("InfoSuccessSave");
        public static string InfoSuccessDelete => GetMessage("InfoSuccessDelete");

        // エラー・警告メッセージ
        public static string ErrorSaveFailed => GetMessage("ErrorSaveFailed");
        public static string ErrorDeleteFailed => GetMessage("ErrorDeleteFailed");
        public static string WarnInputRequired => GetMessage("WarnInputRequired");
        public static string WarnInputRequiredTZ => GetMessage("WarnInputRequiredTZ");
        public static string WarnInvalidCode => GetMessage("WarnInvalidCode");
        public static string WarnNameRequired => GetMessage("WarnNameRequired");
        public static string WarnUsedInOtherTable => GetMessage("WarnUsedInOtherTable");
        public static string WarnUsedInUnitPrice => GetMessage("WarnUsedInUnitPrice");
        public static string WarnUsedInAttendance => GetMessage("WarnUsedInAttendance");

        // ボタンテキスト
        public static string BtnRegister => GetMessage("BtnRegister");
        public static string BtnClose => GetMessage("BtnClose");
        public static string BtnCancel => GetMessage("BtnCancel");
        public static string BtnDeleteRow => GetMessage("BtnDeleteRow");

        // ダイアログタイトル
        public static string TitleConfirm => GetMessage("TitleConfirm");
        public static string TitleInfo => GetMessage("TitleInfo");
        public static string TitleSuccess => GetMessage("TitleSuccess");
        public static string TitleError => GetMessage("TitleError");
        public static string TitleWarning => GetMessage("TitleWarning");
        public static string TitleInputConfirm => GetMessage("TitleInputConfirm");
        public static string TitleDeleteConfirm => GetMessage("TitleDeleteConfirm");
        public static string TitleDeleteStop => GetMessage("TitleDeleteStop");
    }
}

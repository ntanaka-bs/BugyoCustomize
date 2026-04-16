using System;
using System.Collections.Generic;

namespace AttendanceSystem.Models
{
    /// <summary>
    /// 職種マスタ（T_tbJob）用モデル
    /// </summary>
    public class TtbJob
    {
        /// <summary>
        /// 内部ID
        /// </summary>
        public int C_JobID { get; set; }
        /// <summary>
        /// 職種コード
        /// </summary>
        public int C_JobCode { get; set; }
        /// <summary>
        /// 職種名
        /// </summary>
        public string C_JobName { get; set; } = string.Empty;
    }

    /// <summary>
    /// 日種類マスタ（T_tbDayKind）用モデル
    /// </summary>
    public class TtbDayKind
    {
        /// <summary>
        /// 内部ID
        /// </summary>
        public int C_DayKindID { get; set; }
        /// <summary>
        /// 日種類コード
        /// </summary>
        public int C_DayKindCode { get; set; }
        /// <summary>
        /// 日種類名
        /// </summary>
        public string C_DayKindName { get; set; } = string.Empty;
        /// <summary>
        /// 日種類略称
        /// </summary>
        public string C_DayKindAbbreviationName { get; set; } = string.Empty;
    }

    /// <summary>
    /// 時間帯マスタ（T_tbTimeZone）用モデル
    /// </summary>
    public class TtbTimeZone
    {
        /// <summary>
        /// 内部ID
        /// </summary>
        public int C_TimeZoneID { get; set; }
        /// <summary>
        /// 時間帯コード
        /// </summary>
        public int C_TimeZoneCode { get; set; }
        /// <summary>
        /// 時間帯名
        /// </summary>
        public string C_TimeZoneName { get; set; } = string.Empty;
        /// <summary>
        /// 時間帯略称
        /// </summary>
        public string C_TimeZoneAbbreviationName { get; set; } = string.Empty;
    }

    /// <summary>
    /// 単価設定（T_tbUnitPrice）用モデル
    /// </summary>
    public class TtbUnitPrice
    {
        /// <summary>
        /// 単価用の内部ID
        /// </summary>
        public int C_UnitPriceID { get; set; }
        /// <summary>
        /// 職種マスタID
        /// </summary>
        public int C_JobID { get; set; }
        /// <summary>
        /// 日種類マスタID
        /// </summary>
        public int C_DayKindID { get; set; }
        /// <summary>
        /// 時間帯マスタID
        /// </summary>
        public int C_TimeZoneID { get; set; }
        /// <summary>
        /// 単価
        /// </summary>
        public int C_UnitPrice { get; set; }
        /// <summary>
        /// 基礎時間数
        /// </summary>
        public int C_BasicTime { get; set; }
        /// <summary>
        /// 分単価
        /// </summary>
        public int C_MinutePrice { get; set; }
        /// <summary>
        /// 開始時刻
        /// </summary>
        public int C_StartTime { get; set; }
        /// <summary>
        /// 終了時刻
        /// </summary>
        public int C_EndTime { get; set; }
        /// <summary>
        /// 休憩開始時刻
        /// </summary>
        public int C_BreakStartTime { get; set; }
        /// <summary>
        /// 休憩終了時刻
        /// </summary>
        public int C_BreakEndTime { get; set; }
    }

    /// <summary>
    /// 勤怠データ（T_tbAttendance）用モデル
    /// </summary>
    public class TtbAttendance
    {
        /// <summary>
        /// 勤怠データID
        /// </summary>
        public int C_AttendanceID { get; set; }
        public DateTime C_Date { get; set; }
        public int C_TimeZoneID { get; set; }
        public int C_EmployeeID { get; set; }
        public int C_CenterID { get; set; }
        public int C_JobID { get; set; }
        public int C_DayKindID { get; set; }
        /// <summary>
        /// 単価設定ID
        /// </summary>
        public int C_UnitPriceID { get; set; }
        public int C_WorkingCode { get; set; }
        public decimal C_UnitPrice { get; set; }
        public int C_StandardTime { get; set; }
        public decimal C_MinutePrice { get; set; }
        public long C_OverStartTime { get; set; }
        public long C_OverEndTime { get; set; }
        public long C_BreakStartTime { get; set; }
        public long C_BreakEndTime { get; set; }
        public int C_OverTime { get; set; }
        public decimal C_OverMoney { get; set; }
        
        // 追加列
        public long C_LateStartTime { get; set; }
        public long C_LateEndTime { get; set; }
        public int C_LateTime { get; set; }
        public decimal C_LateMoney { get; set; }
        public decimal C_PaymentMoney { get; set; }

        // 通常勤務関連
        public int C_UsualDutiesCode { get; set; }
        public long C_UsualDutiesStartTime { get; set; }
        public long C_UsualDutiesEndTime { get; set; }
        public long C_UsualDutiesRestStartTime { get; set; }
        public long C_UsualDutiesRestEndTime { get; set; }
        public int C_UsualDutiesTime { get; set; }
        public decimal C_UsualDutiesMoney { get; set; }
    }

    /// <summary>
    /// 勤怠データインポート（T_tbAttendanceImport）用モデル
    /// </summary>
    public class TtbAttendanceImport
    {
        public int C_RowID { get; set; }
        public DateTime C_Date { get; set; }
        public int C_CenterID { get; set; }
        public int C_JobID { get; set; }
        public int C_DayKindID { get; set; }
        public int C_TimeZoneID { get; set; }
        public int C_EmployeeID { get; set; }
        public int C_UnitPriceID { get; set; }
        public int C_WorkingCode { get; set; }
        public decimal C_UnitPrice { get; set; }
        public int C_StandardTime { get; set; }
        public decimal C_MinutePrice { get; set; }
        public int C_ErrFlg { get; set; }
        public string C_Err { get; set; } = string.Empty;
    }

    /// <summary>
    /// DataGrid行表示・編集用明細クラス。マスタと単価情報を保持
    /// </summary>
    public class UnitPriceDetail
    {
        /// <summary>
        /// 日種類マスタの内部ID
        /// </summary>
        public int DayKindID { get; set; }
        /// <summary>
        /// 時間帯マスタの内部ID
        /// </summary>
        public int TimeZoneID { get; set; }
        /// <summary>
        /// 職種マスタの内部ID
        /// </summary>
        public int JobID { get; set; }
        /// <summary>
        /// 単価設定レコードの内部ID（新規の場合は0）
        /// </summary>
        public int UnitPriceID { get; set; }

        /// <summary>
        /// 日種類コード
        /// </summary>
        public string DayKindCode { get; set; } = string.Empty;
        /// <summary>
        /// 日種類名称
        /// </summary>
        public string DayKindName { get; set; } = string.Empty;
        /// <summary>
        /// 時間帯コード
        /// </summary>
        public string TimeZoneCode { get; set; } = string.Empty;
        /// <summary>
        /// 時間帯名称
        /// </summary>
        public string TimeZoneName { get; set; } = string.Empty;
        
        /// <summary>
        /// 単価
        /// </summary>
        public int UnitPrice { get; set; }
        /// <summary>
        /// 基礎時間(分)
        /// </summary>
        public int BasicTime { get; set; }
        /// <summary>
        /// 分単価
        /// </summary>
        public int MinutePrice { get; set; }
        /// <summary>
        /// 開始時間（例: 900 -> 9:00）
        /// </summary>
        public int StartTime { get; set; }
        /// <summary>
        /// 終了時間（例: 1730 -> 17:30）
        /// </summary>
        public int EndTime { get; set; }
        /// <summary>
        /// 休憩開始時間
        /// </summary>
        public int BreakStartTime { get; set; }
        /// <summary>
        /// 休憩終了時間
        /// </summary>
        public int BreakEndTime { get; set; }

        /// <summary>
        /// 開始時間の画面表示用文字列
        /// </summary>
        public string StartTimeString => FormatTime(StartTime);
        /// <summary>
        /// 終了時間の画面表示用文字列
        /// </summary>
        public string EndTimeString => FormatTime(EndTime);
        /// <summary>
        /// 休憩開始時間の画面表示用文字列
        /// </summary>
        public string BreakStartTimeString => FormatTime(BreakStartTime);
        /// <summary>
        /// 休憩終了時間の画面表示用文字列
        /// </summary>
        public string BreakEndTimeString => FormatTime(BreakEndTime);

        /// <summary>
        /// 整数型時刻（HHMM）を "HH : MM" 形式にフォーマット
        /// </summary>
        /// <param name="time">時刻（例: 900）</param>
        /// <returns>"09 : 00"</returns>
        public static string FormatTime(int time)
        {
            if (time == 0) return "00 : 00";
            return $"{time / 100:D2} : {time % 100:D2}";
        }
    }
}

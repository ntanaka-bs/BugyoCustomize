using System;

namespace AttendanceSystem.Models
{
    /// <summary>
    /// 日種類データを保持するモデルクラスです。
    /// T_tbDayKind テーブルのレコードに対応します。
    /// </summary>
    public class DayKind
    {
        /// <summary>
        /// 日種類コード (主キー)
        /// </summary>
        public int DayKindCode { get; set; }

        /// <summary>
        /// 日種類名称
        /// </summary>
        public string DayKindName { get; set; } = string.Empty;

        /// <summary>
        /// 日種類略称
        /// </summary>
        public string DayKindAbbreviationName { get; set; } = string.Empty;
        
        /// <summary>
        /// 表示用にフォーマットされたコード (2桁ゼロ埋め)
        /// </summary>
        public string FormattedCode => DayKindCode.ToString("D2");
        
        /// <summary>
        /// オブジェクトのコピーを作成します。
        /// </summary>
        public DayKind Clone()
        {
            return new DayKind
            {
                DayKindCode = this.DayKindCode,
                DayKindName = this.DayKindName,
                DayKindAbbreviationName = this.DayKindAbbreviationName
            };
        }
    }
}

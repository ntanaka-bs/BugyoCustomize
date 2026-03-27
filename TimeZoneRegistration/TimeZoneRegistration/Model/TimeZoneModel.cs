namespace TimeZoneRegistration.Model
{
    /// <summary>
    /// 時間帯区分モデル (T_tbTimeZone用)
    /// </summary>
    public class TimeZoneModel
    {
        /// <summary>
        /// 内部ID
        /// </summary>
        public int C_TimeZoneID { get; set; }

        /// <summary>
        /// 時間帯区分コード
        /// </summary>
        public int? C_TimeZoneCode { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string? C_TimeZoneName { get; set; }

        /// <summary>
        /// 略称
        /// </summary>
        public string? C_TimeZoneAbbreviationName { get; set; }

        /// <summary>
        /// 出力順
        /// </summary>
        public int? C_OrderCode { get; set; }
    }
}

namespace WB.UI.Headquarters.Services
{
    public class AudioAuditSegmentInfo
    {
        public string SegmentId { get; set; } = string.Empty;
        public int SequenceNumber { get; set; }

        /// <summary>
        /// Raw device-local timestamp string parsed from the filename (yyyyMMdd_HHmmssfff), or null
        /// when the filename does not match the expected pattern.
        /// </summary>
        public string DeviceLocalStartTime { get; set; }

        public string ContentType { get; set; }
    }
}

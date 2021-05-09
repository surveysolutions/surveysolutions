using System;
using System.Collections.Generic;

namespace WB.ServicesIntegration.Export
{
    public class InterviewHistoricalRecordView
    {
        public InterviewHistoricalAction Action { get; set; }
        public string OriginatorName { get; set; } = String.Empty;
        public string OriginatorRole { get; set; } = String.Empty;
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
        public DateTime? Timestamp { get; set; }
        public TimeSpan? Offset { get; set; }
    }
}

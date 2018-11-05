using System;
using System.Collections.Generic;

namespace WB.Services.Export.Interview
{
    public class InterviewHistoricalRecordView
    {
        public InterviewHistoricalAction Action { get; set; }
        public string OriginatorName { get; set; }
        public string OriginatorRole { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
        public DateTime? Timestamp { get; set; }
        public TimeSpan? Offset { get; set; }
    }
}

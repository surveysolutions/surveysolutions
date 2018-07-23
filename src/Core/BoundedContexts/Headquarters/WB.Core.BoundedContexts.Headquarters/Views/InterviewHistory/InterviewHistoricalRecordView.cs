using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory
{
    public class InterviewHistoricalRecordView
    {
        public InterviewHistoricalRecordView(long index, InterviewHistoricalAction action, string originatorName, 
            string originatorRole, Dictionary<string, string> parameters, DateTime? timestamp, TimeSpan? offset)
        {
            this.Index = index;
            this.Action = action;
            this.OriginatorName = originatorName;
            this.OriginatorRole = originatorRole;
            this.Parameters = parameters;
            this.Timestamp = timestamp;
            this.Offset = offset;
        }
        public long  Index { get; private set; }
        public InterviewHistoricalAction Action { get; private set; }
        public string OriginatorName { get; private set; }
        public string OriginatorRole { get; private set; }
        public Dictionary<string, string> Parameters { get; private set; }
        public DateTime? Timestamp { get; private set; }
        public TimeSpan? Offset { get; set; }
    }
}

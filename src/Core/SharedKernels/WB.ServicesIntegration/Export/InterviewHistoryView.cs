using System;
using System.Collections.Generic;

namespace WB.ServicesIntegration.Export
{
    public class InterviewHistoryView
    {
        public Guid InterviewId { get; set; }
        public List<InterviewHistoricalRecordView> Records { get; set; } = new List<InterviewHistoricalRecordView>();
    }
}

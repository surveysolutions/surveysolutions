using System;
using System.Collections.Generic;

namespace WB.Services.Export.Interview
{
    public class InterviewHistoryView
    {
        public Guid InterviewId { get; set; }
        public List<InterviewHistoricalRecordView> Records { get; set; }
    }
}

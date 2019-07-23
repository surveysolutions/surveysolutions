using System;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog;

namespace WB.Core.BoundedContexts.Headquarters.InterviewerAuditLog
{
    public class InterviewerAuditLogResult
    {
        public DateTime? NextBatchRecordDate { get; set; }
        public InterviewerAuditLogDateRecords[] Records { get; set; }
    }
    
    public class InterviewerAuditLogDateRecords
    {
        public DateTime Date { get; set; }
        public InterviewerAuditLogRecord[] RecordsByDate { get; set; }
    }

    public class InterviewerAuditLogRecord
    {
        public DateTime Time { get; set; }
        public AuditLogEntityType Type { get; set; }
        public string Message { get; set; }
        public string Description { get; set; }
    }
}

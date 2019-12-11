using System;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog;

namespace WB.Core.BoundedContexts.Headquarters.Users.UserProfile.InterviewerAuditLog
{
    public class AuditLogQueryResult
    {
        public DateTime? NextBatchRecordDate { get; set; }
        public AuditLogRecordItem[] Items { get; set; }
    }
    
    public class AuditLogRecordItem
    {
        public DateTime Time { get; set; }
        public AuditLogEntityType Type { get; set; }
        public string Message { get; set; }
        public string Description { get; set; }
    }
}

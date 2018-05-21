using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.InterviewerAuditLog
{
    public class AuditLogResult
    {
        public IEnumerable<AuditLogRecord> Records { get; set; }
        public DateTime? NextBatchRecordDate { get; set; }
    }
}

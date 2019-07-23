using System;
using System.Collections;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.InterviewerAuditLog
{
    public interface IAuditLogService
    {
        AuditLogQueryResult GetLastExisted7DaysRecords(Guid id, DateTime? startDateTime = null, bool showErrorMessage = false);
        IEnumerable<AuditLogRecordItem> GetAddRecords(Guid id, bool showErrorMessage = false);
    }
}

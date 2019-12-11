using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.Users.UserProfile.InterviewerAuditLog
{
    public interface IAuditLogService
    {
        AuditLogQueryResult GetLastExisted7DaysRecords(Guid id, DateTime? startDateTime = null, bool showErrorMessage = false);
        IEnumerable<AuditLogRecordItem> GetAllRecords(Guid id, bool showErrorMessage = false);
        IEnumerable<AuditLogRecordItem> GetRecords(Guid id, DateTime start, DateTime end, bool showErrorMessage = false);
    }
}

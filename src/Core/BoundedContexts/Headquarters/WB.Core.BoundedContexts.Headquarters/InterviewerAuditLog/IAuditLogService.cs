using System;

namespace WB.Core.BoundedContexts.Headquarters.InterviewerAuditLog
{
    public interface IAuditLogService
    {
        InterviewerAuditLogResult GetLastExisted7DaysRecords(Guid id, DateTime? startDateTime = null, bool showErrorMessage = false);
        byte[] GenerateTabFile(Guid id, bool showErrorMessage = false);
    }
}
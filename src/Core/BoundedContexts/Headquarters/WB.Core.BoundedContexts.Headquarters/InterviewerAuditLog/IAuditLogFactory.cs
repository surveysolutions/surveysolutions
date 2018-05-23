using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.InterviewerAuditLog
{
    public interface IAuditLogFactory
    {
        IEnumerable<AuditLogRecord> GetRecordsFor7Days(Guid responsibleId);
        IEnumerable<AuditLogRecord> GetRecords(Guid responsibleId);
        IEnumerable<AuditLogRecord> GetRecords(Guid responsibleId, DateTime startDateTime, DateTime endDateTime);
        AuditLogResult GetLastExisted7DaysRecords(Guid responsibleId, DateTime dateTime);
    }
}

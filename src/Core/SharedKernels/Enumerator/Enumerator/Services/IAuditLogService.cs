using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IAuditLogService
    {
        void Write(IAuditLogEntity payload);
        void UpdateLastSyncIndex(int id);
        IEnumerable<AuditLogEntityView> GetAuditLogEntitiesForSync();
        void WriteAuditLogRecord(AuditLogEntityView auditLogEntityView);

        IEnumerable<AuditLogEntityView> GetAllAuditLogEntities();
    }
}

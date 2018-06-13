using System.Collections.Generic;
using WB.Core.BoundedContexts.Interviewer.Implementation.AuditLog;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog;

namespace WB.Core.BoundedContexts.Interviewer.Services
{
    public interface IAuditLogService
    {
        void Write(IAuditLogEntity payload);
        void UpdateLastSyncIndex(int id);
        IEnumerable<AuditLogEntityView> GetAuditLogEntitiesForSync();
    }
}

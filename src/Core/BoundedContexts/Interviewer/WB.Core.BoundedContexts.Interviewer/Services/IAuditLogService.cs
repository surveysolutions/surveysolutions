using WB.Core.BoundedContexts.Interviewer.Implementation.AuditLog;

namespace WB.Core.BoundedContexts.Interviewer.Services
{
    public interface IAuditLogService
    {
        void Write(IAuditLogEntity payload);
    }
}

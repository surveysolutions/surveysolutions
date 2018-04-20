namespace WB.Core.BoundedContexts.Interviewer.Implementation.AuditLog.Entities
{
    public class SynchronizationCanceledAuditLogEntity : BaseAuditLogEntity
    {
        public SynchronizationCanceledAuditLogEntity() : base(AuditLogEntityType.SynchronizationCanceled)
        {
        }
    }
}

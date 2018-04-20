namespace WB.Core.BoundedContexts.Interviewer.Implementation.AuditLog.Entities
{
    public abstract class BaseAuditLogEntity : IAuditLogEntity
    {
        protected BaseAuditLogEntity(AuditLogEntityType type)
        {
            Type = type;
        }

        public AuditLogEntityType Type { get; }
    }
}

namespace WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities
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

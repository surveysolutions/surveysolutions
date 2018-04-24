namespace WB.Core.SharedKernels.DataCollection.Views.AuditLog.Entities
{
    public class SynchronizationStartedAuditLogEntity : BaseAuditLogEntity
    {
        public SynchronizationStartedAuditLogEntity() : base(AuditLogEntityType.SynchronizationStarted)
        {
        }
    }
}

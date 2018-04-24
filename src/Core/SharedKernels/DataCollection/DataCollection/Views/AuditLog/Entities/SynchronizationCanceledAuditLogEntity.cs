namespace WB.Core.SharedKernels.DataCollection.Views.AuditLog.Entities
{
    public class SynchronizationCanceledAuditLogEntity : BaseAuditLogEntity
    {
        public SynchronizationCanceledAuditLogEntity() : base(AuditLogEntityType.SynchronizationCanceled)
        {
        }
    }
}

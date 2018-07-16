namespace WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities
{
    public class SynchronizationStartedAuditLogEntity : BaseAuditLogEntity
    {
        public SynchronizationType SynchronizationType { get; }

        public SynchronizationStartedAuditLogEntity(SynchronizationType synchronizationType) : base(AuditLogEntityType.SynchronizationStarted)
        {
            SynchronizationType = synchronizationType;
        }
    }
}

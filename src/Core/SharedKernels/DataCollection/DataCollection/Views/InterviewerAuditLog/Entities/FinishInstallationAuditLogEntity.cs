namespace WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities
{
    public class FinishInstallationAuditLogEntity : BaseAuditLogEntity
    {
        public FinishInstallationAuditLogEntity(string serverUrl) : base(AuditLogEntityType
            .FinishInstallation)
        {
            this.ServerUrl = serverUrl;
        }

        public string ServerUrl { get; set; }
    }
}

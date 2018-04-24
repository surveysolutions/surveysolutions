namespace WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities
{
    public class LoginAuditLogEntity : BaseAuditLogEntity
    {
        public LoginAuditLogEntity() : base(AuditLogEntityType.Login)
        {
        }
    }
}

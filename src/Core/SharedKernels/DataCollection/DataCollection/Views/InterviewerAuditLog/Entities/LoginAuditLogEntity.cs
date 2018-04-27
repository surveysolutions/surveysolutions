namespace WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities
{
    public class LoginAuditLogEntity : BaseAuditLogEntity
    {
        public LoginAuditLogEntity(string userName) : base(AuditLogEntityType.Login)
        {
            UserName = userName;
        }

        public string UserName { get; }
    }
}

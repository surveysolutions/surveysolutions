namespace WB.Core.BoundedContexts.Interviewer.Implementation.AuditLog.Entities
{
    public class LoginAuditLogEntity : BaseAuditLogEntity
    {
        public LoginAuditLogEntity() : base(AuditLogEntityType.Login)
        {
        }
    }
}

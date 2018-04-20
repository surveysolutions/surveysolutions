namespace WB.Core.BoundedContexts.Interviewer.Implementation.AuditLog.Entities
{
    public class LogoutAuditLogEntity : BaseAuditLogEntity
    {
        public LogoutAuditLogEntity() : base(AuditLogEntityType.Logout)
        {
        }
    }
}

namespace WB.Core.SharedKernels.DataCollection.Views.AuditLog.Entities
{
    public class LogoutAuditLogEntity : BaseAuditLogEntity
    {
        public LogoutAuditLogEntity() : base(AuditLogEntityType.Logout)
        {
        }
    }
}

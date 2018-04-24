namespace WB.Core.SharedKernels.DataCollection.Views.AuditLog.Entities
{
    public class LoginAuditLogEntity : BaseAuditLogEntity
    {
        public LoginAuditLogEntity() : base(AuditLogEntityType.Login)
        {
        }
    }
}

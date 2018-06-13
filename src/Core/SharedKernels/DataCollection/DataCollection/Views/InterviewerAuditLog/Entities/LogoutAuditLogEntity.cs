namespace WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities
{
    public class LogoutAuditLogEntity : BaseAuditLogEntity
    {
        public LogoutAuditLogEntity(string userName) : base(AuditLogEntityType.Logout)
        {
            UserName = userName;
        }

        public string UserName { get;  }
    }
}

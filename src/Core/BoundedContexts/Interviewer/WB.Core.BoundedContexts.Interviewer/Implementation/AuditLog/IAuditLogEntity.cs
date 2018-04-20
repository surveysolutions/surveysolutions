namespace WB.Core.BoundedContexts.Interviewer.Implementation.AuditLog
{
    public interface IAuditLogEntity
    {
        AuditLogEntityType Type { get; }
    }
}

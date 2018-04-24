namespace WB.Core.SharedKernels.DataCollection.Views.AuditLog
{
    public interface IAuditLogEntity
    {
        AuditLogEntityType Type { get; }
    }
}

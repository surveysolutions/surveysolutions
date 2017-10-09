namespace WB.Core.BoundedContexts.Headquarters.Services
{
    public interface IAuditLog
    {
        void Append(string message);
    }
}
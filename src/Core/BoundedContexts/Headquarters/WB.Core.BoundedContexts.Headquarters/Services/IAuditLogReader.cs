namespace WB.Core.BoundedContexts.Headquarters.Services
{
    public interface IAuditLogReader
    {
        string[] Read();
        string GetServerFilePath();
    }
}
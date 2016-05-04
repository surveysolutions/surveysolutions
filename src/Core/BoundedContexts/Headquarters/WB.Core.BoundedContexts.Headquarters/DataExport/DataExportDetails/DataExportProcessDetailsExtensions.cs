using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails
{
    public static class DataExportProcessDetailsExtensions
    {
        public static bool IsQueuedOrRunning(this IDataExportProcessDetails process)
        {
            return process.Status == DataExportStatus.Queued || process.Status == DataExportStatus.Running;
        }
    }
}
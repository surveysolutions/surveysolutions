using System;
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

    public interface IDataExportProcessDetails
    {
        string NaturalId { get; }
        string Name { get; }
        DataExportFormat Format { get; }
        DateTime BeginDate { get; }

        DateTime LastUpdateDate { get; set; }
        DataExportStatus Status { get; set; }
        int ProgressInPercents { get; set; }
    }
}
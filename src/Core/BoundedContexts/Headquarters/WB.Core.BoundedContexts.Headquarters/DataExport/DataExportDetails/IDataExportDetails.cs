using System;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails
{
    public interface IDataExportDetails
    {
        string ProcessId { get; }
        string ProcessName { get; }
        DataExportFormat Format { get; }
        DateTime BeginDate { get; }

        DateTime LastUpdateDate { get; set; }
        DataExportStatus Status { get; set; }
        int ProgressInPercents { get; set; }
    }
}
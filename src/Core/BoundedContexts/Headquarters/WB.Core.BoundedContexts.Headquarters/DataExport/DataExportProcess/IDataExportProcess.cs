using System;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.DataExportProcess
{
    public interface IDataExportProcess
    {
        string DataExportProcessId { get; set; }
        string DataExportProcessName { get; set; }
        DateTime BeginDate { get; set; }
        DateTime LastUpdateDate { get; set; }
        DataExportFormat DataExportFormat { get; set; }
        DataExportStatus Status { get; set; }
        int ProgressInPercents { get; set; }
    }
}
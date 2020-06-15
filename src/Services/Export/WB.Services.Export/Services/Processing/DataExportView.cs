using System;
using WB.Services.Export.Models;

namespace WB.Services.Export.Services.Processing
{
    public class DataExportView
    {
        public long ProcessId { get; set; }
        public DataExportFormat DataExportFormat { get; set; }
        public DataExportType DataExportType { get; set; }
        public bool HasDataToExport { get; set; }
        public bool HasAnyDataToBePrepared { get; set; }
        public bool CanRefreshBeRequested { get; set; }
        public DateTime? LastUpdateDate { get; set; }
        public DateTime? EndDate { get; set; }
        public double FileSize { get; set; }
        public int ProgressInPercents { get; set; }
        public string? DataExportProcessId { get; set; }
        public DataExportStatus StatusOfLatestExportProcess { get; set; }
        public TimeSpan? TimeEstimation { get; set; }
        public DataExportErrorView? Error { get; set; }
    }
}

using System;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Views
{
    public class DataExportView
    {
        public DataExportView()
        {
        }
        public DataExportFormat DataExportFormat { get; set; }
        public DataExportType DataExportType { get; set; }
        public bool HasDataToExport { get; set; }
        public bool HasAnyDataToBePrepared { get; set; }
        public bool CanRefreshBeRequested { get; set; }
        public DateTime? LastUpdateDate { get; set; }
        public long FileSize { get; set; }
        public int ProgressInPercents { get; set; }
        public string DataExportProcessId { get; set; }
        public DataExportStatus StatusOfLatestExportProcess { get; set; }
    }
}
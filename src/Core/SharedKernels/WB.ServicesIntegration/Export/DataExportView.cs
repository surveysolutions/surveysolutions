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
        public double FileSize { get; set; }
        public int ProgressInPercents { get; set; }
        public string DataExportProcessId { get; set; }
        public DataExportStatus StatusOfLatestExportProcess { get; set; }
        public TimeSpan? TimeEstimation { get; set; }
        
        public string TimeLeft
        {
            get
            {
                if (TimeEstimation == null || TimeEstimation.Value.TotalSeconds < 1)
                {
                    return string.Empty;
                }

                return Report.TimeLeft + ": " + TimeEstimation.Value.Humanize(2, minUnit: TimeUnit.Second);
            }
        }

        public DataExportErrorView Error { get; set; }
    }
}

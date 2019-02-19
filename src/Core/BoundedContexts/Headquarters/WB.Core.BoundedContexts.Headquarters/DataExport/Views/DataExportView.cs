using System;
using Humanizer;
using Humanizer.Localisation;
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
                if (TimeEstimation == null)
                {
                    return string.Empty;
                }

                return TimeEstimation.Value.Humanize(1, minUnit: TimeUnit.Second);
            }
        }
    }
}

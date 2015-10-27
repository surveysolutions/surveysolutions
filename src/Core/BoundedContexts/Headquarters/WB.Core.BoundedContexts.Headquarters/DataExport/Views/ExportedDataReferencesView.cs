using System;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Views
{
    public class ExportedDataReferencesView
    {
        public ExportedDataReferencesView()
        {
        }

        public ExportedDataReferencesView(string exportedDataReferenceId, DataExportType dataExportType, bool hasDataToExport, DateTime lastUpdateDate, int progressInPercents)
        {
            this.DataExportType = dataExportType;
            this.HasDataToExport = hasDataToExport;
            this.LastUpdateDate = lastUpdateDate;
            this.ProgressInPercents = progressInPercents;
        }
        public string ExportedDataReferenceId { get; set; }
        public DataExportType DataExportType { get; set; }
        public bool HasDataToExport { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public int ProgressInPercents { get; set; }
    }
}
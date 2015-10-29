using System;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Views
{
    public class ExportedDataReferencesView
    {
        public ExportedDataReferencesView()
        {
        }
        public DataExportFormat DataExportFormat { get; set; }
        public bool HasDataToExport { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public int ProgressInPercents { get; set; }
        public DataExportStatus StatusOfLatestExportprocess { get; set; }
    }
}
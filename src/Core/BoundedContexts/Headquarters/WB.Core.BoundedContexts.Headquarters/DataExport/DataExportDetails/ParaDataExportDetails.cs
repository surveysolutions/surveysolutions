using System;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails
{
    public class ParaDataExportDetails: IDataExportDetails
    {
        public string DataExportProcessId { get; set; }
        public string DataExportProcessName { get; set; }
        public DateTime BeginDate { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public DataExportFormat DataExportFormat { get; set; }
        public DataExportStatus Status { get; set; }
        public int ProgressInPercents { get; set; }
    }
}
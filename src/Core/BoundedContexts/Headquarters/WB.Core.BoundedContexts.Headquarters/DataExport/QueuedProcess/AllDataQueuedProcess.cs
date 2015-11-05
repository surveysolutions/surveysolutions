using System;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.QueuedProcess
{
    public class AllDataQueuedProcess: IQueuedProcess
    {
        public Guid QuestionnaireId { get; set; }
        public long QuestionnaireVersion { get; set; }
        public string DataExportProcessId { get; set; }
        public DateTime BeginDate { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public DataExportFormat DataExportFormat { get; set; }
        public DataExportStatus Status { get; set; }
        public int ProgressInPercents { get; set; }
    }
}
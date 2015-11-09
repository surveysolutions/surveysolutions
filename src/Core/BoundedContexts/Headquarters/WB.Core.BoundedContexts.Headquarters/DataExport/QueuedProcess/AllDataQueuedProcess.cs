using System;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.QueuedProcess
{
    public class AllDataQueuedProcess: IQueuedProcess
    {
        public QuestionnaireIdentity QuestionnaireIdentity { get; set; }
        public string DataExportProcessId { get; set; }
        public DateTime BeginDate { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public DataExportFormat DataExportFormat { get; set; }
        public DataExportStatus Status { get; set; }
        public int ProgressInPercents { get; set; }
    }
}
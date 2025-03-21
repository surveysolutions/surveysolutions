using System;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Views
{
    public class DataExportStatusView
    {
        public DataExportStatusView()
        {
            
        }

        public DataExportStatusView(
            string questionnaireId,
            DataExportView[] dataExports,
            RunningDataExportProcessView[] runningDataExportProcesses)
        {
            this.DataExports = dataExports;
            this.RunningDataExportProcesses = runningDataExportProcesses;
            this.QuestionnaireIdentity = QuestionnaireIdentity.Parse(questionnaireId);
        }

        QuestionnaireIdentity QuestionnaireIdentity { get; set; }

        public DataExportView[] DataExports { get; set; }
        public RunningDataExportProcessView[] RunningDataExportProcesses { get; set; }

        public bool Success { get; set; }
    }

    public class ExportDataAvailabilityView
    {
        public bool HasInterviews { get; set; }
        public bool HasBinaryData { get; set; }
        public bool HasAudioAudit { get; set; }
    }

    public class DataExportUpdateRequestResult
    {
        public long JobId { get; set; }
    }
}

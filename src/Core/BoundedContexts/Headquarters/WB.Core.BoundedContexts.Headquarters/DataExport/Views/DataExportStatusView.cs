using System;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Views
{
    public class DataExportStatusView
    {
        public DataExportStatusView(
            Guid questionnaireId, 
            long questionnaireVersion, 
            DataExportView[] dataExports,
            RunningDataExportProcessView[] runningDataExportProcesses)
        {
            this.QuestionnaireId = questionnaireId;
            this.QuestionnaireVersion = questionnaireVersion;
            this.DataExports = dataExports;
            this.RunningDataExportProcesses = runningDataExportProcesses;
        }

        public Guid QuestionnaireId { get; private set; }
        public long QuestionnaireVersion { get; private set; }
        public DataExportView[] DataExports { get; private set; }
        public RunningDataExportProcessView[] RunningDataExportProcesses { get; private set; }
    }
}

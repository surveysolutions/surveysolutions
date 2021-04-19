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
}

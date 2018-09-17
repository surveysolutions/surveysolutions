using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.Services.Processing
{
    public class DataExportStatusView
    {
        public DataExportStatusView(
            QuestionnaireId questionnaireId,
            DataExportView[] dataExports,
            RunningDataExportProcessView[] runningDataExportProcesses)
        {
            QuestionnaireId = questionnaireId;
            this.DataExports = dataExports;
            this.RunningDataExportProcesses = runningDataExportProcesses;
        }

        public QuestionnaireId QuestionnaireId { get; }
        public DataExportView[] DataExports { get;  }
        public RunningDataExportProcessView[] RunningDataExportProcesses { get; }
    }
}

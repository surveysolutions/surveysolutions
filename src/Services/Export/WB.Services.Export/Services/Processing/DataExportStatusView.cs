using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.Services.Processing
{
    public class DataExportStatusView
    {
        public DataExportStatusView(
            string questionnaireId,
            DataExportView[] dataExports,
            RunningDataExportProcessView[] runningDataExportProcesses)
        {
            QuestionnaireId = questionnaireId;
            this.DataExports = dataExports;
            this.RunningDataExportProcesses = runningDataExportProcesses;
        }

        public string QuestionnaireId { get; set;  }
        public DataExportView[] DataExports { get; set; }
        public RunningDataExportProcessView[] RunningDataExportProcesses { get; set; }
    }
}

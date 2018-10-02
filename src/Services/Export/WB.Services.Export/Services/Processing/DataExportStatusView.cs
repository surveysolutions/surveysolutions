using System.Collections.Generic;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.Services.Processing
{
    public class DataExportStatusView
    {
        public DataExportStatusView()
        {
        }

        public DataExportStatusView(
            string questionnaireId,
            List<DataExportView> dataExports,
            RunningDataExportProcessView[] runningDataExportProcesses)
        {
            QuestionnaireId = questionnaireId;
            this.DataExports = dataExports;
            this.RunningDataExportProcesses = runningDataExportProcesses;
        }

        public string QuestionnaireId { get; set;  }
        public List<DataExportView> DataExports { get; set; }
        public RunningDataExportProcessView[] RunningDataExportProcesses { get; set; }
    }
}

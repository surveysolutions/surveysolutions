using System;
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
            DataExportProcessView[] runningDataExportProcesses)
        {
            QuestionnaireId = questionnaireId;
            this.DataExports = dataExports;
            this.RunningDataExportProcesses = runningDataExportProcesses;
        }

        public string QuestionnaireId { get; set;  }
        public List<DataExportView> DataExports { get; set; }
        public DataExportProcessView[] RunningDataExportProcesses { get; set; }
    }

    public class DataExportUpdateRequestResult
    {
        public long JobId { get; set; }
    }

    public class ExportFileInfoView
    {
        public DateTime LastUpdateDate { get; set; }
        public double FileSize { get; set; }
        public bool HasFile { get; set; }
    }

}

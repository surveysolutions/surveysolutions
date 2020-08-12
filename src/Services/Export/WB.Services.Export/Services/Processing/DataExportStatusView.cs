using System;
using System.Collections.Generic;

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

        public string QuestionnaireId { get; set;  } = String.Empty;
        public List<DataExportView> DataExports { get; set; } = new List<DataExportView>();
        public DataExportProcessView[] RunningDataExportProcesses { get; set; } = new DataExportProcessView[0];
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

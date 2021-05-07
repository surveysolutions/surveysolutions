using System.Collections.Generic;

namespace WB.ServicesIntegration.Export
{
    public class DataExportStatusView
    {
        public string? QuestionnaireId { get; set; }

        public List<DataExportView>? DataExports { get; set; }
        public List<RunningDataExportProcessView>? RunningDataExportProcesses { get; set; }

        public bool Success { get; set; }
    }
}

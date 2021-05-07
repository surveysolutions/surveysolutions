namespace WB.ServicesIntegration.Export
{
    public class DataExportStatusView
    {
        QuestionnaireIdentity? QuestionnaireIdentity { get; set; }

        public DataExportView[]? DataExports { get; set; }
        public RunningDataExportProcessView[]? RunningDataExportProcesses { get; set; }

        public bool Success { get; set; }
    }
}

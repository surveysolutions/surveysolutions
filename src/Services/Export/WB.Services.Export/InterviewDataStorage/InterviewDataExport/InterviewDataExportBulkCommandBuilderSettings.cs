namespace WB.Services.Export.InterviewDataStorage.InterviewDataExport
{
    public class InterviewDataExportBulkCommandBuilderSettings
    {
        public int MaxParametersCountInOneCommand { get; set; } = 30_000;
    }
}

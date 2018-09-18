namespace WB.Services.Export.Interview
{
    internal class InterviewDataExportSettings
    {
        public int MaxRecordsCountPerOneExportQuery { get; set; } = 100;
        public string DirectoryPath { get; set; }
    }
}

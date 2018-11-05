namespace WB.Services.Export.Interview
{
    public class InterviewDataExportSettings
    {
        public int MaxRecordsCountPerOneExportQuery { get; set; } = 160;
        public string DirectoryPath { get; set; } = ".export";
        public int ParadataQueryLimit { get; set; } = 4;
    }
}

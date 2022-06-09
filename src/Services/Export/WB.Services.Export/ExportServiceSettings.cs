namespace WB.Services.Export
{
    public class ExportServiceSettings
    {
        public int MaxRecordsCountPerOneExportQuery { get; set; } = 160;
        public string DirectoryPath { get; set; } = ".export";
        public string AudioAuditFolderName { get; set; } = "AudioAudit";
        public int DefaultEventQueryPageSize { get; set; } = 10_000;
        public int? MaxEventQueryPageSize { get; set; } = null;
        public int? MaxSaveEventsPageSize { get; set; } = null;
    }
}

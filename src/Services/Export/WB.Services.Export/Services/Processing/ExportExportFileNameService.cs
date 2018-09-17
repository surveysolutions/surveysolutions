using System;
using WB.Services.Export.Infrastructure;

namespace WB.Services.Export.Services.Processing
{
    internal class ExportExportFileNameService : IExportFileNameService
    {
        private readonly IFileSystemAccessor fileSystemAccessor;

        public ExportExportFileNameService(IFileSystemAccessor fileSystemAccessor)
        {
            this.fileSystemAccessor = fileSystemAccessor;
        }

        public string GetFileNameForBatchUploadByQuestionnaire(string questionnaireFilename)
        {
            return $"template_{questionnaireFilename}.zip";
        }

        public string GetFileNameForDdiByQuestionnaire(string questionnaireFilename, string pathToDdiMetadata)
        {
            return this.fileSystemAccessor.Combine(pathToDdiMetadata, $"{questionnaireFilename}_ddi.zip");
        }

        public string GetFileNameForTabByQuestionnaire(string questionnaireFilename, string pathToExportedData,
            DataExportFormat format, string statusSuffix, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var fromDatePrefix = fromDate == null ? "" : $"_{fromDate.Value:yyyyMMddTHHmm}Z";
            var toDatePrefix = toDate == null ? "" : $"_{toDate.Value:yyyyMMddTHHmm}Z";

            var archiveName = $"{questionnaireFilename}_{format}_{statusSuffix}{fromDatePrefix}{toDatePrefix}.zip";
            return this.fileSystemAccessor.Combine(pathToExportedData, archiveName);
        }

        public string GetFileNameForAssignmentTemplate(string questionnaireFilename)
        {
            return $"{questionnaireFilename}.tab";
        }
    }
}
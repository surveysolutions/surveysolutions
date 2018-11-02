using System;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Models;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.Services.Processing
{
    internal class ExportExportFileNameService : IExportFileNameService
    {
        private readonly IFileSystemAccessor fileSystemAccessor;

        public ExportExportFileNameService(IFileSystemAccessor fileSystemAccessor)
        {
            this.fileSystemAccessor = fileSystemAccessor;
        }

        public string GetFileNameForDdiByQuestionnaire(QuestionnaireId questionnaire, string pathToDdiMetadata)
        {
            return this.fileSystemAccessor.Combine(pathToDdiMetadata, $"{questionnaire}_ddi.zip");
        }

        public string GetFileNameForExportArchive(ExportSettings exportSettings, string withQuestionnaireName = null)
        {
            var statusSuffix = exportSettings.Status != null && exportSettings.ExportFormat != DataExportFormat.Binary 
                ? exportSettings.Status.ToString() 
                : "All";

            var fromDatePrefix = exportSettings.FromDate == null || exportSettings.ExportFormat == DataExportFormat.Binary 
                ? "" : $"_{exportSettings.FromDate.Value:yyyyMMddTHHmm}Z";
            var toDatePrefix = exportSettings.ToDate == null || exportSettings.ExportFormat == DataExportFormat.Binary 
                ? "" : $"_{exportSettings.ToDate.Value:yyyyMMddTHHmm}Z";

            var archiveName = $"{withQuestionnaireName ?? exportSettings.QuestionnaireId.ToString()}_" +
                              $"{exportSettings.ExportFormat}_{statusSuffix}{fromDatePrefix}{toDatePrefix}.zip";
            return archiveName;
        }
    }
}

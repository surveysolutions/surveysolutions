using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unidecode.NET;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Models;
using WB.Services.Export.Questionnaire.Services;
using WB.ServicesIntegration.Export;

namespace WB.Services.Export.Services.Processing
{
    public class ExportExportFileNameService : IExportFileNameService
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IQuestionnaireStorage questionnaireStorage;

        public ExportExportFileNameService(
            IFileSystemAccessor fileSystemAccessor,
            IQuestionnaireStorage questionnaireStorage)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.questionnaireStorage = questionnaireStorage;
        }

        public string GetFileNameForDdiByQuestionnaire(QuestionnaireIdentity questionnaire, string pathToDdiMetadata)
        {
            return this.fileSystemAccessor.Combine(pathToDdiMetadata, $"{questionnaire}_ddi.zip");
        }

        public async Task<string> GetQuestionnaireDirectoryName(ExportSettings settings, CancellationToken cancellationToken)
        {
            QuestionnaireIdentity questionnaireId = settings.QuestionnaireId;
            var questionnaire = await questionnaireStorage.GetQuestionnaireAsync(questionnaireId, token: cancellationToken);

            if (questionnaire == null)
                throw new InvalidOperationException("questionnaire must be not null.");

            var variableName = questionnaire.VariableName ?? questionnaire.Id;

            return $"{variableName}${questionnaireId.Version}";
        }


        public async Task<string> GetFileNameForExportArchiveAsync(ExportSettings exportSettings, string? questionnaireNamePrefixOverride = null)
        {
            var statusSuffix = exportSettings.Status == null ? "All" : exportSettings.Status.ToString();

            var fromDatePrefix = exportSettings.FromDate == null || exportSettings.ExportFormat == DataExportFormat.Binary 
                ? "" : $"_{exportSettings.FromDate.Value:yyyyMMddTHHmm}Z";
            var toDatePrefix = exportSettings.ToDate == null || exportSettings.ExportFormat == DataExportFormat.Binary 
                ? "" : $"_{exportSettings.ToDate.Value:yyyyMMddTHHmm}Z";

            string translationName = string.Empty;
            if (exportSettings.Translation.HasValue)
            {
                var questionnaire = await this.questionnaireStorage.GetQuestionnaireAsync(exportSettings.QuestionnaireId,
                        exportSettings.Translation);
                if (questionnaire == null)
                    throw new InvalidOperationException("questionnaire must be not null.");
                var translation = questionnaire.Translations.FirstOrDefault(x => x.Id == exportSettings.Translation);

                if (translation != null)
                {
                    translationName += $"_{this.fileSystemAccessor.MakeValidFileName(translation.Name.Unidecode())}";
                }
            }

            string metaSuffix = exportSettings.IncludeMeta != false ? "" : "_no-meta";
            
            var archiveName = $"{questionnaireNamePrefixOverride ?? exportSettings.QuestionnaireId.ToString()}_" +
                              $"{exportSettings.ExportFormat}_{statusSuffix}{fromDatePrefix}{toDatePrefix}{translationName}{metaSuffix}.zip";

            return archiveName;
        }
    }
}

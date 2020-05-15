using System;
using System.Threading;
using System.Threading.Tasks;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Models;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Questionnaire.Services;

namespace WB.Services.Export.Services.Processing
{
    internal class ExportExportFileNameService : IExportFileNameService
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

        public string GetFileNameForDdiByQuestionnaire(QuestionnaireId questionnaire, string pathToDdiMetadata)
        {
            return this.fileSystemAccessor.Combine(pathToDdiMetadata, $"{questionnaire}_ddi.zip");
        }

        public async Task<string> GetQuestionnaireDirectoryName(ExportSettings settings, CancellationToken cancellationToken)
        {
            var questionnaireId = settings.QuestionnaireId;
            var questionnaire = await questionnaireStorage.GetQuestionnaireAsync(questionnaireId, token: cancellationToken);
            var variableName = questionnaire.VariableName ?? questionnaire.Id;

            if (TryParseQuestionnaireVersion(questionnaireId, out var version))
            {
                return $"{variableName}${version}";
            }

            return $"{variableName}${questionnaireId}";
        }


        private bool TryParseQuestionnaireVersion(QuestionnaireId questionnaireId, out string version)
        {
            var split = questionnaireId.Id.Split('$');
            if (split.Length == 2)
            {
                version = split[1];
                return true;
            }

            version = null;
            return false;
        }

        public string GetFileNameForExportArchive(ExportSettings exportSettings, string withQuestionnaireName = null)
        {
            var statusSuffix = exportSettings.Status == null ? "All" : exportSettings.Status.ToString();

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

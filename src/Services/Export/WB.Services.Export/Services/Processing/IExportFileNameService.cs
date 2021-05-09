using System.Threading;
using System.Threading.Tasks;
using WB.Services.Export.Models;
using WB.ServicesIntegration.Export;

namespace WB.Services.Export.Services.Processing
{
    public interface IExportFileNameService
    {
        string GetFileNameForDdiByQuestionnaire(QuestionnaireIdentity questionnaire, string pathToDdiMetadata);

        Task<string> GetFileNameForExportArchiveAsync(ExportSettings exportSettings, string? questionnaireNamePrefixOverride = null);
        Task<string> GetQuestionnaireDirectoryName(ExportSettings settings, CancellationToken cancellationToken);
    }
}

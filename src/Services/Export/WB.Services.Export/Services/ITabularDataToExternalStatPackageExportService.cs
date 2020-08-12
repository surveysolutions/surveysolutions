using System;
using System.Threading;
using System.Threading.Tasks;
using WB.Services.Export.Questionnaire;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Services
{
    public interface ITabularDataToExternalStatPackageExportService
    {
        Task<string[]> CreateAndGetStataDataFilesForQuestionnaireAsync(TenantInfo tenant,
            QuestionnaireId questionnaireId, Guid? translationId, string[] tabularDataFiles, ExportProgress progress,
            CancellationToken cancellationToken);
        Task<string[]> CreateAndGetSpssDataFilesForQuestionnaireAsync(TenantInfo tenant,
            QuestionnaireId questionnaireId, Guid? translationId, string[] tabularDataFiles, ExportProgress progress,
            CancellationToken cancellationToken);
    }
}

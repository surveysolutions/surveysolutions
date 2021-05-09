using System;
using System.Threading;
using System.Threading.Tasks;
using WB.Services.Infrastructure.Tenant;
using WB.ServicesIntegration.Export;

namespace WB.Services.Export.Services
{
    public interface ITabularDataToExternalStatPackageExportService
    {
        Task<string[]> CreateAndGetStataDataFilesForQuestionnaireAsync(TenantInfo tenant,
            QuestionnaireIdentity questionnaireId, Guid? translationId, string[] tabularDataFiles, ExportProgress progress,
            CancellationToken cancellationToken);
        Task<string[]> CreateAndGetSpssDataFilesForQuestionnaireAsync(TenantInfo tenant,
            QuestionnaireIdentity questionnaireId, Guid? translationId, string[] tabularDataFiles, ExportProgress progress,
            CancellationToken cancellationToken);
    }
}

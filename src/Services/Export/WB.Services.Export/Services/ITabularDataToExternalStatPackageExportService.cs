using System;
using System.Threading;
using System.Threading.Tasks;
using WB.Services.Export.Questionnaire;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Services
{
    internal interface ITabularDataToExternalStatPackageExportService
    {
        Task<string[]> CreateAndGetStataDataFilesForQuestionnaireAsync(TenantInfo tenant, QuestionnaireId questionnaireId, string[] tabularDataFiles, IProgress<int> progress, CancellationToken cancellationToken);
        Task<string[]> CreateAndGetSpssDataFilesForQuestionnaireAsync(TenantInfo tenant, QuestionnaireId questionnaireId, string[] tabularDataFiles, IProgress<int> progress, CancellationToken cancellationToken);
    }
}

using System;
using System.Threading;
using WB.Services.Export.Questionnaire;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Services
{
    internal interface ITabularDataToExternalStatPackageExportService
    {
        string[] CreateAndGetStataDataFilesForQuestionnaire(TenantInfo tenant, QuestionnaireId questionnaireId, string[] tabularDataFiles, IProgress<int> progress, CancellationToken cancellationToken);
        string[] CreateAndGetSpssDataFilesForQuestionnaire(TenantInfo tenant, QuestionnaireId questionnaireId, string[] tabularDataFiles, IProgress<int> progress, CancellationToken cancellationToken);
    }
}

using System;
using WB.ServicesIntegration.Export;
using QuestionnaireIdentity = WB.Core.SharedKernels.DataCollection.Implementation.Entities.QuestionnaireIdentity;

namespace WB.Core.BoundedContexts.Headquarters.Services
{
    public interface IExportFileNameService
    {
        string GetFileNameForBatchUploadByQuestionnaire(QuestionnaireIdentity identity);
        string GetFileNameForDdiByQuestionnaire(QuestionnaireIdentity identity);

        string GetFileNameForTabByQuestionnaire(QuestionnaireIdentity identity, string pathToExportedData,
            DataExportFormat format, string statusSuffix, DateTime? fromDate = null, DateTime? toDate = null);

        string GetFileNameForAssignmentTemplate(QuestionnaireIdentity identity);
        string GetQuestionnaireTitleWithVersion(QuestionnaireIdentity identity);
    }
}

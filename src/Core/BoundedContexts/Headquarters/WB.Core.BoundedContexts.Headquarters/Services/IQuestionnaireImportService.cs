using System;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Services
{
    public interface IQuestionnaireImportService
    {
        QuestionnaireImportResult GetStatus(Guid processId);

        Task<QuestionnaireImportResult> Import(Guid questionnaireId, string name, bool isCensusMode, string comment, string requestUrl, bool includePdf = true);

        Task<QuestionnaireImportResult> ImportAndMigrateAssignments(Guid questionnaireId, string name,
            bool isCensusMode,
            string comment, string requestUrl, bool includePdf, bool shouldMigrateAssignments,
            QuestionnaireIdentity migrateFrom, CriticalityLevel? criticalityLevel);
    }
}

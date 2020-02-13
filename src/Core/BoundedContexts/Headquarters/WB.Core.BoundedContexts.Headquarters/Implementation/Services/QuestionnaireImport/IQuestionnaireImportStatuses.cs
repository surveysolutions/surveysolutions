using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    public interface IQuestionnaireImportStatuses
    {
        QuestionnaireImportResult GetStatus(QuestionnaireIdentity questionnaireId);
        QuestionnaireImportResult GetOrAdd(QuestionnaireIdentity questionnaireId, QuestionnaireImportResult valueToAdd);
    }
}

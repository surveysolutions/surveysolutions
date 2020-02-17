using System;
using WB.Core.BoundedContexts.Headquarters.Services;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    public interface IQuestionnaireImportStatuses
    {
        QuestionnaireImportResult GetStatus(Guid processId);
        QuestionnaireImportResult StartNew(Guid processId, QuestionnaireImportResult valueToAdd);
    }
}

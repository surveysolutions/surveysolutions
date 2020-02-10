using System.Collections.Concurrent;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    class QuestionnaireImportStatuses : IQuestionnaireImportStatuses
    {
        private readonly ConcurrentDictionary<QuestionnaireIdentity, QuestionnaireImportResult> statuses = new ConcurrentDictionary<QuestionnaireIdentity, QuestionnaireImportResult>();

        public QuestionnaireImportResult GetStatus(QuestionnaireIdentity questionnaireId)
        {
            if (statuses.TryGetValue(questionnaireId, out QuestionnaireImportResult status))
                return status;
            return null;
        }

        public QuestionnaireImportResult GetOrAdd(QuestionnaireIdentity questionnaireId, QuestionnaireImportResult valueToAdd)
        {
            var questionnaireImportResult = statuses.GetOrAdd(questionnaireId, valueToAdd);
            return questionnaireImportResult;
        }
    }
}

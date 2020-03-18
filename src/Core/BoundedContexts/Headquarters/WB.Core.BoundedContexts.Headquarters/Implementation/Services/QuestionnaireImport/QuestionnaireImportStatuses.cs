using System;
using System.Collections.Concurrent;
using WB.Core.BoundedContexts.Headquarters.Services;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    class QuestionnaireImportStatuses : IQuestionnaireImportStatuses
    {
        private readonly ConcurrentDictionary<Guid, QuestionnaireImportResult> statuses = new ConcurrentDictionary<Guid, QuestionnaireImportResult>();

        public QuestionnaireImportResult GetStatus(Guid processId)
        {
            if (statuses.TryGetValue(processId, out QuestionnaireImportResult status))
                return status;
            return null;
        }

        public QuestionnaireImportResult StartNew(Guid processId, QuestionnaireImportResult valueToAdd)
        {
            var questionnaireImportResult = statuses.GetOrAdd(processId, valueToAdd);
            return questionnaireImportResult;
        }
    }
}

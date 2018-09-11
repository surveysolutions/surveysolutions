using System;
using Main.Core.Documents;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface IQuestionnaireHistoryVersionsService
    {
        QuestionnaireDocument GetByHistoryVersion(Guid historyReferenceId);
        void RemoveOldQuestionnaireHistory(string sQuestionnaireId, int? maxSequenceByQuestionnaire, int maxHistoryDepth);
        string GetDiffWithPreviousStoredVersion(QuestionnaireDocument questionnaire);
    }
}

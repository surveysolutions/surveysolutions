using System;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface IQuestionnaireHistoryVersionsService
    {
        QuestionnaireDocument GetByHistoryVersion(Guid historyReferenceId);
        void RemoveOldQuestionnaireHistory(string sQuestionnaireId, int? maxSequenceByQuestionnaire, int maxHistoryDepth);
        string GetDiffWithLastStoredVersion(QuestionnaireDocument questionnaire);

        void AddQuestionnaireChangeItem(
            Guid questionnaireId,
            Guid responsibleId,
            string userName,
            QuestionnaireActionType actionType,
            QuestionnaireItemType targetType,
            Guid targetId,
            string targetTitle,
            string targetNewTitle,
            int? affectedEntries,
            DateTime? targetDateTime,
            QuestionnaireDocument questionnaireDocument,
            QuestionnaireChangeReference reference = null);
    }
}

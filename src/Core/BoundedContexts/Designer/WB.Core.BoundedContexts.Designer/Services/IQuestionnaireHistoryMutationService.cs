using System;
using System.Threading.Tasks;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface IQuestionnaireHistoryMutationService
    {
        Task StageQuestionnaireChangeItemAsync(
            Guid questionnaireId,
            Guid responsibleId,
            string? userName,
            QuestionnaireActionType actionType,
            QuestionnaireItemType targetType,
            Guid targetId,
            string? targetTitle,
            string? targetNewTitle,
            int? affectedEntries,
            DateTime? targetDateTime,
            QuestionnaireDocument? questionnaireDocument,
            QuestionnaireChangeReference? reference = null,
            QuestionnaireChangeRecordMetadata? meta = null);
    }
}

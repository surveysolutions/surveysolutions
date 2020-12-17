using System;
using System.Threading.Tasks;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface IQuestionnaireHistoryVersionsService
    {
        QuestionnaireDocument? GetByHistoryVersion(Guid historyReferenceId);
        
        void AddQuestionnaireChangeItem(
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

        Task<bool> UpdateRevisionCommentaryAsync(string questionnaireChangeRecordId, string comment);
        Task<int> TrackQuestionnaireImportAsync(QuestionnaireDocument questionnaireDocument, string? userAgent, Guid userId);
        Task UpdateQuestionnaireMetadataAsync(Guid questionnaire, int revision, QuestionnaireRevisionMetaDataUpdate metaData);
    }
}

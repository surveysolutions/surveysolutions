using System;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory
{
    internal class QuestionnaireChangeHistoryFactory : IQuestionnaireChangeHistoryFactory
    {
        private readonly IPlainStorageAccessor<QuestionnaireChangeRecord> questionnaireChangeHistoryStorage;
        private readonly IPlainKeyValueStorage<QuestionnaireDocument> questionnaireDocumentStorage;

        public QuestionnaireChangeHistoryFactory(
            IPlainStorageAccessor<QuestionnaireChangeRecord> questionnaireChangeHistoryStorage,
            IPlainKeyValueStorage<QuestionnaireDocument> questionnaireDocumentStorage)
        {
            this.questionnaireChangeHistoryStorage = questionnaireChangeHistoryStorage;
            this.questionnaireDocumentStorage = questionnaireDocumentStorage;
        }

        public QuestionnaireChangeHistory Load(Guid id, int page,int pageSize)
        {
            var questionnaire = questionnaireDocumentStorage.GetById(id.FormatGuid());

            if (questionnaire == null)
                return null;

            var questionnaireId = id.FormatGuid();

            var count = questionnaireChangeHistoryStorage.Query(
                    _ =>
                        _.Count(h => h.QuestionnaireId == questionnaireId));

            var questionnaireHistory =
                questionnaireChangeHistoryStorage.Query(
                    _ =>
                        _.Where(h => h.QuestionnaireId == questionnaireId)
                            .OrderByDescending(h => h.Sequence)
                            .Skip((page - 1)*pageSize)
                            .Take(pageSize).ToArray());
            
            return new QuestionnaireChangeHistory(id, questionnaire.Title,
                questionnaireHistory.Select(h => CreateQuestionnaireChangeHistoryWebItem(questionnaire, h))
                    .ToList(), page, count, pageSize);
        }

        private QuestionnaireChangeHistoricalRecord CreateQuestionnaireChangeHistoryWebItem(QuestionnaireDocument questionnaire, QuestionnaireChangeRecord questionnaireChangeRecord)
        {
            var references =
                questionnaireChangeRecord.References.Select(
                    r => CreateQuestionnaireChangeHistoryReference(questionnaire, r)).ToList();

            return new QuestionnaireChangeHistoricalRecord(questionnaireChangeRecord.UserName,
                questionnaireChangeRecord.Timestamp, 
                questionnaireChangeRecord.ActionType,
                questionnaireChangeRecord.TargetItemId, 
                GetItemParentId(questionnaire, questionnaireChangeRecord.TargetItemId),
                questionnaireChangeRecord.TargetItemTitle,
                questionnaireChangeRecord.TargetItemType,
                questionnaireChangeRecord.TargetItemNewTitle,
                questionnaireChangeRecord.AffectedEntriesCount,
                references);
        }

        private QuestionnaireChangeHistoricalRecordReference CreateQuestionnaireChangeHistoryReference(
            QuestionnaireDocument questionnaire, 
            QuestionnaireChangeReference questionnaireChangeReference)
        {
            return new QuestionnaireChangeHistoricalRecordReference(
                questionnaireChangeReference.ReferenceId,
                GetItemParentId(questionnaire, questionnaireChangeReference.ReferenceId),
                questionnaireChangeReference.ReferenceTitle,
                questionnaireChangeReference.ReferenceType,
                IsQuestionnaireChangeHistoryReferenceExists(questionnaire, questionnaireChangeReference.ReferenceId,
                    questionnaireChangeReference.ReferenceType));
        }

        private Guid? GetItemParentId(QuestionnaireDocument questionnaire, Guid itemId)
        {
            var item = questionnaire.FirstOrDefault<IComposite>(g => g.PublicKey == itemId);
            if (item == null)
                return null;

            while (item.GetParent().GetType() != typeof(QuestionnaireDocument))
            {
                item = item.GetParent();
            }
            return item.PublicKey;
        }

        private bool IsQuestionnaireChangeHistoryReferenceExists(QuestionnaireDocument questionnaire, Guid itemId,
            QuestionnaireItemType type)
        {
            switch (type)
            {
                case QuestionnaireItemType.Group:
                case QuestionnaireItemType.Question:
                case QuestionnaireItemType.Roster:
                case QuestionnaireItemType.StaticText:
                case QuestionnaireItemType.Variable:
                    return questionnaire.FirstOrDefault<IComposite>(g => g.PublicKey == itemId)!=null;
                case QuestionnaireItemType.Person:
                    return true;
                case QuestionnaireItemType.Questionnaire:
                    var questionnaireItem = questionnaireDocumentStorage.GetById(itemId.FormatGuid());
                    return questionnaireItem != null && !questionnaireItem.IsDeleted;
                case QuestionnaireItemType.Macro:
                case QuestionnaireItemType.LookupTable:
                    return false;
            }
            return false;
        }
    }
}

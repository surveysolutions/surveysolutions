using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory
{
    internal class QuestionnaireChangeHistoryFactory : IQuestionnaireChangeHistoryFactory
    {
        private readonly DesignerDbContext dbContext;
        private readonly IPlainKeyValueStorage<QuestionnaireDocument> questionnaireDocumentStorage;

        public QuestionnaireChangeHistoryFactory(
            DesignerDbContext dbContext,
            IPlainKeyValueStorage<QuestionnaireDocument> questionnaireDocumentStorage)
        {
            this.dbContext = dbContext;
            this.questionnaireDocumentStorage = questionnaireDocumentStorage;
        }

        public QuestionnaireChangeHistory Load(Guid id, int page,int pageSize)
        {
            var questionnaire = questionnaireDocumentStorage.GetById(id.FormatGuid());

            if (questionnaire == null)
                return null;

            var questionnaireId = id.FormatGuid();

            var count = this.dbContext.QuestionnaireChangeRecords.Count(h => h.QuestionnaireId == questionnaireId);

            var questionnaireHistory =
                this.dbContext.QuestionnaireChangeRecords.Where(h => h.QuestionnaireId == questionnaireId)
                    .OrderByDescending(h => h.Sequence)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToArray();

            var historyItemIds = questionnaireHistory.Select(x => x.QuestionnaireChangeRecordId).ToArray();

            var hasHistory = this.dbContext.QuestionnaireChangeRecords
                                            .Where(x => historyItemIds.Contains(x.QuestionnaireChangeRecordId) 
                                                          && (x.Patch != null || x.ResultingQuestionnaireDocument != null))
                                            .Select(x => x.QuestionnaireChangeRecordId)
                                            .ToList().ToHashSet();
            
            return new QuestionnaireChangeHistory(id, questionnaire.Title,
                questionnaireHistory.Select(h => CreateQuestionnaireChangeHistoryWebItem(questionnaire, h, hasHistory))
                    .ToList(), page, count, pageSize);
        }

        private QuestionnaireChangeHistoricalRecord CreateQuestionnaireChangeHistoryWebItem(QuestionnaireDocument questionnaire,
            QuestionnaireChangeRecord questionnaireChangeRecord,
            HashSet<string> recordWithRevertAvailable)
        {
            var references =
                questionnaireChangeRecord.References.Select(
                    r => CreateQuestionnaireChangeHistoryReference(questionnaire, r)).ToList();

            return new QuestionnaireChangeHistoricalRecord(
                questionnaireChangeRecord.QuestionnaireChangeRecordId,
                questionnaireChangeRecord.UserName,
                questionnaireChangeRecord.Timestamp,
                questionnaireChangeRecord.ActionType,
                questionnaireChangeRecord.TargetItemId,
                GetItemParentId(questionnaire, questionnaireChangeRecord.TargetItemId),
                questionnaireChangeRecord.TargetItemTitle,
                questionnaireChangeRecord.TargetItemType,
                questionnaireChangeRecord.TargetItemNewTitle,
                questionnaireChangeRecord.AffectedEntriesCount,
                recordWithRevertAvailable.Contains(questionnaireChangeRecord.QuestionnaireChangeRecordId),
                questionnaireChangeRecord.TargetItemDateTime,
                references)
            {
                Sequence = questionnaireChangeRecord.Sequence
            };
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
                case QuestionnaireItemType.Section:
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

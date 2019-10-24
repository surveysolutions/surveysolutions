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

            var query = this.dbContext.QuestionnaireChangeRecords.Where(h => h.QuestionnaireId == questionnaireId);

            

            var questionnaireHistory = query                
                    .OrderByDescending(h => h.Sequence)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToArray();
            
            return new QuestionnaireChangeHistory(id, questionnaire.Title,
                questionnaireHistory.Select(h => CreateQuestionnaireChangeHistoryWebItem(questionnaire, h))
                    .ToList(), page, count, pageSize);
        }
   
        private QuestionnaireChangeHistoricalRecord CreateQuestionnaireChangeHistoryWebItem(
            QuestionnaireDocument questionnaire,  QuestionnaireChangeRecord revision)
        {
            var references =
                revision.References.Select(
                    r => CreateQuestionnaireChangeHistoryReference(questionnaire, r)).ToList();

            return new QuestionnaireChangeHistoricalRecord(
                revision.QuestionnaireChangeRecordId,
                revision.UserName,
                revision.Timestamp,
                revision.ActionType,
                revision.TargetItemId,
                GetItemParentId(questionnaire, revision.TargetItemId),
                revision.TargetItemTitle,
                revision.TargetItemType,
                revision.TargetItemNewTitle,
                revision.AffectedEntriesCount,
                revision.Patch != null || revision.ResultingQuestionnaireDocument != null,
                revision.TargetItemDateTime,
                references,
                revision?.Meta?.Comment,
                revision?.Meta?.Hq?.Version,
                revision?.Meta?.Hq?.QuestionnaireVersion)
            {
                Sequence = revision.Sequence
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

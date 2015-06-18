using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory
{
    internal class QuestionnaireChangeHistoryFactory : IQuestionnaireChangeHistoryFactory
    {
        private readonly IQueryableReadSideRepositoryReader<QuestionnaireChangeRecord> questionnaireChangeHistoryStorage;
        private readonly IReadSideKeyValueStorage<QuestionnaireDocument> questionnaireDocumentStorage;

        public QuestionnaireChangeHistoryFactory(
            IQueryableReadSideRepositoryReader<QuestionnaireChangeRecord> questionnaireChangeHistoryStorage, 
            IReadSideKeyValueStorage<QuestionnaireDocument> questionnaireDocumentStorage)
        {
            this.questionnaireChangeHistoryStorage = questionnaireChangeHistoryStorage;
            this.questionnaireDocumentStorage = questionnaireDocumentStorage;
        }

        public QuestionnaireChangeHistory Load(Guid id, int page,int pageSize)
        {
            var questionnaire = questionnaireDocumentStorage.GetById(id);

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
                            .OrderBy(h => h.Sequence)
                            .Skip((page - 1)*pageSize)
                            .Take(pageSize).ToArray());

            questionnaire.ConnectChildrenWithParent();

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
                questionnaireChangeRecord.Timestamp, questionnaireChangeRecord.ActionType,
                questionnaireChangeRecord.TargetItemId, GetItemParentId(questionnaire, questionnaireChangeRecord.TargetItemId), questionnaireChangeRecord.TargetItemTitle,
                questionnaireChangeRecord.TargetItemType, references);
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
                    return questionnaire.FirstOrDefault<IComposite>(g => g.PublicKey == itemId)!=null;
                case QuestionnaireItemType.Person:
                    return true;
                case QuestionnaireItemType.Questionnaire:
                    var questionnaireItem = questionnaireDocumentStorage.GetById(itemId);
                    return questionnaireItem != null && !questionnaireItem.IsDeleted;
            }
            return false;
        }
    }
}

using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.GenericSubdomains.Portable;
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
            if (questionnaire == null || !questionnaireHistory.Any())
                return null;

            questionnaire.ConnectChildrenWithParent();

            return new QuestionnaireChangeHistory(id, questionnaire.Title,
                questionnaireHistory.Select(h => CreateQuestionnaireChangeHistoryWebItem(questionnaire, h))
                    .ToList(), questionnaire, page, count, pageSize);
        }

        private QuestionnaireChangeHistoricalRecord CreateQuestionnaireChangeHistoryWebItem(QuestionnaireDocument questionnaire, QuestionnaireChangeRecord questionnaireChangeRecord)
        {
            var references = questionnaireChangeRecord.References.Select(CreateQuestionnaireChangeHistoryReference).ToList();

            return new QuestionnaireChangeHistoricalRecord(questionnaireChangeRecord.UserName,
                questionnaireChangeRecord.Timestamp, questionnaireChangeRecord.ActionType,
                questionnaireChangeRecord.TargetItemId, questionnaireChangeRecord.TargetItemTitle,
                questionnaireChangeRecord.TargetItemType, references);
        }
        private QuestionnaireChangeHistoricalRecordReference CreateQuestionnaireChangeHistoryReference(
            QuestionnaireChangeReference questionnaireChangeReference)
        {
            return new QuestionnaireChangeHistoricalRecordReference(questionnaireChangeReference.ReferenceId,
                questionnaireChangeReference.ReferenceTitle, questionnaireChangeReference.ReferenceType);
        }
    }
}

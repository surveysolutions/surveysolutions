using System;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    public class QuestionnireHistotyVersionsService : IQuestionnireHistotyVersionsService
    {
        private readonly IPlainStorageAccessor<QuestionnaireChangeRecord> questionnaireChangeItemStorage;
        private readonly IEntitySerializer<QuestionnaireDocument> entitySerializer;

        public QuestionnireHistotyVersionsService(IPlainStorageAccessor<QuestionnaireChangeRecord> questionnaireChangeItemStorage,
            IEntitySerializer<QuestionnaireDocument> entitySerializer )
        {
            this.questionnaireChangeItemStorage = questionnaireChangeItemStorage;
            this.entitySerializer = entitySerializer;
        }

        public QuestionnaireDocument GetByHistoryVersion(Guid historyReferanceId)
        {
            var questionnaireChangeRecord = this.questionnaireChangeItemStorage.GetById(historyReferanceId.FormatGuid());
            if (questionnaireChangeRecord == null)
                return null;

            var resultingQuestionnaireDocument = questionnaireChangeRecord.ResultingQuestionnaireDocument;
            var questionnaireDocument = this.entitySerializer.Deserialize(resultingQuestionnaireDocument);
            return questionnaireDocument;
        }

        public string GetResultingQuestionnaireDocument(QuestionnaireDocument questionnaireDocument)
        {
            if (questionnaireDocument == null)
                return null;

            return entitySerializer.Serialize(questionnaireDocument);
        }
    }
}
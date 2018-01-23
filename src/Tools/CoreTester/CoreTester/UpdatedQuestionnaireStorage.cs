using System;
using Main.Core.Documents;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace CoreTester
{
    public class UpdatedQuestionnaireStorage : IQuestionnaireStorage
    {
        private IQuestionnaire questionnaire = null;
        private QuestionnaireDocument questionnaireDocument = null;

        public IQuestionnaire GetQuestionnaire(QuestionnaireIdentity identity, string language)
        {
            return questionnaire;
        }

        public void StoreQuestionnaire(Guid id, long version, QuestionnaireDocument questionnaireDocument)
        {
            this.questionnaireDocument = questionnaireDocument;
            this.questionnaire = new PlainQuestionnaire(questionnaireDocument, version, null);
        }

        public QuestionnaireDocument GetQuestionnaireDocument(QuestionnaireIdentity identity)
        {
            return this.questionnaireDocument;
        }

        public QuestionnaireDocument GetQuestionnaireDocument(Guid id, long version)
        {
            return this.questionnaireDocument;
        }

        public void DeleteQuestionnaireDocument(Guid id, long version)
        {
            this.questionnaire = null;
            this.questionnaireDocument = null;
        }
    }
}
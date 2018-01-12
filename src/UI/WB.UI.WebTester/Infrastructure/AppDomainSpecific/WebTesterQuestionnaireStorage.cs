using System;
using Main.Core.Documents;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.UI.WebTester.Infrastructure
{
    public class WebTesterQuestionnaireStorage : IQuestionnaireStorage
    {
        public PlainQuestionnaire Questionnaire { get; set; }

        public IQuestionnaire GetQuestionnaire(QuestionnaireIdentity identity, string language)
        {
            return Questionnaire;
        }

        public void StoreQuestionnaire(Guid id, long version, QuestionnaireDocument questionnaireDocument)
        {
            Questionnaire = new PlainQuestionnaire(questionnaireDocument, version);
        }

        public QuestionnaireDocument GetQuestionnaireDocument(QuestionnaireIdentity identity)
        {
            return Questionnaire.QuestionnaireDocument;
        }

        public QuestionnaireDocument GetQuestionnaireDocument(Guid id, long version)
        {
            return Questionnaire.QuestionnaireDocument;
        }

        public void DeleteQuestionnaireDocument(Guid id, long version)
        {
            Questionnaire = null;
        }
    }
}
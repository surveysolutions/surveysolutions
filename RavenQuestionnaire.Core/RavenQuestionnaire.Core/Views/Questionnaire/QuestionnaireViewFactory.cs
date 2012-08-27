using System;
using RavenQuestionnaire.Core.Denormalizers;
using RavenQuestionnaire.Core.Documents;

namespace RavenQuestionnaire.Core.Views.Questionnaire
{
    public class QuestionnaireViewFactory : IViewFactory<QuestionnaireViewInputModel, QuestionnaireView>
    {
        private IDenormalizerStorage<QuestionnaireDocument> documentSession;

        public QuestionnaireViewFactory(IDenormalizerStorage<QuestionnaireDocument> documentSession)
        {
            this.documentSession = documentSession;
        }
        public QuestionnaireView Load(QuestionnaireViewInputModel input)
        {
            var doc = documentSession.GetByGuid(input.QuestionnaireId);
            
            return new QuestionnaireView(doc);
        }
    }
}

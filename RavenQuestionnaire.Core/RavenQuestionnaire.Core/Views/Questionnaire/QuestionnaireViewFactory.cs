using System;
using Raven.Client;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.ViewSnapshot;

namespace RavenQuestionnaire.Core.Views.Questionnaire
{
    public class QuestionnaireViewFactory : IViewFactory<QuestionnaireViewInputModel, QuestionnaireView>
    {
        private IViewSnapshot documentSession;

        public QuestionnaireViewFactory(IViewSnapshot documentSession)
        {
            this.documentSession = documentSession;
        }
        public QuestionnaireView Load(QuestionnaireViewInputModel input)
        {
            var doc = documentSession.ReadByGuid<QuestionnaireDocument>(Guid.Parse(input.QuestionnaireId));

            /*  var questions =
                  documentSession.Query<QuestionDocument, QuestionnaireContainingQuestions>().Where(
                      question => question.QuestionnaireId.Equals(doc.Id));*/

            return new QuestionnaireView(doc);
        }
    }
}

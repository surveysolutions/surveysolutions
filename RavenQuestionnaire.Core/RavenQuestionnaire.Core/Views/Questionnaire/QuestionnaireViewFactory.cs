using Raven.Client;
using RavenQuestionnaire.Core.Documents;

namespace RavenQuestionnaire.Core.Views.Questionnaire
{
    public class QuestionnaireViewFactory : IViewFactory<QuestionnaireViewInputModel, QuestionnaireView>
    {
        private IDocumentSession documentSession;

        public QuestionnaireViewFactory(IDocumentSession documentSession)
        {
            this.documentSession = documentSession;
        }
        public QuestionnaireView Load(QuestionnaireViewInputModel input)
        {
            var doc = documentSession.Load<QuestionnaireDocument>(input.QuestionnaireId);

            /*  var questions =
                  documentSession.Query<QuestionDocument, QuestionnaireContainingQuestions>().Where(
                      question => question.QuestionnaireId.Equals(doc.Id));*/

            return new QuestionnaireView(doc);
        }
    }
}

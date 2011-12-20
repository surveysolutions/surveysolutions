using System.Linq;
using Raven.Client;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Views.Question;

namespace RavenQuestionnaire.Core.Views.Questionnaire
{
    public class QuestionnaireViewFactory: IViewFactory<QuestionnaireViewInputModel, QuestionnaireView>
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
            
            return new QuestionnaireView(doc.Id,
                                         doc.Title, doc.CreationDate, doc.LastEntryDate,
                                         doc.Questions.Select(q => new QuestionView(q.PublicKey, q.QuestionText, q.QuestionType, q.Answers, doc.Id, q.ConditionExpression)));
        }
    }
}

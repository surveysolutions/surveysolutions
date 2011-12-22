using Raven.Client;
using RavenQuestionnaire.Core.Documents;
using System.Linq;

namespace RavenQuestionnaire.Core.Views.Question
{
    public class QuestionViewFactory : IViewFactory<QuestionViewInputModel, QuestionView>
    {
         private IDocumentSession documentSession;

         public QuestionViewFactory(IDocumentSession documentSession)
        {
            this.documentSession = documentSession;
        }
        public QuestionView Load(QuestionViewInputModel input)
        {
            var doc = documentSession.Load<QuestionnaireDocument>(input.QuestionnaireId);
            var question =doc.Questions.Where(q => q.PublicKey.Equals(input.PublickKey)).FirstOrDefault();
            if (question == null)
                return null;
            return new QuestionView(question.PublicKey, question.QuestionText, question.QuestionType, question.Answers, doc.Id, question.ConditionExpression);
        
        }
    }
}

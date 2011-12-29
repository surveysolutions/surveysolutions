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

             var question =
                 new RavenQuestionnaire.Core.Entities.Questionnaire(doc).Find
                     <RavenQuestionnaire.Core.Entities.SubEntities.Question>(input.PublicKey);
             if (question == null)
                 return null;
             return new QuestionView(doc, question);

         }
    }
}

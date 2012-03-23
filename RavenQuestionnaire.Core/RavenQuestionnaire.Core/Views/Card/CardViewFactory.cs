using System.Linq;
using Raven.Client;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Views.Question;

namespace RavenQuestionnaire.Core.Views.Card
{
    public class CardViewFactory : IViewFactory<CardViewInputModel, CardView>
    {
         private IDocumentSession documentSession;

         public CardViewFactory(IDocumentSession documentSession)
        {
            this.documentSession = documentSession;
        }
         public CardView Load(CardViewInputModel input)
         {
             var doc = documentSession.Load<QuestionnaireDocument>(input.QuestionnaireId);

             var question = new Entities.Questionnaire(doc).Find<Entities.SubEntities.Question>(input.QuestionKey);
             if (question == null)
                 return null;
             return new CardView(input.QuestionKey, question.Cards.Single(c => c.PublicKey == input.ImageKey));
         }
    }
}

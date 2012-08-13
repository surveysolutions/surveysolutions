using System;
using System.Linq;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.ViewSnapshot;
using RavenQuestionnaire.Core.Views.Question;

namespace RavenQuestionnaire.Core.Views.Card
{
    public class CardViewFactory : IViewFactory<CardViewInputModel, CardView>
    {
        private IViewSnapshot documentSession;

        public CardViewFactory(IViewSnapshot documentSession)
        {
            this.documentSession = documentSession;
        }

        public CardView Load(CardViewInputModel input)
        {
            var doc = documentSession.ReadByGuid<QuestionnaireDocument>(Guid.Parse(input.QuestionnaireId));

            var question = doc.Find<IQuestion>(input.QuestionKey);
            if (question == null)
                return null;
            return new CardView(input.QuestionKey, question.Cards.Single(c => c.PublicKey == input.ImageKey));
        }
    }
}

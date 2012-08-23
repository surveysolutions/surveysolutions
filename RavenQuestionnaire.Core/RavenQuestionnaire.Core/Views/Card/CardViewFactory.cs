using System;
using System.Linq;
using RavenQuestionnaire.Core.Denormalizers;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.Question;

namespace RavenQuestionnaire.Core.Views.Card
{
    public class CardViewFactory : IViewFactory<CardViewInputModel, CardView>
    {
        private readonly IDenormalizerStorage<QuestionnaireDocument> _documentSession;

        public CardViewFactory(IDenormalizerStorage<QuestionnaireDocument> documentSession)
        {
            this._documentSession = documentSession;
        }

        public CardView Load(CardViewInputModel input)
        {
            var doc = _documentSession.GetByGuid(Guid.Parse(input.QuestionnaireId));

            var question = doc.Find<IQuestion>(input.QuestionKey);
            if (question == null)
                return null;
            return new CardView(input.QuestionKey, question.Cards.Single(c => c.PublicKey == input.ImageKey));
        }
    }
}

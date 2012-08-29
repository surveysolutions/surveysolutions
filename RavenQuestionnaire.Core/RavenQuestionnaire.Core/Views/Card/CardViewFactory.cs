// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CardViewFactory.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The card view factory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Views.Card
{
    using System;
    using System.Linq;

    using RavenQuestionnaire.Core.Denormalizers;
    using RavenQuestionnaire.Core.Documents;
    using RavenQuestionnaire.Core.Entities.SubEntities;
    using RavenQuestionnaire.Core.Views.Question;

    /// <summary>
    /// The card view factory.
    /// </summary>
    public class CardViewFactory : IViewFactory<CardViewInputModel, CardView>
    {
        #region Fields

        /// <summary>
        /// The _document session.
        /// </summary>
        private readonly IDenormalizerStorage<QuestionnaireDocument> _documentSession;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CardViewFactory"/> class.
        /// </summary>
        /// <param name="documentSession">
        /// The document session.
        /// </param>
        public CardViewFactory(IDenormalizerStorage<QuestionnaireDocument> documentSession)
        {
            this._documentSession = documentSession;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The load.
        /// </summary>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <returns>
        /// The RavenQuestionnaire.Core.Views.Card.CardView.
        /// </returns>
        public CardView Load(CardViewInputModel input)
        {
            QuestionnaireDocument doc = this._documentSession.GetByGuid(Guid.Parse(input.QuestionnaireId));

            var question = doc.Find<IQuestion>(input.QuestionKey);
            if (question == null)
            {
                return null;
            }

            return new CardView(input.QuestionKey, question.Cards.Single(c => c.PublicKey == input.ImageKey));
        }

        #endregion
    }
}
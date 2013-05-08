// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CardViewFactory.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The card view factory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.DenormalizerStorage;

namespace Main.Core.View.Card
{
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
            QuestionnaireDocument doc = this._documentSession.GetById(Guid.Parse(input.QuestionnaireId));

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
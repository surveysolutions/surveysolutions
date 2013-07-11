using System;

namespace Main.Core.View.Card
{
    /// <summary>
    /// The card view input model.
    /// </summary>
    public class CardViewInputModel
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CardViewInputModel"/> class.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="questionnaireId">
        /// The questionnaire id.
        /// </param>
        /// <param name="imageKey">
        /// The image key.
        /// </param>
        public CardViewInputModel(Guid publicKey, string questionnaireId, Guid imageKey)
        {
            this.QuestionKey = publicKey;
            this.ImageKey = imageKey;
            this.QuestionnaireId = questionnaireId;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the image key.
        /// </summary>
        public Guid ImageKey { get; private set; }

        /// <summary>
        /// Gets the question key.
        /// </summary>
        public Guid QuestionKey { get; private set; }

        /// <summary>
        /// Gets or sets the questionnaire id.
        /// </summary>
        public string QuestionnaireId { get; set; }

        #endregion
    }
}
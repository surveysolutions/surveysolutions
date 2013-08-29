using System;

namespace Main.Core.View.Question
{
    /// <summary>
    /// The question view input model.
    /// </summary>
    public class QuestionViewInputModel
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionViewInputModel"/> class.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="questionnaireId">
        /// The questionnaire id.
        /// </param>
        public QuestionViewInputModel(Guid publicKey, Guid questionnaireId)
        {
            this.PublicKey = publicKey;
            this.QuestionnaireId = questionnaireId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionViewInputModel"/> class.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="questionnaireId">
        /// The questionnaire id.
        /// </param>
        /// <param name="groupPublicKey">
        /// The group public key.
        /// </param>
        public QuestionViewInputModel(Guid publicKey, Guid questionnaireId, Guid? groupPublicKey)
        {
            this.PublicKey = publicKey;
            this.QuestionnaireId = questionnaireId;
            this.GroupPublicKey = groupPublicKey;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the public key.
        /// </summary>
        public Guid PublicKey { get; private set; }

        /// <summary>
        /// Gets or sets the questionnaire id.
        /// </summary>
        public Guid QuestionnaireId { get; set; }

        /// <summary>
        /// Gets or sets GroupPublicKey.
        /// </summary>
        public Guid? GroupPublicKey { get; set; }

        #endregion
    }
}
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteGroupViewInputModel.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The complete group view input model.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Views.Group
{
    using System;

    /// <summary>
    /// The complete group view input model.
    /// </summary>
    public class CompleteGroupViewInputModel
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteGroupViewInputModel"/> class.
        /// </summary>
        /// <param name="propagationkey">
        /// The propagationkey.
        /// </param>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="questionnaireId">
        /// The questionnaire id.
        /// </param>
        public CompleteGroupViewInputModel(Guid? propagationkey, Guid? publicKey, string questionnaireId)
        {
            this.PropagationKey = propagationkey;
            if (publicKey.HasValue && publicKey.Value != Guid.Empty)
            {
                this.PublicKey = publicKey;
            }

            this.QuestionnaireId = questionnaireId;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the propagation key.
        /// </summary>
        public Guid? PropagationKey { get; private set; }

        /// <summary>
        /// Gets the public key.
        /// </summary>
        public Guid? PublicKey { get; private set; }

        /// <summary>
        /// Gets or sets the questionnaire id.
        /// </summary>
        public string QuestionnaireId { get; set; }

        #endregion
    }
}
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GroupViewInputModel.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The group view input model.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Core.Views.Group
{
    using System;

    /// <summary>
    /// The group view input model.
    /// </summary>
    public class GroupViewInputModel
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupViewInputModel"/> class.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="questionnaireId">
        /// The questionnaire id.
        /// </param>
        public GroupViewInputModel(Guid publicKey, Guid questionnaireId)
        {
            this.PublicKey = publicKey;
            this.QuestionnaireId = questionnaireId;
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

        #endregion
    }
}
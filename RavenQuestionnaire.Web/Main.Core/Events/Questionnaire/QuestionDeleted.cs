// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestionDeleted.cs" company="">
//   
// </copyright>
// <summary>
//   The question deleted.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.Events.Questionnaire
{
    using System;

    using Ncqrs.Eventing.Storage;

    /// <summary>
    /// The question deleted.
    /// </summary>
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:QuestionDeleted")]
    public class QuestionDeleted
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionDeleted"/> class.
        /// </summary>
        /// <param name="questionId">
        /// The question id.
        /// </param>
        /// <param name="parentPublicKey">
        /// The parent public key.
        /// </param>
        public QuestionDeleted(Guid questionId, Guid parentPublicKey)
        {
            this.QuestionId = questionId;
            this.ParentPublicKey = parentPublicKey;
        }


        #region Public Properties

        /// <summary>
        /// Gets or sets the question id.
        /// </summary>
        public Guid QuestionId { get; set; }

        /// <summary>
        /// Gets or sets the group public key.
        /// </summary>
        public Guid ParentPublicKey { get; set; }
        
        #endregion
    }
}
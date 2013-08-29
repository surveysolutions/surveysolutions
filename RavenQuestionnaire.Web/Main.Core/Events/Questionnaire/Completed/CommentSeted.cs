namespace Main.Core.Events.Questionnaire.Completed
{
    using System;

    using Main.Core.Entities.SubEntities;

    using Ncqrs.Eventing.Storage;

    /// <summary>
    /// The comment set.
    /// </summary>
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:CommentSet")]
    public class CommentSet
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the comments.
        /// </summary>
        public string Comments { get; set; }
        
        /// <summary>
        /// Gets or sets the propagation public key.
        /// </summary>
        public Guid? PropagationPublicKey { get; set; }

        /// <summary>
        /// Gets or sets the question Public key.
        /// </summary>
        public Guid QuestionPublickey { get; set; }

        /// <summary>
        /// Gets or sets User.
        /// </summary>
        public UserLight User { get; set; }

        #endregion
    }
}
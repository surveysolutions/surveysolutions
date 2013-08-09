namespace Main.Core.Events.Questionnaire.Completed
{
    using System;

    using Ncqrs.Eventing.Storage;

    /// <summary>
    /// The comment set.
    /// </summary>
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:FlagSet")]
    public class FlagSet
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets a value indicating whether IsFlaged.
        /// </summary>
        public bool IsFlaged { get; set; }
        
        /// <summary>
        /// Gets or sets the propagation public key.
        /// </summary>
        public Guid? PropagationPublicKey { get; set; }

        /// <summary>
        /// Gets or sets the question Public key.
        /// </summary>
        public Guid QuestionPublickey { get; set; }

        #endregion
    }
}
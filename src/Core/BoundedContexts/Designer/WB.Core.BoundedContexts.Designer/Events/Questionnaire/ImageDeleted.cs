namespace Main.Core.Events.Questionnaire
{
    using System;

    using Ncqrs.Eventing.Storage;

    /// <summary>
    /// The image deleted.
    /// </summary>
    [EventName("RavenQuestionnaire.Core:Events:ImageDeleted")]
    public class ImageDeleted
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the image key.
        /// </summary>
        public Guid ImageKey { get; set; }

        /// <summary>
        /// Gets or sets the question key.
        /// </summary>
        public Guid QuestionKey { get; set; }

        #endregion
    }
}
namespace Main.Core.Events.Location
{
    using System;

    using Ncqrs.Eventing.Storage;

    /// <summary>
    /// The new location created.
    /// </summary>
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:NewLocationCreated")]
    public class NewLocationCreated
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the creation date.
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Gets or sets the location id.
        /// </summary>
        public Guid LocationId { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        #endregion
    }
}
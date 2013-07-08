namespace Main.Core.Events.Collection
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Entities.SubEntities;

    using Ncqrs.Eventing.Storage;

    /// <summary>
    /// The new collection created.
    /// </summary>
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:NewCollectionCreated")]
    public class NewCollectionCreated
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the collection id.
        /// </summary>
        public Guid CollectionId { get; set; }

        /// <summary>
        /// Gets or sets the creation date.
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Gets or sets the items.
        /// </summary>
        public List<CollectionItem> Items { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        #endregion
    }
}
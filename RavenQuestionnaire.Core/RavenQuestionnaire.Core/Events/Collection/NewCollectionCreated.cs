// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NewCollectionCreated.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   Defines the NewCollectionCreated type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Core.Events.Collection
{
    using System;
    using System.Collections.Generic;

    using Ncqrs.Eventing.Storage;

    using RavenQuestionnaire.Core.Entities.SubEntities;

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
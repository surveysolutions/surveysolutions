// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CreateCollectionCommand.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The create collection command.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Commands.Collection
{
    using System;
    using System.Collections.Generic;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    using RavenQuestionnaire.Core.Domain;
    using RavenQuestionnaire.Core.Entities.SubEntities;

    /// <summary>
    /// The create collection command.
    /// </summary>
    [Serializable]
    [MapsToAggregateRootConstructor(typeof(CollectionAR))]
    public class CreateCollectionCommand : CommandBase
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateCollectionCommand"/> class.
        /// </summary>
        /// <param name="collectionId">
        /// The collection id.
        /// </param>
        /// <param name="title">
        /// The title.
        /// </param>
        /// <param name="items">
        /// The items.
        /// </param>
        public CreateCollectionCommand(Guid collectionId, string title, List<CollectionItem> items)
        {
            this.CollectionId = collectionId;
            this.Text = title;
            this.Items = items;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the collection id.
        /// </summary>
        public Guid CollectionId { get; set; }

        /// <summary>
        /// Gets or sets the items.
        /// </summary>
        public List<CollectionItem> Items { get; set; }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        public string Text { get; set; }

        #endregion
    }
}
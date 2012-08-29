// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CollectionItemBrowseView.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The collection item browse view.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Views.CollectionItem
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The collection item browse view.
    /// </summary>
    public class CollectionItemBrowseView
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionItemBrowseView"/> class.
        /// </summary>
        /// <param name="collectionId">
        /// The collection id.
        /// </param>
        /// <param name="items">
        /// The items.
        /// </param>
        /// <param name="questionId">
        /// The question id.
        /// </param>
        public CollectionItemBrowseView(string collectionId, List<CollectionItemBrowseItem> items, Guid questionId)
        {
            this.CollectionId = collectionId;
            this.Items = items;
            this.QuestionId = questionId;
            this.PublicKey = Guid.NewGuid();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the collection id.
        /// </summary>
        public string CollectionId { get; set; }

        /// <summary>
        /// Gets or sets the items.
        /// </summary>
        public List<CollectionItemBrowseItem> Items { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PublicKey { get; set; }

        /// <summary>
        /// Gets or sets the question id.
        /// </summary>
        public Guid QuestionId { get; set; }

        #endregion
    }
}
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CollectionItemView.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The collection item view.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Views.CollectionItem
{
    using System;

    using RavenQuestionnaire.Core.Entities.SubEntities;

    /// <summary>
    /// The collection item view.
    /// </summary>
    public class CollectionItemView
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionItemView"/> class.
        /// </summary>
        public CollectionItemView()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionItemView"/> class.
        /// </summary>
        /// <param name="collectionId">
        /// The collection id.
        /// </param>
        public CollectionItemView(string collectionId)
            : this()
        {
            this.CollectionId = collectionId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionItemView"/> class.
        /// </summary>
        /// <param name="collectionId">
        /// The collection id.
        /// </param>
        /// <param name="doc">
        /// The doc.
        /// </param>
        public CollectionItemView(string collectionId, ICollectionItem doc)
            : this(collectionId)
        {
            this.PublicKey = doc.PublicKey;
            this.Key = doc.Key;
            this.Value = doc.Value;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the collection id.
        /// </summary>
        public string CollectionId { get; set; }

        /// <summary>
        /// Gets or sets the index.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PublicKey { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public string Value { get; set; }

        #endregion
    }
}
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Collection.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The Collection interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Entities
{
    using System;
    using System.Collections.Generic;

    using RavenQuestionnaire.Core.Documents;
    using RavenQuestionnaire.Core.Entities.SubEntities;

    /// <summary>
    /// The Collection interface.
    /// </summary>
    public interface ICollection
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        string Name { get; set; }

        #endregion
    }

    /// <summary>
    /// The collection.
    /// </summary>
    public class Collection : IEntity<CollectionDocument>
    {
        #region Fields

        /// <summary>
        /// The inner document.
        /// </summary>
        private readonly CollectionDocument innerDocument;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Collection"/> class.
        /// </summary>
        /// <param name="innerDocument">
        /// The inner document.
        /// </param>
        public Collection(CollectionDocument innerDocument)
        {
            this.innerDocument = innerDocument;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Collection"/> class.
        /// </summary>
        /// <param name="title">
        /// The title.
        /// </param>
        /// <param name="items">
        /// The items.
        /// </param>
        public Collection(string title, List<CollectionItem> items)
        {
            this.innerDocument = new CollectionDocument { Name = title, Items = items };
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the collection id.
        /// </summary>
        public string CollectionId
        {
            get
            {
                return this.innerDocument.Id;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The add collection items.
        /// </summary>
        /// <param name="item">
        /// The item.
        /// </param>
        public void AddCollectionItems(List<CollectionItem> item)
        {
            foreach (CollectionItem collectionItem in item)
            {
                this.innerDocument.Items.Add(collectionItem);
            }
        }

        /// <summary>
        /// The clear items.
        /// </summary>
        public void ClearItems()
        {
            this.innerDocument.Items.Clear();
        }

        /// <summary>
        /// The delete item from collection.
        /// </summary>
        /// <param name="itemId">
        /// The item id.
        /// </param>
        public void DeleteItemFromCollection(Guid itemId)
        {
            CollectionItem item = this.innerDocument.Items.Find(t => t.PublicKey == itemId);
            this.innerDocument.Items.Remove(item);
        }

        /// <summary>
        /// The update text.
        /// </summary>
        /// <param name="text">
        /// The text.
        /// </param>
        public void UpdateText(string text)
        {
            this.innerDocument.Name = text;
        }

        #endregion

        #region Explicit Interface Methods

        /// <summary>
        /// The get inner document.
        /// </summary>
        /// <returns>
        /// The RavenQuestionnaire.Core.Documents.CollectionDocument.
        /// </returns>
        CollectionDocument IEntity<CollectionDocument>.GetInnerDocument()
        {
            return this.innerDocument;
        }

        #endregion
    }
}
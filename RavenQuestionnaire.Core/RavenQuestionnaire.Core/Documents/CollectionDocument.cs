// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CollectionDocument.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The CollectionDocument interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Documents
{
    using System.Collections.Generic;

    using RavenQuestionnaire.Core.Entities.SubEntities;

    /// <summary>
    /// The CollectionDocument interface.
    /// </summary>
    public interface ICollectionDocument
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
    /// The collection document.
    /// </summary>
    public class CollectionDocument : ICollectionDocument
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionDocument"/> class.
        /// </summary>
        public CollectionDocument()
        {
            this.Items = new List<CollectionItem>();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the items.
        /// </summary>
        public List<CollectionItem> Items { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        #endregion
    }
}
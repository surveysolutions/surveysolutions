﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CollectionDocument.cs" company="">
//   
// </copyright>
// <summary>
//   The CollectionDocument interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.Documents
{
    using System.Collections.Generic;

    using Main.Core.Entities.SubEntities;

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
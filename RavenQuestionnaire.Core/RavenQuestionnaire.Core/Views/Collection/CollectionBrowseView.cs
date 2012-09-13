// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CollectionBrowseView.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The collection browse view.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Core.Views.Collection
{
    using System.Collections.Generic;

    /// <summary>
    /// The collection browse view.
    /// </summary>
    public class CollectionBrowseView
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionBrowseView"/> class.
        /// </summary>
        /// <param name="page">
        /// The page.
        /// </param>
        /// <param name="pageSize">
        /// The page size.
        /// </param>
        /// <param name="totalCount">
        /// The total count.
        /// </param>
        /// <param name="items">
        /// The items.
        /// </param>
        public CollectionBrowseView(int page, int pageSize, int totalCount, IEnumerable<CollectionBrowseItem> items)
        {
            this.TotalCount = totalCount;
            this.Items = items;
            this.Page = page;
            this.PageSize = pageSize;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the items.
        /// </summary>
        public IEnumerable<CollectionBrowseItem> Items { get; private set; }

        /// <summary>
        /// Gets the page.
        /// </summary>
        public int Page { get; private set; }

        /// <summary>
        /// Gets the page size.
        /// </summary>
        public int PageSize { get; private set; }

        /// <summary>
        /// Gets the total count.
        /// </summary>
        public int TotalCount { get; private set; }

        #endregion
    }
}
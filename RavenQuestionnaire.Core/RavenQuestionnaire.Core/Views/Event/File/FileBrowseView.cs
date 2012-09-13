// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileBrowseView.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The file browse view.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Core.Views.Event.File
{
    using System.Collections.Generic;

    /// <summary>
    /// The file browse view.
    /// </summary>
    public class FileBrowseView
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FileBrowseView"/> class.
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
        public FileBrowseView(int page, int pageSize, int totalCount, IEnumerable<FileBrowseItem> items)
        {
            this.Page = page;
            this.TotalCount = totalCount;
            this.PageSize = pageSize;
            this.Items = items;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the items.
        /// </summary>
        public IEnumerable<FileBrowseItem> Items { get; private set; }

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
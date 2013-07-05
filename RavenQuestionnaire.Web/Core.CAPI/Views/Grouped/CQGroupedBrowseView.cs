using System.Collections.Generic;

namespace Core.CAPI.Views.Grouped
{
    /// <summary>
    /// The cq grouped browse view.
    /// </summary>
    public class CQGroupedBrowseView
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CQGroupedBrowseView"/> class.
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
        /// <param name="groups">
        /// The groups.
        /// </param>
        public CQGroupedBrowseView(int page, int pageSize, int totalCount, IList<CQGroupItem> groups)
        {
            this.Page = page;
            this.TotalCount = totalCount;
            this.PageSize = pageSize;
            this.Groups = groups;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the groups.
        /// </summary>
        public IList<CQGroupItem> Groups { get; private set; }

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
using System.Collections.Generic;

namespace Main.Core.View.User
{
    /// <summary>
    /// The user browse view.
    /// </summary>
    public class UserBrowseView
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UserBrowseView"/> class.
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
        public UserBrowseView(int page, int pageSize, int totalCount, IEnumerable<UserBrowseItem> items)
        {
            this.Page = page;
            this.Items = items;
            this.PageSize = pageSize;
            this.TotalCount = totalCount;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the items.
        /// </summary>
        public IEnumerable<UserBrowseItem> Items { get; private set; }

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
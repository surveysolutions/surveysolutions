namespace Core.Supervisor.Views.User
{
    using System.Collections.Generic;

    /// <summary>
    /// The user list view.
    /// </summary>
    public class UserListView
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UserListView"/> class.
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
        /// <param name="order">
        /// The order.
        /// </param>
        public UserListView(int page, int pageSize, int totalCount, IEnumerable<UserListItem> items, string order)
        {
            this.Page = page;
            this.TotalCount = totalCount;
            this.PageSize = pageSize;
            this.Items = items;
            this.Order = order;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the items.
        /// </summary>
        public IEnumerable<UserListItem> Items { get; private set; }

        /// <summary>
        ///     Gets or sets the order.
        /// </summary>
        public string Order { get; private set; }

        /// <summary>
        ///     Gets the page.
        /// </summary>
        public int Page { get; private set; }

        /// <summary>
        ///     Gets the page size.
        /// </summary>
        public int PageSize { get; private set; }

        /// <summary>
        ///     Gets the total count.
        /// </summary>
        public int TotalCount { get; private set; }

        #endregion
    }
}
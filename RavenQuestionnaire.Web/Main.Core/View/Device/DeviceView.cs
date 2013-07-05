namespace Main.Core.View.Device
{
    using System.Collections.Generic;

    using Main.Core.Entities;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class DeviceView
    {

        #region Fields

        /// <summary>
        /// The _order.
        /// </summary>
        private string _order = string.Empty;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceView"/> class.
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
        public DeviceView(
            int page, int pageSize, int totalCount, List<RegisterData> items, string order)
        {
            this.Page = page;
            this.TotalCount = totalCount;
            this.PageSize = pageSize;
            this.Items = items;
            this.Order = order;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets Items.
        /// </summary>
        public List<RegisterData> Items { get; private set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        public string Order
        {
            get
            {
                return this._order;
            }

            set
            {
                this._order = value;
            }
        }

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

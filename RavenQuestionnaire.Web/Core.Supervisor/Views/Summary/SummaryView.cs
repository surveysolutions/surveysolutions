// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SummaryView.cs" company="">
//   
// </copyright>
// <summary>
//   TODO: Update summary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Core.Supervisor.Views.Summary
{
    using System.Collections.Generic;

    using Main.Core.Entities;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class SummaryView
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SummaryView"/> class.
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
        public SummaryView(int page, int pageSize, int totalCount)
        {
            this.Orders = new List<OrderRequestItem>();
            this.Order = string.Empty;
            this.Page = page;
            this.PageSize = pageSize;
            this.TotalCount = totalCount;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        public string Order { get; set; }

        /// <summary>
        /// Gets or sets the orders.
        /// </summary>
        public List<OrderRequestItem> Orders { get; set; }

        /// <summary>
        /// Gets the page.
        /// </summary>
        public int Page { get; private set; }

        /// <summary>
        /// Gets the page size.
        /// </summary>
        public int PageSize { get; private set; }

        /// <summary>
        /// Gets or sets Summary.
        /// </summary>
        public SummaryViewItem Summary { get; set; }

        /// <summary>
        /// Gets or sets the total count.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Gets or sets Items.
        /// </summary>
        public List<SummaryViewItem> Items { get; set; }

        #endregion
    }
}
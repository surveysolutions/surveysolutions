// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IndexView.cs" company="">
//   
// </copyright>
// <summary>
//   The survey browse view.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Core.Supervisor.Views.Index
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Documents;
    using Main.Core.Entities;
    using Main.Core.Entities.SubEntities;

    /// <summary>
    /// The survey browse view.
    /// </summary>
    public class IndexView
    {
        #region Constants and Fields

        /// <summary>
        /// The _order.
        /// </summary>
        private string _order = string.Empty;

        /// <summary>
        /// The _orders.
        /// </summary>
        private List<OrderRequestItem> _orders = new List<OrderRequestItem>();

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexView"/> class.
        /// </summary>
        public IndexView()
        {
            this.Items = new List<IndexViewItem>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexView"/> class.
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
        /// <param name="user">
        /// The user.
        /// </param>
        public IndexView(
            int page, int pageSize, int totalCount, IEnumerable<IndexViewItem> items, UserDocument user)
            : this()
        {
            this.User = user == null ? new UserLight(Guid.Empty, "All") : new UserLight(user.PublicKey, user.UserName);
            this.Page = page;
            this.TotalCount = totalCount;
            this.PageSize = pageSize;
            this.Headers =
                new SurveyGroupedByStatusHeader(
                    new Dictionary<string, string>
                        {
                            { "Unassign", "Unassign" },
                            { "Initial", SurveyStatus.Initial.Name },
                            { "Redo", SurveyStatus.Redo.Name },
                            { "Complete", SurveyStatus.Complete.Name },
                            { "Error", SurveyStatus.Error.Name },
                            { "Approve", SurveyStatus.Approve.Name },
                            { "Total", "Total" },
                        });

            foreach (IndexViewItem item in items)
            {
                this.Items.Add(
                    new IndexViewItem(
                        item.Id,
                        item.Title,
                        item.Unassign,
                        item.Total,
                        item.Initial,
                        item.Error,
                        item.Complete,
                        item.Approve,
                        item.Redo));
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the headers.
        /// </summary>
        public SurveyGroupedByStatusHeader Headers { get; set; }

        /// <summary>
        /// Gets or sets the items.
        /// </summary>
        public List<IndexViewItem> Items { get; set; }

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
        /// Gets or sets the orders.
        /// </summary>
        public List<OrderRequestItem> Orders
        {
            get
            {
                return this._orders;
            }

            set
            {
                this._orders = value;
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
        /// Gets and sets the total count.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Gets User.
        /// </summary>
        public UserLight User { get; private set; }

        /// <summary>
        /// Gets or sets Summary.
        /// </summary>
        public IndexViewItem Summary { get; set; }

        public object Chart { get; set; }

        #endregion
    }
}
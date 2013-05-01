// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IndexView.cs" company="">
//   
// </copyright>
// <summary>
//   The survey browse view.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Linq;

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

        public IndexView(
            int page, int pageSize,  IEnumerable<IndexViewItem> items, UserDocument user)
            : this()
        {
            this.User = user == null ? new UserLight(Guid.Empty, "All") : new UserLight(user.PublicKey, user.UserName);
            this.Page = page;
            this.PageSize = pageSize;
            this.Headers =
                new SurveyGroupedByStatusHeader(
                    new Dictionary<string, string>
                        {
                            { "Unassigned", "Unassigned" },
                            { "Initial", SurveyStatus.Initial.Name },
                            { "Redo", SurveyStatus.Redo.Name },
                            { "Completed", SurveyStatus.Complete.Name },
                            { "Error", SurveyStatus.Error.Name },
                            { "Approved", SurveyStatus.Approve.Name },
                            { "Total", "Total" },
                        });

            foreach (IndexViewItem item in items)
            {
                this.Items.Add(
                    new IndexViewItem(
                        item.Id,
                        item.Title,
                        item.Unassigned,
                        item.Total,
                        item.Initial,
                        item.Error,
                        item.Completed,
                        item.Approved,
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
        public int TotalCount {
            get { return Items.Count; }
        }

        /// <summary>
        /// Gets User.
        /// </summary>
        public UserLight User { get; private set; }

        /// <summary>
        /// Gets or sets Summary.
        /// </summary>
        public IndexViewItem Summary
        {
            get
            {
                if (_summary == null)
                {
                    _summary = new IndexViewItem(
                        Guid.Empty,
                        "Summary",
                        Items.Sum(x => x.Unassigned),
                        Items.Sum(x => x.Total),
                        Items.Sum(x => x.Initial),
                        Items.Sum(x => x.Error),
                        Items.Sum(x => x.Completed),
                        Items.Sum(x => x.Approved),
                        Items.Sum(x => x.Redo));
                }
                return _summary;
            }
        }

        private IndexViewItem _summary;

        #endregion
    }
}
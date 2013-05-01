// -----------------------------------------------------------------------
// <copyright file="SummaryInputModel.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Core.Supervisor.Views.Summary
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Main.Core.Entities;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Utility;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class SummaryInputModel
    {
        /// <summary>
        /// The _orders.
        /// </summary>
        private List<OrderRequestItem> _orders = new List<OrderRequestItem>();

        /// <summary>
        /// The _page.
        /// </summary>
        private int _page = 1;

        /// <summary>
        /// The _page size.
        /// </summary>
        private int _pageSize = 20;
        
        public SummaryInputModel(UserLight viewer)
        {
            this.ViewerId = viewer.Id;
        }

        /// <summary>
        /// Gets or sets ViewerId.
        /// </summary>
        public Guid ViewerId { get; set; }

        /// <summary>
        /// Gets or sets TemplateId.
        /// </summary>
        public Guid? TemplateId { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        public string Order
        {
            get
            {
                return StringUtil.GetOrderRequestString(this._orders);
            }

            set
            {
                this._orders = StringUtil.ParseOrderRequestString(value);
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
                this._orders = value ?? new List<OrderRequestItem>();
            }
        }

        /// <summary>
        /// Gets or sets the page.
        /// </summary>
        public int Page
        {
            get
            {
                return this._page;
            }

            set
            {
                this._page = value;
            }
        }

        /// <summary>
        /// Gets or sets the page size.
        /// </summary>
        public int PageSize
        {
            get
            {
                return this._pageSize;
            }

            set
            {
                this._pageSize = value;
            }
        }
    }
}

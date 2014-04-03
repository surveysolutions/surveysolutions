﻿using System.Collections.Generic;
using Main.Core.Entities;
using Main.Core.Utility;

namespace WB.Core.SharedKernels.SurveyManagement.Views
{
    public class ListViewModelBase
    {
        /// <summary>
        ///     The _orders.
        /// </summary>
        private IEnumerable<OrderRequestItem> _orders = new List<OrderRequestItem>();

        /// <summary>
        ///     The _page.
        /// </summary>
        private int _page = 1;

        /// <summary>
        ///     The _page size.
        /// </summary>
        private int _pageSize = 20;

        /// <summary>
        ///     Gets or sets the order.
        /// </summary>
        public string Order
        {
            get { return StringUtil.GetOrderRequestString(this._orders); }

            set { this._orders = StringUtil.ParseOrderRequestString(value); }
        }

        /// <summary>
        ///     Gets or sets the orders.
        /// </summary>
        public IEnumerable<OrderRequestItem> Orders
        {
            get { return this._orders; }

            set { this._orders = value ?? new List<OrderRequestItem>(); }
        }

        /// <summary>
        ///     Gets or sets the page.
        /// </summary>
        public int Page
        {
            get { return this._page; }

            set { this._page = value; }
        }

        /// <summary>
        ///     Gets or sets the page size.
        /// </summary>
        public int PageSize
        {
            get { return this._pageSize; }

            set { this._pageSize = value; }
        }
    }
}

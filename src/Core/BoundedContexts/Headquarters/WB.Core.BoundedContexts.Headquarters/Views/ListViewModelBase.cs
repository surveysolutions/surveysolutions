using System.Collections.Generic;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.BoundedContexts.Headquarters.Views
{
    public class ListViewModelBase
    {
        /// <summary>
        ///     Gets or sets the order.
        /// </summary>
        public string Order
        {
            get => this.Orders.GetOrderRequestString();
            set => this.Orders = value.ParseOrderRequestString();
        }

        /// <summary>
        ///     Gets or sets the orders.
        /// </summary>
        public IEnumerable<OrderRequestItem> Orders { get; set; } = new List<OrderRequestItem>();

        /// <summary>
        ///     Gets or sets the page.
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        ///     Gets or sets the page size.
        /// </summary>
        public int PageSize { get; set; } = 20;
    }
}

using Main.Core.Entities;
using Main.Core.Utility;
using System.Collections.Generic;

namespace Designer.Web.Providers.CQRS
{
    /// <summary>
    /// The account list input model.
    /// </summary>
    public class AccountListViewInputModel
    {
        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        public string Order
        {
            get
            {
                return StringUtil.GetOrderRequestString(Orders);
            }

            set
            {
                Orders = StringUtil.ParseOrderRequestString(value);
            }
        }

        /// <summary>
        /// Gets or sets the orders.
        /// </summary>
        public List<OrderRequestItem> Orders = new List<OrderRequestItem>();

        /// <summary>
        /// Gets or sets the page.
        /// </summary>
        public int Page = 1;

        /// <summary>
        /// Gets or sets the page size.
        /// </summary>
        public int PageSize = 20;

        /// <summary>
        /// Get or set accounts online only
        /// </summary>
        public bool IsOnlineOnly = false;

        /// <summary>
        /// Get or set account new only
        /// </summary>
        public bool IsNewOnly = false;

        /// <summary>
        /// Get or set account by account name
        /// </summary>
        public string AccountName = string.Empty;

        /// <summary>
        /// Get or set account by account email
        /// </summary>
        public string AccountEmail = string.Empty;
    }
}

using Main.Core.Entities;
using Main.Core.Utility;
using System.Collections.Generic;

namespace WB.UI.Designer.Providers.CQRS.Roles.View
{
    /// <summary>
    /// The account list input model.
    /// </summary>
    public class RoleListViewInputModel
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
        /// Get or set account by account name
        /// </summary>
        public string RoleName = string.Empty;
    }
}

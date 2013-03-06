using WB.UI.Designer.Providers.Roles;
using Main.Core.Entities;
using Main.Core.Utility;
using System.Collections.Generic;

namespace WB.UI.Designer.Providers.CQRS.Accounts.View
{
    using WB.UI.Designer.Providers.Roles;

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
        /// Get online accounts only
        /// </summary>
        public bool IsOnlineOnly = false;

        /// <summary>
        /// Get new accounts only
        /// </summary>
        public bool IsNewOnly = false;

        /// <summary>
        /// Get accounts by name
        /// </summary>
        public string Name = string.Empty;

        /// <summary>
        /// Get accounts by email
        /// </summary>
        public string Email = string.Empty;

        /// <summary>
        /// Get accounts by role
        /// </summary>
        public SimpleRoleEnum Role = SimpleRoleEnum.Undefined;
    }
}

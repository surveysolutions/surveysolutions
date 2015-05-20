using System.Collections.Generic;
using WB.Core.GenericSubdomains.Portable;
using WB.UI.Shared.Web.MembershipProvider.Roles;

namespace WB.Core.BoundedContexts.Designer.Views.Account
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
                return this.Orders.GetOrderRequestString();
            }

            set
            {
                this.Orders = value.ParseOrderRequestString();
            }
        }

        /// <summary>
        /// Get accounts by name
        /// </summary>
        public string Name = string.Empty;

        /// <summary>
        /// Gets or sets the orders.
        /// </summary>
        public IEnumerable<OrderRequestItem> Orders = new List<OrderRequestItem>();

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
        /// Get accounts by email
        /// </summary>
        public string Email = string.Empty;

        /// <summary>
        /// Get accounts by role
        /// </summary>
        public SimpleRoleEnum Role = SimpleRoleEnum.Undefined;
    }
}

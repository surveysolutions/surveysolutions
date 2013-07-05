namespace Core.Supervisor.Views.User
{
    using System.Collections.Generic;

    using Main.Core.Entities;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Utility;

    /// <summary>
    /// The user list view input model.
    /// </summary>
    public class UserListViewInputModel
    {
        #region Fields
        
        /// <summary>
        ///     Gets or sets the orders.
        /// </summary>
        public List<OrderRequestItem> Orders = new List<OrderRequestItem>();

        /// <summary>
        ///     Gets or sets the page.
        /// </summary>
        public int Page = 1;

        /// <summary>
        ///     Gets or sets the page size.
        /// </summary>
        public int PageSize = 20;

        /// <summary>
        ///     Get users by role
        /// </summary>
        public UserRoles Role = UserRoles.Undefined;

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the order.
        /// </summary>
        public string Order
        {
            get
            {
                return StringUtil.GetOrderRequestString(this.Orders);
            }

            set
            {
                this.Orders = StringUtil.ParseOrderRequestString(value);
            }
        }

        #endregion
    }
}
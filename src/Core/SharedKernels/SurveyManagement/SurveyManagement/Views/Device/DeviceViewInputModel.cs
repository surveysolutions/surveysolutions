using System;
using System.Collections.Generic;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Device
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class DeviceViewInputModel
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceViewInputModel"/> class.
        /// </summary>
        public DeviceViewInputModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceViewInputModel"/> class.
        /// </summary>
        /// <param name="registratorId">
        /// The registrator id.
        /// </param>
        public DeviceViewInputModel(Guid registratorId)
        {
            this.RegistratorId = registratorId;
        }

        #endregion
        
        #region Properties

        /// <summary>
        /// Gets or sets RegistratorId.
        /// </summary>
        public Guid RegistratorId { get; set; }

        /// <summary>
        /// Gets or sets Order.
        /// </summary>
        public string Order
        {
            get
            {
                return this._orders.GetOrderRequestString();
            }

            set
            {
                this._orders = value.ParseOrderRequestString();
            }
        }

        /// <summary>
        /// Gets or sets the orders.
        /// </summary>
        public IEnumerable<OrderRequestItem> Orders
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

        #endregion

        #region Fields

        /// <summary>
        /// The _orders.
        /// </summary>
        private IEnumerable<OrderRequestItem> _orders = new List<OrderRequestItem>();

        /// <summary>
        /// The _page.
        /// </summary>
        private int _page = 1;

        /// <summary>
        /// The _page size.
        /// </summary>
        private int _pageSize = 20;

        #endregion
    }
}

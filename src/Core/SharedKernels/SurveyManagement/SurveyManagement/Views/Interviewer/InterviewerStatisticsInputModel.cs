using System;
using System.Collections.Generic;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interviewer
{
    /// <summary>
    /// Input model for interviewer statisctic page
    /// </summary>
    public class InterviewerStatisticsInputModel
    {
        #region Fields

        /// <summary>
        /// The _orders.
        /// </summary>
        private IEnumerable<OrderRequestItem> orders = new List<OrderRequestItem>();

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="InterviewerStatisticsInputModel"/> class.
        /// </summary>
        public InterviewerStatisticsInputModel()
        {
            this.Page = 1;
            this.PageSize = 20;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        public string Order
        {
            get
            {
                return this.orders.GetOrderRequestString();
            }

            set
            {
                this.orders = value.ParseOrderRequestString();
            }
        }

        /// <summary>
        /// Gets or sets the orders.
        /// </summary>
        public IEnumerable<OrderRequestItem> Orders
        {
            get
            {
                return this.orders;
            }

            set
            {
                this.orders = value;
            }
        }

        /// <summary>
        /// Gets or sets the page.
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Gets or sets the page size.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets or sets a user public key.
        /// </summary>
        public Guid? InterviewerId { get; set; }

        #endregion
    }
}
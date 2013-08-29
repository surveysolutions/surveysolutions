using System;
using System.Collections.Generic;
using Main.Core.Entities;
using Main.Core.Utility;

namespace Main.Core.View.CompleteQuestionnaire
{
    /// <summary>
    /// The complete questionnaire browse input model.
    /// </summary>
    public class CompleteQuestionnaireBrowseInputModel
    {
        #region Fields

        /// <summary>
        /// The _orders.
        /// </summary>
        public List<OrderRequestItem> orders = new List<OrderRequestItem>();

        /// <summary>
        /// The _page.
        /// </summary>
        private int page = 1;

        /// <summary>
        /// The _page size.
        /// </summary>
        private int pageSize = 20;

        /// <summary>
        /// The _responsible id.
        /// </summary>
        private Guid responsibleId = Guid.Empty;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        public string Order
        {
            get
            {
                return StringUtil.GetOrderRequestString(this.orders);
            }

            set
            {
                this.orders = StringUtil.ParseOrderRequestString(value);
            }
        }

        /// <summary>
        /// Gets or sets the orders.
        /// </summary>
        public List<OrderRequestItem> Orders
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
        public int Page
        {
            get
            {
                return this.page;
            }

            set
            {
                this.page = value;
            }
        }

        /// <summary>
        /// Gets or sets the page size.
        /// </summary>
        public int PageSize
        {
            get
            {
                return this.pageSize;
            }

            set
            {
                this.pageSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the responsible id.
        /// </summary>
        public Guid ResponsibleId
        {
            get
            {
                return this.responsibleId;
            }

            set
            {
                this.responsibleId = value;
            }
        }

        #endregion
    }
}
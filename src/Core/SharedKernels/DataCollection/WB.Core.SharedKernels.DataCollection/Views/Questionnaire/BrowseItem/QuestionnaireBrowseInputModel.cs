using System;
using System.Collections.Generic;
using Main.Core.Entities;
using Main.Core.Utility;

namespace WB.Core.SharedKernels.DataCollection.Views.Questionnaire.BrowseItem
{
    /// <summary>
    /// The questionnaire browse input model.
    /// </summary>
    public class QuestionnaireBrowseInputModel
    {
        #region Fields

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionnaireBrowseInputModel"/> class.
        /// </summary>
        public QuestionnaireBrowseInputModel()
        {
            this.Orders = new List<OrderRequestItem>();
            this.PageSize = 20;
            this.Page = 1;
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
                return StringUtil.GetOrderRequestString(this.Orders);
            }

            set
            {
                this.Orders = StringUtil.ParseOrderRequestString(value);
            }
        }

        /// <summary>
        /// Gets or sets the orders.
        /// </summary>
        public List<OrderRequestItem> Orders { get; set; }

        /// <summary>
        /// Gets or sets the page.
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Gets or sets the page size.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets or sets the is admin mode.
        /// </summary>
        public bool? IsAdminMode { get; set; }

        /// <summary>
        /// Gets or sets the created by.
        /// </summary>
        public Guid CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is only owner items.
        /// </summary>
        public bool IsOnlyOwnerItems { get; set; }

        /// <summary>
        /// Gets or sets the filter.
        /// </summary>
        public string Filter { get; set; }

        #endregion
    }
}
namespace Core.Supervisor.Views.Template
{
    using System.Collections.Generic;

    using Main.Core.Entities;
    using Main.Core.Utility;

    /// <summary>
    /// The questionnaire list input model.
    /// </summary>
    public class QuestionnaireListInputModel
    {
        #region Fields

        /// <summary>
        /// The orders.
        /// </summary>
        private List<OrderRequestItem> orders = new List<OrderRequestItem>();

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionnaireListInputModel"/> class.
        /// </summary>
        public QuestionnaireListInputModel()
        {
            this.Page = 1;
            this.PageSize = 20;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the order.
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
        ///     Gets or sets the orders.
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
        ///     Gets or sets the page.
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        ///     Gets or sets the page size.
        /// </summary>
        public int PageSize { get; set; }

        #endregion
    }
}
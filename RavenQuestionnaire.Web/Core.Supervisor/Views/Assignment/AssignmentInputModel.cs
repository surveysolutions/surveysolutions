// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssignmentInputModel.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The survey group input model.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Core.Supervisor.Views.Assignment
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Entities;
    using Main.Core.Utility;
    using Main.Core.View.CompleteQuestionnaire;

    /// <summary>
    /// The survey group input model.
    /// </summary>
    public class AssignmentInputModel
    {
        #region Fields

        /// <summary>
        /// The _orders.
        /// </summary>
        private List<OrderRequestItem> orders = new List<OrderRequestItem>();

        /// <summary>
        /// The _page.
        /// </summary>
        private int page = 1;

        /// <summary>
        /// The _page size.
        /// </summary>
        private int pageSize = 20;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AssignmentInputModel"/> class.
        /// </summary>
        public AssignmentInputModel()
        {
        }

        public AssignmentInputModel(Guid? templateId, Guid? interviewerId, int? page,
                                    int? pageSize,
                                    List<OrderRequestItem> sortOrder, Guid? status, bool isNotAssigned)
        {
            this.TemplateId = templateId;
            this.StatusId = status;
            this.IsNotAssigned = isNotAssigned;
            this.InterviewerId = interviewerId;
            if (page.HasValue)
                this.Page = page.Value;
            if (pageSize.HasValue)
                this.PageSize = pageSize.Value;
            this.Orders = sortOrder;
        }


        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public Guid? TemplateId { get; set; }

        /// <summary>
        /// Gets or sets InterviewerId.
        /// </summary>
        public Guid? InterviewerId { get; set; }

        /// <summary>
        /// Gets or sets the questionnaire id.
        /// </summary>
        public Guid? QuestionnaireId { get; set; }

        /// <summary>
        /// Gets or sets Statuses.
        /// </summary>
        public Guid? StatusId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IsNotAssigned.
        /// </summary>
        public bool IsNotAssigned { get; set; }

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
                this.orders = value ?? new List<OrderRequestItem>();
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

        #endregion
    }
}
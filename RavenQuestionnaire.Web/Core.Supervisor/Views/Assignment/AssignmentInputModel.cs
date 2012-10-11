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

        /// <summary>
        /// Initializes a new instance of the <see cref="AssignmentInputModel"/> class.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="questionnaireId">
        /// The questionnaire id.
        /// </param>
        public AssignmentInputModel(Guid id, Guid questionnaireId)
        {
            this.Id = id;
            this.QuestionnaireId = questionnaireId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssignmentInputModel"/> class.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="userId">
        /// The user Id.
        /// </param>
        /// <param name="questionnaireId">
        /// The questionnaire id.
        /// </param>
        /// <param name="statuses">
        /// The statistic filter.
        /// </param>
        public AssignmentInputModel(Guid id, Guid? userId, Guid questionnaireId, ICollection<string> statuses)
        {
            this.Id = id;
            this.QuestionnaireId = questionnaireId;
            this.Statuses = statuses;
            this.UserId = userId;
        }



        /// <summary>
        /// Initializes a new instance of the <see cref="AssignmentInputModel"/> class.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="userId">
        /// The user Id.
        /// </param>
        /// <param name="page">
        /// The page.
        /// </param>
        /// <param name="pageSize">
        /// The page size.
        /// </param>
        /// <param name="orders">
        /// The orders.
        /// </param>
        /// <param name="statuses">
        /// The statuses.
        /// </param>
        /// <param name="isNotAssigned">
        /// The isNotAssigned.
        /// </param>
        public AssignmentInputModel(Guid id, Guid? userId, int page, int pageSize, List<OrderRequestItem> orders, ICollection<string> statuses, bool isNotAssigned)
        {
            this.Id = id;
            this.Page = page;
            this.PageSize = pageSize;
            this.Orders = orders;
            this.Statuses = statuses;
            this.IsNotAssigned = isNotAssigned;
            this.UserId = userId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssignmentInputModel"/> class.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="userId">
        /// The user Id.
        /// </param>
        /// <param name="page">
        /// The page.
        /// </param>
        /// <param name="pageSize">
        /// The page size.
        /// </param>
        /// <param name="orders">
        /// The orders.
        /// </param>
        public AssignmentInputModel(Guid id, Guid? userId, int page, int pageSize, List<OrderRequestItem> orders)
        {
            this.Id = id;
            this.Page = page;
            this.PageSize = pageSize;
            this.Orders = orders;
            this.UserId = userId;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the expression.
        /// </summary>
        public Func<CompleteQuestionnaireBrowseItem, bool> Expression
        {
            get
            {
                return x => x.TemplateId == this.Id;
            }
        }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets UserId.
        /// </summary>
        public Guid? UserId { get; set; }

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

        /// <summary>
        /// Gets or sets the questionnaire id.
        /// </summary>
        public Guid QuestionnaireId { get; set; }

        /// <summary>
        /// Gets or sets Statuses.
        /// </summary>
        public ICollection<string> Statuses { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IsNotAssigned.
        /// </summary>
        public bool IsNotAssigned { get; set; }

        #endregion
    }
}
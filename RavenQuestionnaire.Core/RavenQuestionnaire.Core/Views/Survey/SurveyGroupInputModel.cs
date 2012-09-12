// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SurveyGroupInputModel.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The survey group input model.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Views.Survey
{
    using System;
    using System.Collections.Generic;
    using RavenQuestionnaire.Core.Entities;
    using RavenQuestionnaire.Core.Utility;
    using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;

    /// <summary>
    /// The survey group input model.
    /// </summary>
    public class SurveyGroupInputModel
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
        /// Initializes a new instance of the <see cref="SurveyGroupInputModel"/> class.
        /// </summary>
        public SurveyGroupInputModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SurveyGroupInputModel"/> class.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="questionnaireId">
        /// The questionnaire id.
        /// </param>
        public SurveyGroupInputModel(Guid id, Guid questionnaireId)
        {
            this.Id = id;
            this.QuestionnaireId = questionnaireId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SurveyGroupInputModel"/> class.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="questionnaireId">
        /// The questionnaire id.
        /// </param>
        /// /// <param name="status">
        /// The statistic filter.
        /// </param>
        public SurveyGroupInputModel(Guid id, Guid questionnaireId, string status)
        {
            this.Id = id;
            this.QuestionnaireId = questionnaireId;
            this.Status = status;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="SurveyGroupInputModel"/> class.
        /// </summary>
        /// <param name="id">
        /// The id.
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
        public SurveyGroupInputModel(Guid id, int page, int pageSize, List<OrderRequestItem> orders, string status)
        {
            this.Id = id;
            this.Page = page;
            this.PageSize = pageSize;
            this.Orders = orders;
            this.Status = status;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SurveyGroupInputModel"/> class.
        /// </summary>
        /// <param name="id">
        /// The id.
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
        /// <param name="status">
        /// The statistic filter
        /// </param>
        public SurveyGroupInputModel(Guid id, int page, int pageSize, List<OrderRequestItem> orders)
        {
            this.Id = id;
            this.Page = page;
            this.PageSize = pageSize;
            this.Orders = orders;
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
        /// Gets or sets the questionnaire id.
        /// </summary>
        public Guid QuestionnaireId { get; set; }

        public string Status { get; set; }

        #endregion
    }
}
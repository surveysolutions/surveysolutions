﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InterviewerInputModel.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The interviewer input model.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Core.Views.Interviewer
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Entities;
    using Main.Core.Utility;

    using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;

    /// <summary>
    /// The interviewer input model.
    /// </summary>
    public class InterviewerInputModel
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
        private int pageSize = 10;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the expression.
        /// </summary>
        public Func<CompleteQuestionnaireBrowseItem, bool> Expression
        {
            get
            {
                return q => (q.Responsible != null && q.Responsible.Id == this.UserId);
            }
        }

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
        /// Gets or sets the template id.
        /// </summary>
        public Guid TemplateId { get; set; }

        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        public Guid UserId { get; set; }

        #endregion
    }
}
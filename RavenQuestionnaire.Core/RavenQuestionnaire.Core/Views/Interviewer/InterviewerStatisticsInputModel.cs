﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InterviewerStatisticsInputModel.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The interviewers input model.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Core.Views.Interviewer
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Entities;
    using Main.Core.Utility;

    /// <summary>
    /// Input model for interviewer statisctic page
    /// </summary>
    public class InterviewerStatisticsInputModel
    {
        #region Fields

        /// <summary>
        /// The _orders.
        /// </summary>
        private List<OrderRequestItem> orders = new List<OrderRequestItem>();

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
        public int Page { get; set; }

        /// <summary>
        /// Gets or sets the page size.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets or sets a user public key.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets UserName.
        /// </summary>
        public string UserName { get; set; }

        #endregion
    }
}
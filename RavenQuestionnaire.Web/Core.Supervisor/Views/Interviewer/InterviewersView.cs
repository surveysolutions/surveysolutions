// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InterviewersView.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The interviewers view.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Core.Supervisor.Views.Interviewer
{
    /// <summary>
    /// The interviewers view.
    /// </summary>
    public class InterviewersView
    {
        #region Fields

        /// <summary>
        /// The _order.
        /// </summary>
        private string order = string.Empty;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="InterviewersView"/> class.
        /// </summary>
        /// <param name="page">
        /// The page.
        /// </param>
        /// <param name="pageSize">
        /// The page size.
        /// </param>
        /// <param name="totalCount">
        /// The total count.
        /// </param>
        /// <param name="items">
        /// The items.
        /// </param>
        /// <param name="supervisorId">
        /// The supervisor id.
        /// </param>
        public InterviewersView(
            int page, 
            int pageSize, 
            int totalCount, 
            IEnumerable<InterviewersItem> items, 
            Guid supervisorId)
        {
            this.Page = page;
            this.TotalCount = totalCount;
            this.PageSize = pageSize;
            this.Items = items;
            this.SupervisorId = supervisorId;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the items.
        /// </summary>
        public IEnumerable<InterviewersItem> Items { get; private set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        public string Order
        {
            get
            {
                return this.order;
            }

            set
            {
                this.order = value;
            }
        }

        /// <summary>
        /// Gets the page.
        /// </summary>
        public int Page { get; private set; }

        /// <summary>
        /// Gets the page size.
        /// </summary>
        public int PageSize { get; private set; }

        /// <summary>
        /// Gets the supervisor id.
        /// </summary>
        public Guid SupervisorId { get; private set; }

        /// <summary>
        /// Gets the total count.
        /// </summary>
        public int TotalCount { get; private set; }

        #endregion
    }
}
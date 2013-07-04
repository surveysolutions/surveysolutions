using System;
using System.Collections.Generic;
using System.Linq;

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

        
        public InterviewersView(
            int page, 
            int pageSize, 
            IEnumerable<InterviewersItem> items, 
            Guid supervisorId)
        {
            this.Page = page;
            this.PageSize = pageSize;
            this.Items = items.ToList();
            this.ViewerId = supervisorId;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the items.
        /// </summary>
        public IList<InterviewersItem> Items { get; private set; }

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
        public Guid ViewerId { get; private set; }

        /// <summary>
        /// Gets the total count.
        /// </summary>
        public int TotalCount {
            get { return Items.Count; }
        }

        #endregion
    }
}
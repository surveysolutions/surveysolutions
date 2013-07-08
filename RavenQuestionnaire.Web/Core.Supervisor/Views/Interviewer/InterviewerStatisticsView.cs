using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;

namespace Core.Supervisor.Views.Interviewer
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class InterviewerStatisticsView
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="InterviewerStatisticsView"/> class.
        /// </summary>
        /// <param name="id">
        /// Interviewer's ID
        /// </param>
        /// <param name="name">
        /// Interviewer login
        /// </param>
        /// <param name="order">
        /// The order.
        /// </param>
        /// <param name="items">
        /// List of table rows 
        /// </param>
        /// <param name="page">
        /// The page.
        /// </param>
        /// <param name="pageSize">
        /// The page size.
        /// </param>
        /// <param name="totalCount">
        /// The total count.
        /// </param>
        public InterviewerStatisticsView(
            Guid id, 
            string name, 
            string order, 
            List<InterviewerStatisticsViewItem> items, 
            int page, 
            int pageSize, 
            int totalCount)
        {
            this.Id = id;
            this.Name = name;
            this.Order = order;
            this.Items = items;
            this.Page = page;
            this.PageSize = pageSize;
            this.TotalCount = totalCount;
            this.Headers = new Dictionary<Guid, string>
                {
                    { SurveyStatus.Initial.PublicId, SurveyStatus.Initial.Name }, 
                    { SurveyStatus.Error.PublicId, SurveyStatus.Error.Name }, 
                    { SurveyStatus.Complete.PublicId, SurveyStatus.Complete.Name }, 
                    { SurveyStatus.Approve.PublicId, SurveyStatus.Approve.Name },
                    { SurveyStatus.Redo.PublicId, SurveyStatus.Redo.Name }
                };
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets table headers.
        /// </summary>
        public Dictionary<Guid, string> Headers { get; set; }

        /// <summary>
        /// Gets or sets interviewer id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets Items.
        /// </summary>
        public List<InterviewerStatisticsViewItem> Items { get; set; }

        /// <summary>
        /// Gets or sets interviewer name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        public string Order { get; set; }

        /// <summary>
        /// Gets the page.
        /// </summary>
        public int Page { get; private set; }

        /// <summary>
        /// Gets the page size.
        /// </summary>
        public int PageSize { get; private set; }

        /// <summary>
        /// Gets the total count.
        /// </summary>
        public int TotalCount { get; private set; }

        #endregion
    }
}
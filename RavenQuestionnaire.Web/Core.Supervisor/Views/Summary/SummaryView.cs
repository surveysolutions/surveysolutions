// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SummaryView.cs" company="">
//   
// </copyright>
// <summary>
//   TODO: Update summary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Core.Supervisor.Views.Summary
{
    using System.Collections.Generic;

    using Core.Supervisor.Views.Index;

    using Main.Core.Entities;
    using Main.Core.Entities.SubEntities;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class SummaryView
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SummaryView"/> class.
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
        /// <param name="template">
        /// The template.
        /// </param>
        public SummaryView(int page, int pageSize, int totalCount, TemplateLight template)
        {
            this.Orders = new List<OrderRequestItem>();
            this.Order = string.Empty;
            this.Page = page;
            this.PageSize = pageSize;
            this.TotalCount = totalCount;
            this.Template = template;
            this.Headers = new SurveyGroupedByStatusHeader(
                    new Dictionary<string, string>
                        {
                            { "Initial", SurveyStatus.Initial.Name },
                            { "Redo", SurveyStatus.Redo.Name },
                            { "Complete", SurveyStatus.Complete.Name },
                            { "Error", SurveyStatus.Error.Name },
                            { "Approve", SurveyStatus.Approve.Name },
                            { "Total", "Total" },
                        });
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the headers.
        /// </summary>
        public SurveyGroupedByStatusHeader Headers { get; set; }

        /// <summary>
        /// Gets or sets Template.
        /// </summary>
        public TemplateLight Template { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        public string Order { get; set; }

        /// <summary>
        /// Gets or sets the orders.
        /// </summary>
        public List<OrderRequestItem> Orders { get; set; }

        /// <summary>
        /// Gets the page.
        /// </summary>
        public int Page { get; private set; }

        /// <summary>
        /// Gets the page size.
        /// </summary>
        public int PageSize { get; private set; }

        /// <summary>
        /// Gets or sets Summary.
        /// </summary>
        public SummaryViewItem Summary { get; set; }

        /// <summary>
        /// Gets or sets the total count.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Gets or sets Items.
        /// </summary>
        public List<SummaryViewItem> Items { get; set; }

        #endregion
    }
}
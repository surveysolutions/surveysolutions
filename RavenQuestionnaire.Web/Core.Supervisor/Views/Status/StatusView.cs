// -----------------------------------------------------------------------
// <copyright file="StatusView.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Core.Supervisor.Views.Status
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Core.Supervisor.Views.Index;

    using Main.Core.Entities;
    using Main.Core.Entities.SubEntities;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class StatusView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StatusView"/> class.
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
        /// <param name="status">
        /// The status.
        /// </param>
        public StatusView(int page, int pageSize, int totalCount, SurveyStatus status, List<TemplateLight> headers)
        {
            this.Page = page;
            this.PageSize = pageSize;
            this.TotalCount = totalCount;
            this.Status = status;
            this.Headers = headers;
        }

        /// <summary>
        /// Gets or sets the headers.
        /// </summary>
        public List<TemplateLight> Headers { get; set; }

        /// <summary>
        /// Gets or sets Template.
        /// </summary>
        public SurveyStatus Status { get; set; }

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
        public StatusViewItem Summary { get; set; }

        /// <summary>
        /// Gets or sets the total count.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Gets or sets Items.
        /// </summary>
        public List<StatusViewItem> Items { get; set; }

        /// <summary>
        /// Builds summary row
        /// </summary>
        /// <param name="items">
        /// The items.
        /// </param>
        /// <param name="headers">
        /// The headers.
        /// </param>
        public void BuildSummary(IQueryable<StatusViewItem> items, List<TemplateLight> headers)
        {
            var dict = new Dictionary<Guid, int>();
            foreach (var header in headers)
            {
                if (!dict.ContainsKey(header.TemplateId))
                {
                    dict.Add(header.TemplateId, 0);
                }
            }

            foreach (var i in items.SelectMany(item => item.Items))
            {
                dict[i.Key.TemplateId] += i.Value;
            }

            this.Summary = new StatusViewItem(new UserLight(Guid.Empty, "Summary"), dict, headers);
        }
    }
}

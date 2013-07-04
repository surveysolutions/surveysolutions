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
        public StatusView(int page, int pageSize,  SurveyStatus status, List<TemplateLight> headers, IEnumerable<StatusViewItem> items)
        {
            this.Page = page;
            this.PageSize = pageSize;
            this.Status = status;
            this.Items = items.ToList();

            this.BuildSummary(headers);

        }

        /// <summary>
        /// Gets or sets the headers.
        /// </summary>
        public List<TemplateLight> Headers { get; private set; }

        /// <summary>
        /// Gets or sets Template.
        /// </summary>
        public SurveyStatus Status { get; private set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        public string Order { get; private set; }

        /// <summary>
        /// Gets or sets the orders.
        /// </summary>
        public List<OrderRequestItem> Orders { get; private set; }

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
        public StatusViewItem Summary { get; private set; }

        /// <summary>
        /// Gets or sets the total count.
        /// </summary>
        public int TotalCount {
            get { return Items.Count; }
        }

        /// <summary>
        /// Gets or sets Items.
        /// </summary>
        public List<StatusViewItem> Items { get;private set; }

        private void BuildSummary(List<TemplateLight> headers)
        {
            var dict = new Dictionary<Guid, int>();
            var newHeader = new List<TemplateLight>();
            foreach (var header in headers)
            {
                if (!dict.ContainsKey(header.TemplateId))
                {
                    var totalSum = Items.Select(i => i.GetCount(header.TemplateId)).Sum();
                    dict.Add(header.TemplateId, totalSum);
                    if (totalSum > 0)
                        newHeader.Add(header);
                }
            }
            this.Headers = newHeader;
            this.Summary = new StatusViewItem(new UserLight(Guid.Empty, "Summary"), dict);
        }
    }
}

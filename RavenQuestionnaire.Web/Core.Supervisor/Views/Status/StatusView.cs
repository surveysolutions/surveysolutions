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
        public StatusView(int page, int pageSize, SurveyStatus status, List<TemplateLight> headers, IEnumerable<StatusViewItem> items)
        {
            this.Page = page;
            this.PageSize = pageSize;
            this.Status = status;
            this.Items = items.ToList();
            this.Headers = headers;
        }

        public List<TemplateLight> Headers { get; private set; }

        public SurveyStatus Status { get; private set; }

        public string Order { get; private set; }

        public List<OrderRequestItem> Orders { get; private set; }

        public int Page { get; private set; }

        public int PageSize { get; private set; }

        public int TotalCount
        {
            get { return Items.Count; }
        }

        public List<StatusViewItem> Items { get; private set; }
    }
}

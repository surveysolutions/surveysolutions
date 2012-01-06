using System.Collections.Generic;

namespace RavenQuestionnaire.Core.Views.Report
{
    public class ReportBrowseView
    {
        public int PageSize
        {
            get;
            private set;
        }

        public int Page
        {
            get;
            private set;
        }

        public int TotalCount { get; private set; }


        public IEnumerable<ReportBrowseItem> Items
        {
            get;
            private set;
        }

        public ReportBrowseView(int page, int pageSize, int totalCount, IEnumerable<ReportBrowseItem> items)
        {
            this.Page = page;
            this.TotalCount = totalCount;
            this.PageSize = pageSize;
            this.Items = items;
        }

    }
}

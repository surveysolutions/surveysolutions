using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.Interview.Views
{
    public class AllInterviewsView
    {
        public int Page { get; set; }
        public int PageSize { get; set; }

        public int TotalCount { get; set; }
        public IEnumerable<AllInterviewsViewItem> Items { get; set; }
    }
}
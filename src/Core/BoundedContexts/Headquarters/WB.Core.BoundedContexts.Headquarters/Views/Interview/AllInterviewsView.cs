using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public class AllInterviewsView : IListView<AllInterviewsViewItem>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }

        public int TotalCount { get; set; }
        public IEnumerable<AllInterviewsViewItem> Items { get; set; }
    }

    public class InterviewsWithoutPrefilledView : IListView<InterviewListItem>
    {
        public int TotalCount { get; set; }
        public IEnumerable<InterviewListItem> Items { get; set; }
    }
}
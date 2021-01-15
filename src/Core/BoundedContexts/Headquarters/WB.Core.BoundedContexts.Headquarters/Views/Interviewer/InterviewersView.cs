using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interviewer
{
    public class InterviewersView : IListView<InterviewersItem>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }

        public IEnumerable<InterviewersItem> Items { get; set; }

        public InterviewersItem ItemsSummary { get; set; }
    }
}

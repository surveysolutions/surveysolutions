using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interviewer
{
    public class InterviewersView : IListView<InterviewersItem>
    {
        public int TotalCount { get; set; }

        public IEnumerable<InterviewersItem> Items { get; set; }

        public InterviewersItem ItemsSummary { get; set; }
    }
}
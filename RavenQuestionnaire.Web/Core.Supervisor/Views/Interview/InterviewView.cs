namespace Core.Supervisor.Views.Interview
{
    using System.Collections.Generic;

    public class InterviewView : IListView<InterviewViewItem>
    {
        public int TotalCount { get; set; }
        public IEnumerable<InterviewViewItem> Items { get; set; }
        public InterviewViewItem ItemsSummary { get; set; }
    }
}
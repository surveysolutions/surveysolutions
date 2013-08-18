namespace Core.Supervisor.Views.Interview
{
    using System.Collections.Generic;

    public class AllInterviewsView : IListView<AllInterviewsViewItem>
    {
        public int TotalCount { get; set; }
        public IEnumerable<AllInterviewsViewItem> Items { get; set; }
    }
}
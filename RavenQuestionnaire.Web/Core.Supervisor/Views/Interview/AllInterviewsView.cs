namespace Core.Supervisor.Views.Interview
{
    using System.Collections.Generic;

    public class AllInterviewsView : IListView<AllInterviewsViewItem>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }

        public int TotalCount { get; set; }
        public IEnumerable<AllInterviewsViewItem> Items { get; set; }
    }
}
namespace Core.Supervisor.Views.Summary
{
    using System.Collections.Generic;

    public class SummaryView : IListView<SummaryViewItem>
    {
        public int TotalCount { get; set; }

        public IEnumerable<SummaryViewItem> Items { get; set; }

        public SummaryViewItem ItemsSummary { get; set; }
    }
}
namespace Core.Supervisor.Views.Summary
{
    using System.Collections.Generic;

    public class SummaryView : IListView<SummaryViewItem>
    {
        #region Public Properties
        
        /// <summary>
        /// Gets or sets the total count.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Gets or sets Items.
        /// </summary>
        public IEnumerable<SummaryViewItem> Items { get; set; }

        public SummaryViewItem ItemsSummary { get; set; }

        #endregion
    }
}
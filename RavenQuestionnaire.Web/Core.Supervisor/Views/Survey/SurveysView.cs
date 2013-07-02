using System.Collections.Generic;

namespace Core.Supervisor.Views.Survey
{
    public class SurveysView : IListView<SurveysViewItem>
    {
        #region Public Properties

        public int TotalCount { get; set; }

        public IEnumerable<SurveysViewItem> Items { get; set; }

        public SurveysViewItem ItemsSummary { get; set; }

        #endregion
    }
}
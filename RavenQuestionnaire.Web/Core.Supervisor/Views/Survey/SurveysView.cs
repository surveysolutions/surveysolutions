using System.Collections.Generic;

namespace Core.Supervisor.Views.Survey
{
    public class SurveysView
    {
        #region Public Properties

        public int TotalCount { get; set; }

        public IEnumerable<SurveysViewItem> Items { get; set; }

        #endregion
    }
}
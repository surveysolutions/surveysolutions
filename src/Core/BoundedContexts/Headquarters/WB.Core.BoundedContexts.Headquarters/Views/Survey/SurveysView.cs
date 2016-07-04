﻿using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.Views.Survey
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
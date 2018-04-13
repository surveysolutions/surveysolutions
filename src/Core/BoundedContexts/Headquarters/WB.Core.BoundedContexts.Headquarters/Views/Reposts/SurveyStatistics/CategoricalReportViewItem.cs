using System;
using System.Linq;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics
{
    public class CategoricalReportViewItem
    {
        public Guid Responsible { get; set; }
        public string ResponsibleName { get; set; }
        public Guid TeamLeadId { get; set; }
        public string TeamLeadName { get; set; }
        public int[] Values { get; set; }
        public long Total => Values.Aggregate(0L, (l, i) => l + i);
    }
}

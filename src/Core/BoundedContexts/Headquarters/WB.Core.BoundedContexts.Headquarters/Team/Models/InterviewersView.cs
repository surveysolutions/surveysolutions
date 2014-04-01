using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.Team.Models
{
    public class InterviewersView
    {
        public int TotalCount { get; set; }

        public IEnumerable<InterviewersItem> Items { get; set; }

        public InterviewersItem ItemsSummary { get; set; }
    }
}
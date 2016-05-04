using System.Collections.Generic;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public class DashboardInformation
    {
        public DashboardInformation()
        {
            this.CensusQuestionnaires = new List<IDashboardItem>();
            this.NewInterviews = new List<IDashboardItem>();
            this.StartedInterviews = new List<IDashboardItem>();
            this.CompletedInterviews = new List<IDashboardItem>();
            this.RejectedInterviews = new List<IDashboardItem>();
        }

        public DashboardInformation(
            IList<IDashboardItem> censusQuestionnaires, 
            IList<IDashboardItem> newInterviews,
            IList<IDashboardItem> startedInterviews,
            IList<IDashboardItem> completedInterviews, 
            IList<IDashboardItem> rejectedInterviews)
        {
            this.CensusQuestionnaires = censusQuestionnaires;
            this.NewInterviews = newInterviews;
            this.StartedInterviews = startedInterviews;
            this.CompletedInterviews = completedInterviews;
            this.RejectedInterviews = rejectedInterviews;
        }

        public IList<IDashboardItem> CensusQuestionnaires { get; private set; }
        public IList<IDashboardItem> NewInterviews { get; private set; }
        public IList<IDashboardItem> StartedInterviews { get; private set; }
        public IList<IDashboardItem> CompletedInterviews { get; private set; }
        public IList<IDashboardItem> RejectedInterviews { get; private set; }
    }
}
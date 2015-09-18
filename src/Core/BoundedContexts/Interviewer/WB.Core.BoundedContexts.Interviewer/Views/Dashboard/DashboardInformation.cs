using System.Collections.Generic;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public class DashboardInformation
    {
        public DashboardInformation()
        {
            CensusQuestionniories = new List<IDashboardItem>();
            NewInterviews = new List<IDashboardItem>();
            StartedInterviews = new List<IDashboardItem>();
            CompletedInterviews = new List<IDashboardItem>();
            RejectedInterviews = new List<IDashboardItem>();
        }

        public List<IDashboardItem> CensusQuestionniories { get; private set; }
        public List<IDashboardItem> NewInterviews { get; private set; }
        public List<IDashboardItem> StartedInterviews { get; private set; }
        public List<IDashboardItem> CompletedInterviews { get; private set; }
        public List<IDashboardItem> RejectedInterviews { get; private set; }
    }
}
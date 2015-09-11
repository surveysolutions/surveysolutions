using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public class DashboardInformation
    {
        public DashboardInformation()
        {
            NewInterviews = new List<IDashboardItem>();
            StartedInterviews = new List<IDashboardItem>();
            CompletedInterviews = new List<IDashboardItem>();
            RejectedInterviews = new List<IDashboardItem>();
        }

        public List<IDashboardItem> NewInterviews { get; private set; }
        public List<IDashboardItem> StartedInterviews { get; private set; }
        public List<IDashboardItem> CompletedInterviews { get; private set; }
        public List<IDashboardItem> RejectedInterviews { get; private set; }
    }
}
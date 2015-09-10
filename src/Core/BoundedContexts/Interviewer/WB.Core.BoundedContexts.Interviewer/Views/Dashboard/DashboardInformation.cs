using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public class DashboardInformation
    {
         public List<IDashboardItem> NewInterviews { get; set; }
         public List<IDashboardItem> StartedInterviews { get; set; }
         public List<IDashboardItem> CompletedInterviews { get; set; }
         public List<IDashboardItem> RejectedInterviews { get; set; }
    }
}
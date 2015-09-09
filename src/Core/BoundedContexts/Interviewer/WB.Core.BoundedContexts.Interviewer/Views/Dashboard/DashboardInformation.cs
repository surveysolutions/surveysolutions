using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public class DashboardInformation
    {
         public int NewInterviewsCount {get; set; }
         public int StartedInterviewsCount { get; set; }
         public int CompletedInterviewsCount { get; set; }
         public int RejectedInterviewsCount { get; set; }

         public List<IDashboardItem> DashboardItems { get; set; }
    }
}
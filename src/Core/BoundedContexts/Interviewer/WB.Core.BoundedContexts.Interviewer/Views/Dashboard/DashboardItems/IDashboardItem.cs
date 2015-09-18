using System;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems
{
    public interface IDashboardItem
    {
        event EventHandler<EventArgs> StartingLongOperation;
    }
}
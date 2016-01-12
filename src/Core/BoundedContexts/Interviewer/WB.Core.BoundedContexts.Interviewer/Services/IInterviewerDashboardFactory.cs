using System;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;

namespace WB.Core.BoundedContexts.Interviewer.Services
{
    public interface IInterviewerDashboardFactory
    {
        DashboardInformation GetInterviewerDashboard(Guid interviewerId);
    }
}
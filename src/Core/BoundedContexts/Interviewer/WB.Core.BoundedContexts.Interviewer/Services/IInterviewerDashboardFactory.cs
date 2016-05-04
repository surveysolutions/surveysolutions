using System;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;

namespace WB.Core.BoundedContexts.Interviewer.Services
{
    public interface IInterviewerDashboardFactory
    {
        DashboardInformation GetInterviewerDashboardAsync(Guid interviewerId);
    }
}
using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Interviewer.Services
{
    public interface IInterviewerDashboardFactory
    {
        IEnumerable<DashboardItemViewModel> GetDashboardItems(Guid interviewerId);
    }
}
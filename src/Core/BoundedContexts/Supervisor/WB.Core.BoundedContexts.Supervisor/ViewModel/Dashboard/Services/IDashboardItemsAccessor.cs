using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard.Services
{
    public interface IDashboardItemsAccessor
    {
        IEnumerable<IDashboardItem> TasksToBeAssigned();
        int TasksToBeAssignedCount();

        IEnumerable<IDashboardItem> WaitingForSupervisorAction();
        int WaitingForSupervisorActionCount();

        IEnumerable<IDashboardItem> Outbox();
        int OutboxCount();
        bool IsWaitingForSupervisorActionInterview(Guid interviewId);
        bool IsOutboxInterview(Guid interviewId);
        IEnumerable<IDashboardItem> GetSentToInterviewerItems();
        int SentToInterviewerCount();
    }
}

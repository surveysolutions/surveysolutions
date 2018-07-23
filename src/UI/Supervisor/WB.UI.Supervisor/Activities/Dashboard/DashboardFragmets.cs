using MvvmCross.Platforms.Android.Presenters.Attributes;
using WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard;

namespace WB.UI.Supervisor.Activities.Dashboard
{
    [MvxFragmentPresentation(typeof(ToBeAssignedItemsViewModel), Resource.Id.dashboard_content,
        ActivityHostViewModelType = typeof(DashboardViewModel))]
    public class DashboardToBeAssignedFragment : RecyclerViewFragment<ToBeAssignedItemsViewModel>
    {
    }
    
    [MvxFragmentPresentation(typeof(WaitingForSupervisorActionViewModel), Resource.Id.dashboard_content,
        ActivityHostViewModelType = typeof(DashboardViewModel))]
    public class DashboardWaitingForSupervisorActionFragment : RecyclerViewFragment<WaitingForSupervisorActionViewModel>
    {
    }

    [MvxFragmentPresentation(typeof(OutboxViewModel), Resource.Id.dashboard_content,
        ActivityHostViewModelType = typeof(DashboardViewModel))]
    public class DashboardOutboxFragment : RecyclerViewFragment<OutboxViewModel>
    {
    }

    [MvxFragmentPresentation(typeof(SentToInterviewerViewModel), Resource.Id.dashboard_content,
        ActivityHostViewModelType = typeof(DashboardViewModel))]
    public class DashboardSentFragment : RecyclerViewFragment<SentToInterviewerViewModel>
    {
    }
}

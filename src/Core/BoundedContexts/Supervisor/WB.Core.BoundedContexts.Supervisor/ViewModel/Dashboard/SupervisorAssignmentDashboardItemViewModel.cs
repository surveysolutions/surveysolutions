using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard
{
    public class SupervisorAssignmentDashboardItemViewModel : AssignmentDashboardItemViewModel
    {
        public SupervisorAssignmentDashboardItemViewModel(IServiceLocator serviceLocator) : base(serviceLocator)
        {
        }
    }
}

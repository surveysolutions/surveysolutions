using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Supervisor.Properties;
using WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard
{
    public class SentToInterviewerViewModel : RefreshingAfterSyncListViewModel
    {
        private readonly IDashboardItemsAccessor dashboardItemsAccessor;
        private readonly IInterviewViewModelFactory viewModelFactory;

        public SentToInterviewerViewModel(IDashboardItemsAccessor dashboardItemsAccessor,
            IInterviewViewModelFactory viewModelFactory)
        {
            this.dashboardItemsAccessor = dashboardItemsAccessor;
            this.viewModelFactory = viewModelFactory;
        }

        public string TabTitle => SupervisorDashboard.ToBeAssigned;

        public override GroupStatus InterviewStatus => GroupStatus.Started;

        protected override IEnumerable<IDashboardItem> GetUiItems()
        {
            var subtitle = viewModelFactory.GetNew<DashboardSubTitleViewModel>();
            subtitle.Title = SupervisorDashboard.SentToInterviewerSubtitle;

            var tasksToBeAssigned = this.dashboardItemsAccessor.GetSentToInterviewerItems();

            return subtitle.ToEnumerable().Concat(tasksToBeAssigned);
        }
    }
}

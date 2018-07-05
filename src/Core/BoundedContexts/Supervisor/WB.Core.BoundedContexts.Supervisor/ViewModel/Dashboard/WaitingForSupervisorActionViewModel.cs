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
    public class WaitingForSupervisorActionViewModel : RefreshingAfterSyncListViewModel
    {
        private readonly IDashboardItemsAccessor dashboardItemsAccessor;
        private readonly IInterviewViewModelFactory viewModelFactory;

        public override async Task Initialize()
        {
            await base.Initialize();
            await this.UpdateUiItems();
        }

        public WaitingForSupervisorActionViewModel(IDashboardItemsAccessor dashboardItemsAccessor,
            IInterviewViewModelFactory viewModelFactory)
        {
            this.dashboardItemsAccessor = dashboardItemsAccessor;
            this.viewModelFactory = viewModelFactory;
        }

        public override GroupStatus InterviewStatus => GroupStatus.Started;

        protected override IEnumerable<IDashboardItem> GetUiItems()
        {
            var subtitle = viewModelFactory.GetNew<DashboardSubTitleViewModel>();
            subtitle.Title = SupervisorDashboard.WaitingForSupervisorActionSubtitle;

            var waitingForSupervisorAction = this.dashboardItemsAccessor.WaitingForSupervisorAction();
            return  subtitle.ToEnumerable().Concat(waitingForSupervisorAction);
        }
    }
}

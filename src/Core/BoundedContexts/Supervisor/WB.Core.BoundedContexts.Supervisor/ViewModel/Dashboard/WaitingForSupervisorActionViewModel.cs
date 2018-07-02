using System.Collections.Generic;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard
{
    public class WaitingForSupervisorActionViewModel : ListViewModel
    {
        private readonly IDashboardItemsAccessor dashboardItemsAccessor;

        public override async Task Initialize()
        {
            await base.Initialize();
            await this.UpdateUiItems();
        }

        public WaitingForSupervisorActionViewModel(IDashboardItemsAccessor dashboardItemsAccessor)
        {
            this.dashboardItemsAccessor = dashboardItemsAccessor;
        }

        public override GroupStatus InterviewStatus => GroupStatus.Started;

        protected override IEnumerable<IDashboardItem> GetUiItems()
        {
            return this.dashboardItemsAccessor.WaitingForSupervisorAction();
        }
    }
}

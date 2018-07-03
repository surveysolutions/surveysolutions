using System.Collections.Generic;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard
{
    public class OutboxViewModel : RefreshingAfterSyncListViewModel
    {
        private readonly IDashboardItemsAccessor dashboardItemsAccessor;

        public override async Task Initialize()
        {
            await base.Initialize();
            await this.UpdateUiItems();
        }

        public OutboxViewModel(IDashboardItemsAccessor dashboardItemsAccessor)
        {
            this.dashboardItemsAccessor = dashboardItemsAccessor;
        }

        public override GroupStatus InterviewStatus => GroupStatus.Completed;

        protected override IEnumerable<IDashboardItem> GetUiItems()
        {
            return dashboardItemsAccessor.Outbox();
        }
    }
}

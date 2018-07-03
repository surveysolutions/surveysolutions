using System.Collections.Generic;
using System.Threading.Tasks;
using MvvmCross.Plugin.Messenger;
using WB.Core.BoundedContexts.Supervisor.Properties;
using WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard
{
    public class ToBeAssignedItemsViewModel : RefreshingAfterSyncListViewModel
    {
        private readonly IDashboardItemsAccessor dashboardItemsAccessor;

        public ToBeAssignedItemsViewModel(IDashboardItemsAccessor dashboardItemsAccessor)
        {
            this.dashboardItemsAccessor = dashboardItemsAccessor;
        }

        public override async Task Initialize()
        {
            await base.Initialize();
            await this.UpdateUiItems();
        }

        public string TabTitle => SupervisorDashboard.ToBeAssigned;

        public override GroupStatus InterviewStatus => GroupStatus.NotStarted;

        protected override IEnumerable<IDashboardItem> GetUiItems()
        {
            return this.dashboardItemsAccessor.TasksToBeAssigned();
        }
    }
}

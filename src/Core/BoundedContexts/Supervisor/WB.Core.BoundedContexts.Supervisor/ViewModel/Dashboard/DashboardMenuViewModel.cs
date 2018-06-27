using System.Threading.Tasks;
using System.Timers;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard
{
    public class DashboardMenuViewModel : MvxViewModel
    {
        private readonly IMvxNavigationService mvxNavigationService;
        private readonly IMvxMessenger messenger;
        private readonly IDashboardItemsAccessor dashboardItemsAccessor;
        private int toBeAssignedItemsCount;
        private int outboxItemsCount;
        private int waitingForDecisionCount;

        public DashboardMenuViewModel(IMvxNavigationService mvxNavigationService, 
            IMvxMessenger messenger,
            IDashboardItemsAccessor dashboardItemsAccessor
            )
        {
            this.mvxNavigationService = mvxNavigationService;
            this.messenger = messenger;
            this.dashboardItemsAccessor = dashboardItemsAccessor;
        }

        public override Task Initialize()
        {
            messenger.Subscribe<SynchronizationCompletedMsg>(msg => RefreshCounters());
            RefreshCounters();

            return Task.CompletedTask;
        }

        private void RefreshCounters()
        {
            this.ToBeAssignedItemsCount = dashboardItemsAccessor.TasksToBeAssignedCount();
            this.WaitingForDecisionCount = dashboardItemsAccessor.WaitingForSupervisorActionCount();
            this.OutboxItemsCount = dashboardItemsAccessor.OutboxCount();
        }

        public int Counter { get; set; }

        public int ToBeAssignedItemsCount
        {
            get => toBeAssignedItemsCount;
            set => SetProperty(ref toBeAssignedItemsCount, value);
        }

        public int OutboxItemsCount
        {
            get => outboxItemsCount;
            set => SetProperty(ref outboxItemsCount, value);
        }

        public int WaitingForDecisionCount
        {
            get => waitingForDecisionCount;
            set => SetProperty(ref waitingForDecisionCount, value);
        }

        public IMvxCommand ShowToBeAssignedItems => 
            new MvxAsyncCommand(async() => await mvxNavigationService.Navigate<ToBeAssignedItemsViewModel>());

        public IMvxCommand ShowWaitingForActionItems =>
            new MvxAsyncCommand(async() => await mvxNavigationService.Navigate<WaitingForSupervisorActionViewModel>());

        public IMvxCommand ShowOutboxItems =>
            new MvxAsyncCommand(async () => await mvxNavigationService.Navigate<OutboxViewModel>());
    }
}

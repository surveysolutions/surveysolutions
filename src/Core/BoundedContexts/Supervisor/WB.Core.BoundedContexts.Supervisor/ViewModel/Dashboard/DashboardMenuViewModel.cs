using System;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard
{
    public class DashboardMenuViewModel : MvxViewModel
    {
        private readonly IMvxNavigationService mvxNavigationService;
        private readonly IMvxMessenger messenger;
        private readonly IDashboardItemsAccessor dashboardItemsAccessor;
        
        private int toBeAssignedItemsCount;
        private int outboxItemsCount;
        private int sentToInterviewerCount;
        private int waitingForDecisionCount;

        protected readonly IPrincipal Principal;
        private MvxSubscriptionToken messengerSubscribtion;

        private MvxSubscriptionToken syncMessengerSubscribtion;

        public DashboardMenuViewModel(IMvxNavigationService mvxNavigationService, 
            IMvxMessenger messenger,
            IDashboardItemsAccessor dashboardItemsAccessor,
            IPrincipal principal)
        {
            this.mvxNavigationService = mvxNavigationService;
            this.messenger = messenger;
            this.dashboardItemsAccessor = dashboardItemsAccessor;

            this.Principal = principal ?? throw new ArgumentNullException(nameof(principal));
        }

        public override async Task Initialize()
        {
            await base.Initialize();
        }

        public override void ViewAppeared()
        {
            base.ViewAppeared();
            messengerSubscribtion = messenger.Subscribe<DashboardChangedMsg>(msg => RefreshCounters(), MvxReference.Strong);
            RefreshCounters();
        }

        public override void ViewDisappeared()
        {
            base.ViewDisappeared();
            messengerSubscribtion.Dispose();
        }

        private void RefreshCounters()
        {
            this.ToBeAssignedItemsCount = dashboardItemsAccessor.TasksToBeAssignedCount();
            this.WaitingForDecisionCount = dashboardItemsAccessor.WaitingForSupervisorActionCount();
            this.OutboxItemsCount = dashboardItemsAccessor.OutboxCount();
            this.SentToInterviewerCount = dashboardItemsAccessor.SentToInterviewerCount();

            this.UserName = Principal.CurrentUserIdentity.Name;
            this.UserEmail = Principal.CurrentUserIdentity.Email;
        }

        private string userName;

        public string UserName
        {
            get => userName;
            set => SetProperty(ref userName, value);
        }

        private string userEmail;

        public string UserEmail
        {
            get => userEmail;
            set => SetProperty(ref userEmail, value);
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

        public int SentToInterviewerCount
        {
            get => sentToInterviewerCount;
            set => SetProperty(ref sentToInterviewerCount, value);
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

        public IMvxCommand ShowSentItems => 
            new MvxAsyncCommand(async () => await mvxNavigationService.Navigate<SentToInterviewerViewModel>());
    }
}

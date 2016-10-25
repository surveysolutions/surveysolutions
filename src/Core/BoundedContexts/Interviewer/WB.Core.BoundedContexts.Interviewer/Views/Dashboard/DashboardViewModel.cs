using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.Messages;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;


namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public class DashboardViewModel : BaseViewModel, IDisposable
    {
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly IInterviewerDashboardFactory dashboardFactory;
        private readonly IMvxMessenger messenger;
        private readonly ICommandService commandService;

        private DashboardInformation dashboardInformation = new DashboardInformation();
        private DashboardInterviewStatus currentDashboardStatus;

        private MvxSubscriptionToken startingLongOperationMessageSubscriptionToken;
        private MvxSubscriptionToken removedDashboardItemMessageSubscriptionToken;

        public DashboardViewModel(IViewModelNavigationService viewModelNavigationService,
            IInterviewerDashboardFactory dashboardFactory,
            IPrincipal principal, 
            SynchronizationViewModel synchronization,
            IMvxMessenger messenger,
            ICommandService commandService) : base(principal, viewModelNavigationService)
        {
            this.viewModelNavigationService = viewModelNavigationService;
            this.dashboardFactory = dashboardFactory;
            this.messenger = messenger;
            this.commandService = commandService;
            this.Synchronization = synchronization;
            this.Synchronization.SyncCompleted += (sender, args) => this.RefreshDashboard();
        }

        private IMvxCommand synchronizationCommand;
        public IMvxCommand SynchronizationCommand
        {
            get
            {
                return synchronizationCommand ??
                       (synchronizationCommand = new MvxCommand(this.RunSynchronization,
                           () => !this.Synchronization.IsSynchronizationInProgress));
            }
        }
        public IMvxCommand ShowNewItemsInterviewsCommand => new MvxCommand(() => ShowInterviews(DashboardInterviewStatus.New));
        public IMvxCommand ShowStartedInterviewsCommand => new MvxCommand(() => ShowInterviews(DashboardInterviewStatus.InProgress));
        public IMvxCommand ShowCompletedInterviewsCommand => new MvxCommand(() => ShowInterviews(DashboardInterviewStatus.Completed));
        public IMvxCommand ShowRejectedInterviewsCommand => new MvxCommand(() => ShowInterviews(DashboardInterviewStatus.Rejected));
        public IMvxCommand SignOutCommand => new MvxCommand(this.SignOut);
        public IMvxCommand NavigateToDiagnosticsPageCommand => new MvxCommand(this.NavigateToDiagnostics);
            
        public bool IsNewInterviewsCategorySelected => this.CurrentDashboardStatus == DashboardInterviewStatus.New;
        public bool IsStartedInterviewsCategorySelected => this.CurrentDashboardStatus == DashboardInterviewStatus.InProgress;
        public bool IsCompletedInterviewsCategorySelected => this.CurrentDashboardStatus == DashboardInterviewStatus.Completed;
        public bool IsRejectedInterviewsCategorySelected => this.CurrentDashboardStatus == DashboardInterviewStatus.Rejected;

        public int NewInterviewsCount => this.dashboardInformation.NewInterviews.Count();
        public int StartedInterviewsCount => this.dashboardInformation.StartedInterviews.Count();
        public int CompletedInterviewsCount => this.dashboardInformation.CompletedInterviews.Count();

        public int RejectedInterviewsCount => this.dashboardInformation.RejectedInterviews.Count();

        public bool IsExistsAnyCensusQuestionniories => this.dashboardInformation.CensusQuestionnaires.Any();
        public bool IsExistsAnyNewInterview => this.NewInterviewsCount > 0;
        public bool IsExistsAnyStartedInterview => this.StartedInterviewsCount > 0;
        public bool IsExistsAnyCompletedInterview => this.CompletedInterviewsCount > 0;
        public bool IsExistsAnyRejectedInterview => this.RejectedInterviewsCount > 0;

        private bool isInProgress;
        public bool IsInProgress
        {
            get { return this.isInProgress; }
            set { this.isInProgress = value; this.RaisePropertyChanged(); }
        }
        
        private SynchronizationViewModel synchronization;
        public SynchronizationViewModel Synchronization
        {
            get { return synchronization; }
            set
            {
                this.synchronization = value;
                this.RaisePropertyChanged();
            }
        }

        private bool isLoaded;
        public bool IsLoaded
        {
            get { return this.isLoaded; }
            set { this.isLoaded = value; this.RaisePropertyChanged(); }
        }

        public string DashboardTitle
        {
            get
            {
                var numberOfAssignedInterviews = this.NewInterviewsCount
                    + this.StartedInterviewsCount
                    + this.CompletedInterviewsCount
                    + this.RejectedInterviewsCount;

                var userName = this.principal.CurrentUserIdentity.Name;
                return InterviewerUIResources.Dashboard_Title.FormatString(numberOfAssignedInterviews, userName);
            }
        }

        private IList<IDashboardItem> dashboardItems;
        public IList<IDashboardItem> DashboardItems
        {
            get { return this.dashboardItems; }
            set { this.dashboardItems = value; this.RaisePropertyChanged(); }
        }

        public DashboardInterviewStatus CurrentDashboardStatus
        {
            get { return this.currentDashboardStatus; }
            set 
            {
                this.currentDashboardStatus = value; 
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(() => IsNewInterviewsCategorySelected);
                this.RaisePropertyChanged(() => IsStartedInterviewsCategorySelected);
                this.RaisePropertyChanged(() => IsCompletedInterviewsCategorySelected);
                this.RaisePropertyChanged(() => IsRejectedInterviewsCategorySelected); 
            }
        }

        public DashboardInformation DashboardInformation
        {
            get { return this.dashboardInformation; }
            set
            {
                this.dashboardInformation = value;
                this.RaisePropertyChanged(() => IsExistsAnyCensusQuestionniories);
                this.RaisePropertyChanged(() => NewInterviewsCount);
                this.RaisePropertyChanged(() => IsExistsAnyNewInterview);
                this.RaisePropertyChanged(() => StartedInterviewsCount);
                this.RaisePropertyChanged(() => IsExistsAnyStartedInterview);
                this.RaisePropertyChanged(() => CompletedInterviewsCount);
                this.RaisePropertyChanged(() => IsExistsAnyCompletedInterview);
                this.RaisePropertyChanged(() => RejectedInterviewsCount);
                this.RaisePropertyChanged(() => IsExistsAnyRejectedInterview);
                this.RaisePropertyChanged(() => DashboardTitle);
            }
        }

        public override void Load()
        {
            startingLongOperationMessageSubscriptionToken = this.messenger.Subscribe<StartingLongOperationMessage>(this.DashboardItemOnStartingLongOperation);
            removedDashboardItemMessageSubscriptionToken = this.messenger.Subscribe<RemovedDashboardItemMessage>(DashboardItemOnRemovedDashboardItem);

            this.Synchronization.Init();
            this.RefreshDashboard();
        }

        private void RefreshDashboard()
        {
            if(this.principal.CurrentUserIdentity == null)
                return;

            this.DashboardInformation = this.dashboardFactory.GetInterviewerDashboardAsync(
                this.principal.CurrentUserIdentity.UserId);

            if ((CurrentDashboardStatus == DashboardInterviewStatus.Completed && this.CompletedInterviewsCount == 0)
                || (CurrentDashboardStatus == DashboardInterviewStatus.InProgress && this.StartedInterviewsCount == 0)
                || (CurrentDashboardStatus == DashboardInterviewStatus.Rejected && this.RejectedInterviewsCount == 0))
            {
                this.CurrentDashboardStatus = DashboardInterviewStatus.New;
            }

            this.RefreshTab();
            IsLoaded = true;
        }

        private void RefreshTab()
        {
            switch (this.CurrentDashboardStatus)
            {
                case DashboardInterviewStatus.New:
                    this.DashboardItems = dashboardInformation.CensusQuestionnaires.Union(dashboardInformation.NewInterviews).ToList();
                    break;
                case DashboardInterviewStatus.InProgress:
                    this.DashboardItems = dashboardInformation.StartedInterviews;
                    break;
                case DashboardInterviewStatus.Completed:
                    this.DashboardItems = dashboardInformation.CompletedInterviews;
                    break;
                case DashboardInterviewStatus.Rejected:
                    this.DashboardItems = dashboardInformation.RejectedInterviews;
                    break;
            }
        }

        private void RunSynchronization()
        {
            if (this.viewModelNavigationService.HasPendingOperations)
            {
                this.viewModelNavigationService.ShowWaitMessage();
                return;
            }

            this.Synchronization.IsSynchronizationInProgress = true;
            this.Synchronization.Synchronize();
        }

        private void ShowInterviews(DashboardInterviewStatus status)
        {
            if (status == this.CurrentDashboardStatus)
                return;

            this.CurrentDashboardStatus = status;

            this.RefreshTab();
        }

        private void NavigateToDiagnostics()
        {
            this.Synchronization.CancelSynchronizationCommand.Execute();
            this.viewModelNavigationService.NavigateTo<DiagnosticsViewModel>();
        }

        private void SignOut()
        {
            this.Synchronization.CancelSynchronizationCommand.Execute();
            
            this.viewModelNavigationService.SignOutAndNavigateToLogin();
        }

        private async void DashboardItemOnRemovedDashboardItem(RemovedDashboardItemMessage message)
        {
            await this.commandService.WaitPendingCommandsAsync();
            this.RefreshDashboard();
        }
        private void DashboardItemOnStartingLongOperation(StartingLongOperationMessage message)
        {
            IsInProgress = true;
        }

        public void Dispose()
        {
            messenger.Unsubscribe<StartingLongOperationMessage>(startingLongOperationMessageSubscriptionToken);
            messenger.Unsubscribe<RemovedDashboardItemMessage>(removedDashboardItemMessageSubscriptionToken);
        }
    }
}
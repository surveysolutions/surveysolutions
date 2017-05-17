using System;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.Messages;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;


namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public class DashboardViewModel : BaseViewModel, IDisposable
    {
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly IMvxMessenger messenger;

        private MvxSubscriptionToken startingLongOperationMessageSubscriptionToken;

        public DashboardQuestionnairesAndNewInterviewsViewModel QuestionnairesAndNewInterviewes { get; }
        public DashboardStartedInterviewsViewModel StartedInterviews { get; }
        public DashboardCompletedInterviewsViewModel CompletedInterviews { get; }
        public DashboardRejectedInterviewsViewModel RejectedInterviews { get; }

        public DashboardViewModel(IViewModelNavigationService viewModelNavigationService,
            IPrincipal principal, 
            SynchronizationViewModel synchronization,
            IMvxMessenger messenger,
            DashboardQuestionnairesAndNewInterviewsViewModel dashboardQuestionnairesViewModel,
            DashboardStartedInterviewsViewModel dashboardStartedInterviewsViewModel,
            DashboardCompletedInterviewsViewModel dashboardCompletedInterviewsViewModel,
            DashboardRejectedInterviewsViewModel dashboardRejectedInterviewsViewModel) : base(principal, viewModelNavigationService)
        {
            this.viewModelNavigationService = viewModelNavigationService;
            this.messenger = messenger;
            this.Synchronization = synchronization;
            this.Synchronization.SyncCompleted += (sender, args) => this.RefreshDashboard();

            this.QuestionnairesAndNewInterviewes = dashboardQuestionnairesViewModel;
            this.StartedInterviews = dashboardStartedInterviewsViewModel;
            this.CompletedInterviews = dashboardCompletedInterviewsViewModel;
            this.RejectedInterviews = dashboardRejectedInterviewsViewModel;
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
        public IMvxCommand SignOutCommand => new MvxCommand(this.SignOut);
        public IMvxCommand NavigateToDiagnosticsPageCommand => new MvxCommand(this.NavigateToDiagnostics);
        
        private bool isInProgress;
        public bool IsInProgress
        {
            get { return this.isInProgress; }
            set { this.isInProgress = value; this.RaisePropertyChanged(); }
        }

        private int numberOfAssignedInterviews => this.QuestionnairesAndNewInterviewes.NewInterviewsCount
                                                  + this.StartedInterviews.Items.Count
                                                  + this.CompletedInterviews.Items.Count
                                                  + this.RejectedInterviews.Items.Count;

        public bool IsExistsAnyCensusQuestionnairesOrInterviews
            => numberOfAssignedInterviews + this.QuestionnairesAndNewInterviewes.CensusQuestionnairesCount > 0;

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
            => InterviewerUIResources.Dashboard_Title.FormatString(this.numberOfAssignedInterviews,
                this.principal.CurrentUserIdentity.Name);

        public override void Load()
        {
            startingLongOperationMessageSubscriptionToken = this.messenger.Subscribe<StartingLongOperationMessage>(this.DashboardItemOnStartingLongOperation);

            this.Synchronization.Init();
            this.StartedInterviews.OnInterviewRemoved += this.OnInterviewRemoved;
            this.CompletedInterviews.OnInterviewRemoved += this.OnInterviewRemoved;
            this.RefreshDashboard();
        }

        private void OnInterviewRemoved(object sender, EventArgs e)
            => this.RaisePropertyChanged(() => this.DashboardTitle);

        private void RefreshDashboard()
        {
            if(this.principal.CurrentUserIdentity == null)
                return;

            this.QuestionnairesAndNewInterviewes.Load();
            this.StartedInterviews.Load();
            this.CompletedInterviews.Load();
            this.RejectedInterviews.Load();
            
            this.RaisePropertyChanged(() => this.DashboardTitle);
            this.RaisePropertyChanged(() => this.IsExistsAnyCensusQuestionnairesOrInterviews);

            IsLoaded = true;
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

        private void DashboardItemOnStartingLongOperation(StartingLongOperationMessage message)
        {
            IsInProgress = true;
        }

        public void Dispose()
        {
            messenger.Unsubscribe<StartingLongOperationMessage>(startingLongOperationMessageSubscriptionToken);
            this.StartedInterviews.OnInterviewRemoved -= this.OnInterviewRemoved;
            this.CompletedInterviews.OnInterviewRemoved -= this.OnInterviewRemoved;
        }
    }
}
using System;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.Messages;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public class DashboardViewModel : MvxViewModel<DashboardArgs>, IDisposable
    {
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly IPrincipal principal;
        private readonly IMvxMessenger messenger;

        private MvxSubscriptionToken startingLongOperationMessageSubscriptionToken;
        private MvxSubscriptionToken stopLongOperationMessageSubscriptionToken;

        public CreateNewViewModel CreateNew { get; }
        public StartedInterviewsViewModel StartedInterviews { get; }
        public CompletedInterviewsViewModel CompletedInterviews { get; }
        public RejectedInterviewsViewModel RejectedInterviews { get; }

        public DashboardViewModel(IViewModelNavigationService viewModelNavigationService,
            IPrincipal principal,
            SynchronizationViewModel synchronization,
            IMvxMessenger messenger,
            CreateNewViewModel createNewViewModel,
            StartedInterviewsViewModel startedInterviewsViewModel,
            CompletedInterviewsViewModel completedInterviewsViewModel,
            RejectedInterviewsViewModel rejectedInterviewsViewModel)
        {
            this.viewModelNavigationService = viewModelNavigationService;
            this.principal = principal;
            this.messenger = messenger;
            this.Synchronization = synchronization;
            this.Synchronization.SyncCompleted += async (sender, args) => await this.RefreshDashboard();

            this.CreateNew = createNewViewModel;
            this.StartedInterviews = startedInterviewsViewModel;
            this.CompletedInterviews = completedInterviewsViewModel;
            this.RejectedInterviews = rejectedInterviewsViewModel;
        }

        public override async Task Initialize(DashboardArgs parameter)
        {
            startingLongOperationMessageSubscriptionToken = this.messenger.Subscribe<StartingLongOperationMessage>(this.DashboardItemOnStartingLongOperation);
            stopLongOperationMessageSubscriptionToken = this.messenger.Subscribe<StopingLongOperationMessage>(this.DashboardItemOnStopLongOperation);
            this.Synchronization.Init();
            this.StartedInterviews.OnInterviewRemoved += this.OnInterviewRemoved;
            this.CompletedInterviews.OnInterviewRemoved += this.OnInterviewRemoved;

            await this.RefreshDashboard();
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
            get => this.isInProgress;
            set
            {
                if (this.isInProgress != value)
                {
                    this.isInProgress = value;
                    RaisePropertyChanged();
                }
            }
        }

        private GroupStatus typeOfInterviews;
        public GroupStatus TypeOfInterviews
        {
            get => this.typeOfInterviews;
            set
            {
                if (this.typeOfInterviews != value)
                {
                    this.typeOfInterviews = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int NumberOfAssignedInterviews => this.StartedInterviews.ItemsCount
                                                  + this.CompletedInterviews.ItemsCount
                                                  + this.RejectedInterviews.ItemsCount;

        public SynchronizationViewModel Synchronization { get; set; }

        private bool isLoaded;
        public bool IsLoaded
        {
            get => this.isLoaded;
            set
            {
                if (this.isLoaded != value)
                {
                    this.isLoaded = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string DashboardTitle
            => InterviewerUIResources.Dashboard_Title.FormatString(this.NumberOfAssignedInterviews.ToString(),
                this.principal.CurrentUserIdentity.Name);

        private void OnInterviewRemoved(object sender, InterviewRemovedArgs e)
        {
            this.RaisePropertyChanged(() => this.DashboardTitle);
            this.CreateNew.UpdateAssignment(e.AssignmentId);
        }

        private async Task RefreshDashboard()
        {
            if (this.principal.CurrentUserIdentity == null)
                return;

            this.IsInProgress = true;

            await this.CreateNew.Load(this.Synchronization);
            await this.StartedInterviews.Load();
            await this.RejectedInterviews.Load();
            await this.CompletedInterviews.Load();

            this.RaisePropertyChanged(() => this.DashboardTitle);

            this.IsInProgress = false;
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

        private void DashboardItemOnStopLongOperation(StopingLongOperationMessage message)
        {
            IsInProgress = false;
        }

        public void Dispose()
        {
            messenger.Unsubscribe<StartingLongOperationMessage>(startingLongOperationMessageSubscriptionToken);
            messenger.Unsubscribe<StopingLongOperationMessage>(stopLongOperationMessageSubscriptionToken);
            this.StartedInterviews.OnInterviewRemoved -= this.OnInterviewRemoved;
            this.CompletedInterviews.OnInterviewRemoved -= this.OnInterviewRemoved;
        }
    }

    public class DashboardArgs
    {
    }
}
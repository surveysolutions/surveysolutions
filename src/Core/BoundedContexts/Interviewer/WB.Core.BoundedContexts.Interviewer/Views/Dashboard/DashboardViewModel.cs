using System;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using Ncqrs.Domain;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.Messages;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;


namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public class DashboardViewModel : BaseViewModel, IDisposable
    {
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly IMvxMessenger messenger;

        private MvxSubscriptionToken startingLongOperationMessageSubscriptionToken;
        
        public CreateNewViewModel CreateNew { get; }
        public StartedInterviewsViewModel StartedInterviews { get; }
        public CompletedInterviewsViewModel CompletedInterviews { get; }
        public RejectedInterviewsViewModel RejectedInterviews { get; }

        public event EventHandler InterviewsCountChanged;

        public DashboardViewModel(IViewModelNavigationService viewModelNavigationService,
            IPrincipal principal, 
            SynchronizationViewModel synchronization,
            IMvxMessenger messenger,
            CreateNewViewModel createNewViewModel,
            StartedInterviewsViewModel startedInterviewsViewModel,
            CompletedInterviewsViewModel completedInterviewsViewModel,
            RejectedInterviewsViewModel rejectedInterviewsViewModel) : base(principal, viewModelNavigationService)
        {
            this.viewModelNavigationService = viewModelNavigationService;
            this.messenger = messenger;
            this.Synchronization = synchronization;
            this.Synchronization.SyncCompleted += async (sender, args) => await this.RefreshDashboardAsync();

            this.CreateNew = createNewViewModel;
            this.StartedInterviews = startedInterviewsViewModel;
            this.CompletedInterviews = completedInterviewsViewModel;
            this.RejectedInterviews = rejectedInterviewsViewModel;
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

        private GroupStatus typeOfInterviews;
        public GroupStatus TypeOfInterviews
        {
            get { return this.typeOfInterviews; }
            set { this.typeOfInterviews = value; this.RaisePropertyChanged(); }
        }

        private int numberOfAssignedInterviews => this.StartedInterviews.Items.Count
                                                  + this.CompletedInterviews.Items.Count
                                                  + this.RejectedInterviews.Items.Count;

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
        
        public override async void Load()
        {
            startingLongOperationMessageSubscriptionToken = this.messenger.Subscribe<StartingLongOperationMessage>(this.DashboardItemOnStartingLongOperation);

            this.Synchronization.Init();
            this.StartedInterviews.OnInterviewRemoved += this.OnInterviewRemoved;
            this.CompletedInterviews.OnInterviewRemoved += this.OnInterviewRemoved;

            await this.RefreshDashboardAsync();
        }

        private void OnInterviewRemoved(object sender, EventArgs e)
        {
            this.RaisePropertyChanged(() => this.DashboardTitle);
            this.OnInterviewsCountChanged();
        }

        private async Task RefreshDashboardAsync()
        {
            if(this.principal.CurrentUserIdentity == null)
                return;

            await this.CreateNew.LoadAsync(this.Synchronization, this);
            await this.StartedInterviews.LoadAsync();
            await this.RejectedInterviews.LoadAsync();
            await this.CompletedInterviews.LoadAsync();
            
            this.RaisePropertyChanged(() => this.DashboardTitle);

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

        protected virtual void OnInterviewsCountChanged()
        {
            this.InterviewsCountChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
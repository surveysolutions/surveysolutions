using System;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.Messages;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public class DashboardViewModel : BaseViewModel<DashboardViewModelArgs>, IDisposable
    {
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly IInterviewerSettings interviewerSettings;
        private readonly IPlainStorage<InterviewView> interviewsRepository;
        private readonly IAuditLogService auditLogService;

        private readonly MvxSubscriptionToken startingLongOperationMessageSubscriptionToken;
        private readonly MvxSubscriptionToken stopLongOperationMessageSubscriptionToken;

        public CreateNewViewModel CreateNew { get; }
        public StartedInterviewsViewModel StartedInterviews { get; }
        public CompletedInterviewsViewModel CompletedInterviews { get; }
        public RejectedInterviewsViewModel RejectedInterviews { get; }

        public DashboardViewModel(IViewModelNavigationService viewModelNavigationService,
            IPrincipal principal,
            SynchronizationViewModel synchronization,
            IMvxMessenger messenger,
            IInterviewerSettings interviewerSettings,
            CreateNewViewModel createNewViewModel,
            StartedInterviewsViewModel startedInterviewsViewModel,
            CompletedInterviewsViewModel completedInterviewsViewModel,
            RejectedInterviewsViewModel rejectedInterviewsViewModel,
            IPlainStorage<InterviewView> interviewsRepository,
            IAuditLogService auditLogService): base (principal, viewModelNavigationService)
        {
            this.viewModelNavigationService = viewModelNavigationService;
            this.interviewerSettings = interviewerSettings;
            this.interviewsRepository = interviewsRepository;
            this.auditLogService = auditLogService;
            this.Synchronization = synchronization;
            this.Synchronization.SyncCompleted += this.Refresh;

            this.CreateNew = createNewViewModel;
            this.StartedInterviews = startedInterviewsViewModel;
            this.CompletedInterviews = completedInterviewsViewModel;
            this.RejectedInterviews = rejectedInterviewsViewModel;

            startingLongOperationMessageSubscriptionToken = messenger.Subscribe<StartingLongOperationMessage>(this.DashboardItemOnStartingLongOperation);
            stopLongOperationMessageSubscriptionToken = messenger.Subscribe<StopingLongOperationMessage>(this.DashboardItemOnStopLongOperation);
            this.Synchronization.Init();
            this.StartedInterviews.OnInterviewRemoved += this.OnInterviewRemoved;
            this.CompletedInterviews.OnInterviewRemoved += this.OnInterviewRemoved;
            this.StartedInterviews.OnItemsLoaded += this.OnItemsLoaded;
            this.RejectedInterviews.OnItemsLoaded += this.OnItemsLoaded;
            this.CompletedInterviews.OnItemsLoaded += this.OnItemsLoaded;
            this.CreateNew.OnItemsLoaded += this.OnItemsLoaded;
        }

        public override void Prepare(DashboardViewModelArgs parameter)
        {
            this.LastVisitedInterviewId = parameter.InterviewId;
        }

        public override Task Initialize()
        {
            this.RefreshDashboard(this.LastVisitedInterviewId);
            this.SelectTypeOfInterviewsByInterviewId(this.LastVisitedInterviewId);
            return Task.CompletedTask;
        }

        public override void ViewAppeared()
        {
            base.ViewAppeared();
            this.SynchronizationWithHqEnabled = this.interviewerSettings.AllowSyncWithHq;
        }

        public bool SynchronizationWithHqEnabled
        {
            get => synchronizationWithHqEnabled;
            private set => SetProperty(ref synchronizationWithHqEnabled, value);
        }

        private Guid? LastVisitedInterviewId { set; get; }

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

        public IMvxCommand SignOutCommand => new MvxAsyncCommand(this.SignOut);

        public IMvxCommand NavigateToDiagnosticsPageCommand => new MvxAsyncCommand(this.NavigateToDiagnostics);

        public IMvxCommand NavigateToMapsCommand => new MvxAsyncCommand(this.NavigateToMaps);
        public IMvxCommand NavigateToOfflineSyncCommand => new MvxAsyncCommand(this.NavigateToOfflineSync);
        private Task NavigateToOfflineSync()
        {
            this.Synchronization.CancelSynchronizationCommand.Execute();
            return this.viewModelNavigationService.NavigateToAsync<OfflineInterviewerSyncViewModel>();
        }

        private Task NavigateToMaps()
        {
            this.Synchronization.CancelSynchronizationCommand.Execute();
            return this.viewModelNavigationService.NavigateToAsync<MapsViewModel>();
        }
        
        private bool isInProgress;
        public bool IsInProgress
        {
            get => this.isInProgress;
            set => SetProperty(ref this.isInProgress, value);
        }

        private GroupStatus typeOfInterviews;
        private bool synchronizationWithHqEnabled;

        public GroupStatus TypeOfInterviews
        {
            get => this.typeOfInterviews;
            set => SetProperty(ref this.typeOfInterviews, value);
        }

        private int NumberOfAssignedInterviews => this.StartedInterviews.ItemsCount
                                                  + this.CompletedInterviews.ItemsCount
                                                  + this.RejectedInterviews.ItemsCount;

        public SynchronizationViewModel Synchronization { get; set; }

        public string DashboardTitle
            => InterviewerUIResources.Dashboard_Title.FormatString(this.NumberOfAssignedInterviews.ToString(),
                Principal.CurrentUserIdentity.Name);

        private void OnInterviewRemoved(object sender, InterviewRemovedArgs e)
        {
            this.RaisePropertyChanged(() => this.DashboardTitle);
            this.CreateNew.UpdateAssignment(e.AssignmentId);
        }

        private void OnItemsLoaded(object sender, EventArgs e) =>
            this.IsInProgress = !(this.StartedInterviews.IsItemsLoaded && this.RejectedInterviews.IsItemsLoaded &&
                                  this.CompletedInterviews.IsItemsLoaded && this.CreateNew.IsItemsLoaded);

        private void Refresh(object sender, EventArgs e)
        {
            this.RefreshDashboard();
        }

        private void RefreshDashboard(Guid? lastVisitedInterviewId = null)
        {
            this.IsInProgress = true;

            this.CreateNew.Load(this.Synchronization);
            this.StartedInterviews.Load(lastVisitedInterviewId);
            this.RejectedInterviews.Load(lastVisitedInterviewId);
            this.CompletedInterviews.Load(lastVisitedInterviewId);

            this.RaisePropertyChanged(() => this.DashboardTitle);
        }

        private void SelectTypeOfInterviewsByInterviewId(Guid? lastVisitedInterviewId)
        {
            if (!lastVisitedInterviewId.HasValue)
                this.TypeOfInterviews = this.CreateNew.InterviewStatus;

            var interviewView = this.interviewsRepository.GetById(lastVisitedInterviewId.FormatGuid());
            if (interviewView == null) return;

            if (interviewView.Status == InterviewStatus.RejectedBySupervisor)
                this.TypeOfInterviews = this.RejectedInterviews.InterviewStatus;

            if (interviewView.Status == InterviewStatus.Completed)
                this.TypeOfInterviews = this.CompletedInterviews.InterviewStatus;

            if (interviewView.Status == InterviewStatus.InterviewerAssigned ||
                interviewView.Status == InterviewStatus.Restarted)
                this.TypeOfInterviews = this.StartedInterviews.InterviewStatus;
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

        private Task NavigateToDiagnostics()
        {
            this.Synchronization.CancelSynchronizationCommand.Execute();
            return this.viewModelNavigationService.NavigateToAsync<DiagnosticsViewModel>();
        }

        private Task SignOut()
        {
            this.Synchronization.CancelSynchronizationCommand.Execute();
            var userName = this.Principal.CurrentUserIdentity.Name;
            this.auditLogService.Write(new LogoutAuditLogEntity(userName));
            return this.viewModelNavigationService.SignOutAndNavigateToLoginAsync();
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
            startingLongOperationMessageSubscriptionToken.Dispose();
            stopLongOperationMessageSubscriptionToken.Dispose();

            this.Synchronization.SyncCompleted -= this.Refresh;

            this.StartedInterviews.OnInterviewRemoved -= this.OnInterviewRemoved;
            this.CompletedInterviews.OnInterviewRemoved -= this.OnInterviewRemoved;
            this.StartedInterviews.OnItemsLoaded -= this.OnItemsLoaded;
            this.RejectedInterviews.OnItemsLoaded -= this.OnItemsLoaded;
            this.CompletedInterviews.OnItemsLoaded -= this.OnItemsLoaded;
            this.CreateNew.OnItemsLoaded -= this.OnItemsLoaded;
        }

        protected override void InitFromBundle(IMvxBundle parameters)
        {
            base.InitFromBundle(parameters);
            this.LoadFromBundle(parameters);
        }

        protected override void ReloadFromBundle(IMvxBundle parameters)
        {
            base.ReloadFromBundle(parameters);
            this.LoadFromBundle(parameters);
        }

        private void LoadFromBundle(IMvxBundle parameters)
        {
            if (!parameters.Data.ContainsKey(nameof(LastVisitedInterviewId)) || parameters.Data[nameof(LastVisitedInterviewId)] == null) return;

            if (Guid.TryParse(parameters.Data[nameof(LastVisitedInterviewId)], out var parsedLastVisitedId))
                this.LastVisitedInterviewId = parsedLastVisitedId;
        }

        protected override void SaveStateToBundle(IMvxBundle bundle)
        {
            base.SaveStateToBundle(bundle);
            if (this.LastVisitedInterviewId != null)
            {
                bundle.Data[nameof(LastVisitedInterviewId)] = this.LastVisitedInterviewId.ToString();
            }
        }

        public IMvxAsyncCommand ShowSearchCommand => new MvxAsyncCommand(viewModelNavigationService.NavigateToAsync<DashboardSearchViewModel>);
    }

    public class DashboardViewModelArgs
    {
        public Guid InterviewId { get; set; }
    }
}

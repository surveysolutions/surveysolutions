using System;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform;
using MvvmCross.Plugins.Messenger;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.Messages;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public class DashboardViewModel : BaseViewModel<DashboardViewModelArgs>, IDisposable
    {
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly IPlainStorage<InterviewView> interviewsRepository;

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
            CreateNewViewModel createNewViewModel,
            StartedInterviewsViewModel startedInterviewsViewModel,
            CompletedInterviewsViewModel completedInterviewsViewModel,
            RejectedInterviewsViewModel rejectedInterviewsViewModel,
            IPlainStorage<InterviewView> interviewsRepository): base (principal, viewModelNavigationService)
        {
            this.viewModelNavigationService = viewModelNavigationService;
            this.interviewsRepository = interviewsRepository;
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

        public IMvxCommand SignOutCommand => new MvxCommand(this.SignOut);

        public IMvxCommand NavigateToDiagnosticsPageCommand => new MvxCommand(this.NavigateToDiagnostics);

        public IMvxCommand NavigateToMapsCommand => new MvxCommand(this.NavigateToMaps);

        private void NavigateToMaps()
        {
            this.Synchronization.CancelSynchronizationCommand.Execute();
            this.viewModelNavigationService.NavigateTo<MapsViewModel>();
        }

        private bool isInProgress;
        public bool IsInProgress
        {
            get => this.isInProgress;
            set => SetProperty(ref this.isInProgress, value);
        }

        private GroupStatus typeOfInterviews;
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

        public IMvxCommand ShowSearchCommand => new MvxCommand(() => viewModelNavigationService.NavigateTo<DashboardSearchViewModel>());
    }

    public class DashboardViewModelArgs
    {
        public Guid InterviewId { get; set; }
    }
}

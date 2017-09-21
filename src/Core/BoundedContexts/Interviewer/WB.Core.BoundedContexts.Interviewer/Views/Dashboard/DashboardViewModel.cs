using System;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.Messages;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public class DashboardViewModel : MvxViewModel<DashboardArgs>, IDisposable
    {
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly IPrincipal principal;
        private readonly IMvxMessenger messenger;
        private readonly IPlainStorage<InterviewView> interviewsRepository;

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
            RejectedInterviewsViewModel rejectedInterviewsViewModel,
            IPlainStorage<InterviewView> interviewsRepository)
        {
            this.viewModelNavigationService = viewModelNavigationService;
            this.principal = principal;
            this.messenger = messenger;
            this.interviewsRepository = interviewsRepository;
            this.Synchronization = synchronization;
            this.Synchronization.SyncCompleted += (sender, args) => this.RefreshDashboard();

            this.CreateNew = createNewViewModel;
            this.StartedInterviews = startedInterviewsViewModel;
            this.CompletedInterviews = completedInterviewsViewModel;
            this.RejectedInterviews = rejectedInterviewsViewModel;
        }

        public override Task Initialize(DashboardArgs parameter)
        {
            startingLongOperationMessageSubscriptionToken = this.messenger.Subscribe<StartingLongOperationMessage>(this.DashboardItemOnStartingLongOperation);
            stopLongOperationMessageSubscriptionToken = this.messenger.Subscribe<StopingLongOperationMessage>(this.DashboardItemOnStopLongOperation);
            this.Synchronization.Init();
            this.StartedInterviews.OnInterviewRemoved += this.OnInterviewRemoved;
            this.CompletedInterviews.OnInterviewRemoved += this.OnInterviewRemoved;
            this.StartedInterviews.OnItemsLoaded += this.OnItemsLoaded;
            this.RejectedInterviews.OnItemsLoaded += this.OnItemsLoaded;
            this.CompletedInterviews.OnItemsLoaded += this.OnItemsLoaded;
            this.CreateNew.OnItemsLoaded += this.OnItemsLoaded;

            this.RefreshDashboard(parameter.LastVisitedInterviewId);
            this.SelectTypeOfInterviewsByInterviewId(parameter.LastVisitedInterviewId);

            return Task.CompletedTask;
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

        public string DashboardTitle
            => InterviewerUIResources.Dashboard_Title.FormatString(this.NumberOfAssignedInterviews.ToString(),
                this.principal.CurrentUserIdentity.Name);

        private void OnInterviewRemoved(object sender, InterviewRemovedArgs e)
        {
            this.RaisePropertyChanged(() => this.DashboardTitle);
            this.CreateNew.UpdateAssignment(e.AssignmentId);
        }

        private void OnItemsLoaded(object sender, EventArgs e) =>
            this.IsInProgress = !(this.StartedInterviews.IsItemsLoaded && this.RejectedInterviews.IsItemsLoaded &&
                                  this.CompletedInterviews.IsItemsLoaded && this.CreateNew.IsItemsLoaded);

        private void RefreshDashboard(Guid? lastVisitedInterviewId = null)
        {
            if (this.principal.CurrentUserIdentity == null)
                return;

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
            messenger.Unsubscribe<StartingLongOperationMessage>(startingLongOperationMessageSubscriptionToken);
            messenger.Unsubscribe<StopingLongOperationMessage>(stopLongOperationMessageSubscriptionToken);
            this.StartedInterviews.OnInterviewRemoved -= this.OnInterviewRemoved;
            this.CompletedInterviews.OnInterviewRemoved -= this.OnInterviewRemoved;
            this.StartedInterviews.OnItemsLoaded -= this.OnItemsLoaded;
            this.RejectedInterviews.OnItemsLoaded -= this.OnItemsLoaded;
            this.CompletedInterviews.OnItemsLoaded -= this.OnItemsLoaded;
            this.CreateNew.OnItemsLoaded -= this.OnItemsLoaded;
        }
    }

    public class DashboardArgs
    {
        public Guid? LastVisitedInterviewId { get; set; }
    }
}
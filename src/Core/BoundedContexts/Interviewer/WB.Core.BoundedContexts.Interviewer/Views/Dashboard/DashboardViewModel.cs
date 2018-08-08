using System;
using System.Threading;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.Messages;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Entities;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.OfflineSync.ViewModels;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;
using WB.Core.SharedKernels.Enumerator.Views;
using InterviewerUIResources = WB.Core.BoundedContexts.Interviewer.Properties.InterviewerUIResources;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public class DashboardViewModel : BaseOfflineSyncViewModel<DashboardViewModelArgs>
    {
        private readonly IInterviewerPrincipal principal;

        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly IInterviewerSettings interviewerSettings;
        private readonly IPlainStorage<InterviewView> interviewsRepository;
        private readonly IAuditLogService auditLogService;
        private readonly ISynchronizationMode synchronizationMode;
        private readonly IOfflineSyncClient syncClient;
        private readonly IMvxMessenger messenger;

        private MvxSubscriptionToken startingLongOperationMessageSubscriptionToken;
        private MvxSubscriptionToken stopLongOperationMessageSubscriptionToken;

        public CreateNewViewModel CreateNew { get; }
        public StartedInterviewsViewModel StartedInterviews { get; }
        public CompletedInterviewsViewModel CompletedInterviews { get; }
        public RejectedInterviewsViewModel RejectedInterviews { get; }

        public DashboardViewModel(IViewModelNavigationService viewModelNavigationService,
            IInterviewerPrincipal principal,
            LocalSynchronizationViewModel synchronization,
            IMvxMessenger messenger,
            IInterviewerSettings interviewerSettings,
            CreateNewViewModel createNewViewModel,
            StartedInterviewsViewModel startedInterviewsViewModel,
            CompletedInterviewsViewModel completedInterviewsViewModel,
            RejectedInterviewsViewModel rejectedInterviewsViewModel,
            IPlainStorage<InterviewView> interviewsRepository,
            IAuditLogService auditLogService,
            ISynchronizationCompleteSource synchronizationCompleteSource,
            IPermissionsService permissionsService,
            INearbyConnection nearbyConnection,
            IRestService restService,
            ISynchronizationMode synchronizationMode,
            IOfflineSyncClient syncClient) : base(principal, viewModelNavigationService, permissionsService,
            nearbyConnection, interviewerSettings, restService)
        {
            this.messenger = messenger;
            this.viewModelNavigationService = viewModelNavigationService;
            this.principal = principal;
            this.interviewerSettings = interviewerSettings;
            this.interviewsRepository = interviewsRepository;
            this.auditLogService = auditLogService;
            this.synchronizationMode = synchronizationMode;
            this.syncClient = syncClient;
            this.Synchronization = synchronization;
            this.syncSubscription = synchronizationCompleteSource.SynchronizationEvents.Subscribe(r =>
            {
                this.RefreshDashboard();
            });

            this.CreateNew = createNewViewModel;
            this.StartedInterviews = startedInterviewsViewModel;
            this.CompletedInterviews = completedInterviewsViewModel;
            this.RejectedInterviews = rejectedInterviewsViewModel;

            startingLongOperationMessageSubscriptionToken =
                messenger.Subscribe<StartingLongOperationMessage>(this.DashboardItemOnStartingLongOperation);
            stopLongOperationMessageSubscriptionToken =
                messenger.Subscribe<StopingLongOperationMessage>(this.DashboardItemOnStopLongOperation);
            this.Synchronization.Init();
            this.Synchronization.OnCancel += Synchronization_OnCancel;
            this.Synchronization.OnProgressChanged += Synchronization_OnProgressChanged;
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

        public override async Task Initialize()
        {
            await base.Initialize().ConfigureAwait(false);
            this.RefreshDashboard(this.LastVisitedInterviewId);
            this.SelectTypeOfInterviewsByInterviewId(this.LastVisitedInterviewId);
        }

        public override void ViewAppeared()
        {
            base.ViewAppeared();

            startingLongOperationMessageSubscriptionToken =
                messenger.Subscribe<StartingLongOperationMessage>(this.DashboardItemOnStartingLongOperation);
            stopLongOperationMessageSubscriptionToken =
                messenger.Subscribe<StopingLongOperationMessage>(this.DashboardItemOnStopLongOperation);
            
            this.SynchronizationWithHqEnabled = this.interviewerSettings.AllowSyncWithHq;
        }

        public override void ViewDisappeared()
        {
            startingLongOperationMessageSubscriptionToken.Dispose();
            stopLongOperationMessageSubscriptionToken.Dispose();

            base.ViewDisappeared();
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
        private readonly IDisposable syncSubscription;

        public GroupStatus TypeOfInterviews
        {
            get => this.typeOfInterviews;
            set => SetProperty(ref this.typeOfInterviews, value);
        }

        private int NumberOfAssignedInterviews => this.StartedInterviews.ItemsCount
                                                  + this.CompletedInterviews.ItemsCount
                                                  + this.RejectedInterviews.ItemsCount;

        public LocalSynchronizationViewModel Synchronization { get; set; }

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
            {
                this.TypeOfInterviews = this.CreateNew.InterviewStatus;
                return;
            }

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

            this.synchronizationMode.Set(SynchronizationWithHqEnabled 
                ? SynchronizationMode.Online 
                : SynchronizationMode.Offline);

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

        public override void Dispose()
        {
            base.Dispose();

            startingLongOperationMessageSubscriptionToken.Dispose();
            stopLongOperationMessageSubscriptionToken.Dispose();

            syncSubscription.Dispose();

            this.StartedInterviews.OnInterviewRemoved -= this.OnInterviewRemoved;
            this.CompletedInterviews.OnInterviewRemoved -= this.OnInterviewRemoved;
            this.StartedInterviews.OnItemsLoaded -= this.OnItemsLoaded;
            this.RejectedInterviews.OnItemsLoaded -= this.OnItemsLoaded;
            this.CompletedInterviews.OnItemsLoaded -= this.OnItemsLoaded;
            this.CreateNew.OnItemsLoaded -= this.OnItemsLoaded;
            this.Synchronization.OnCancel -= this.Synchronization_OnCancel;
            this.Synchronization.OnProgressChanged -= this.Synchronization_OnProgressChanged;
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
            if (!parameters.Data.ContainsKey(nameof(LastVisitedInterviewId)) ||
                parameters.Data[nameof(LastVisitedInterviewId)] == null) return;

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

        public IMvxAsyncCommand ShowSearchCommand =>
            new MvxAsyncCommand(viewModelNavigationService.NavigateToAsync<SearchViewModel>);

        #region Offline synchronization

        public IMvxCommand StartOfflineSyncCommand => new MvxCommand(this.StartOfflineSynchronization);
        public event EventHandler OnOfflineSynchonizationStarted;

        private void StartOfflineSynchronization()
        {
            this.Synchronization.CancelSynchronizationCommand.Execute();
            
            this.cancellationTokenSource = new CancellationTokenSource();

            this.Synchronization.Status = SynchronizationStatus.Started;
            this.Synchronization.ProcessOperation = InterviewerUIResources.SendToSupervisor_LookingForSupervisor;
            this.Synchronization.ProcessOperationDescription = InterviewerUIResources.SendToSupervisor_CheckSupervisorDevice;

            this.Synchronization.IsSynchronizationInProgress = true;
            this.Synchronization.IsSynchronizationInfoShowed = true;

            this.OnOfflineSynchonizationStarted?.Invoke(this, EventArgs.Empty);
        }

        protected override async Task OnStartDiscovery()
        {
            var discoveryStatus = await this.nearbyConnection.StartDiscoveryAsync(this.GetServiceName(), cancellationTokenSource.Token);
            if (!discoveryStatus.IsSuccess)
                this.OnConnectionError(discoveryStatus.StatusMessage, discoveryStatus.Status);
        }

        protected override void OnConnectionError(string errorMessage, ConnectionStatusCode errorCode)
        {
            this.StopDiscovery();

            switch (errorCode)
            {
                case ConnectionStatusCode.StatusBluetoothError:
                    this.ShowSynchronizationError(InterviewerUIResources.SendToSupervisor_BluetoothError);
                    break;
                case ConnectionStatusCode.MissingPermissionAccessCoarseLocation:
                case ConnectionStatusCode.StatusEndpointUnknown:
                    this.ShowSynchronizationError(errorMessage);
                    break;
                default:
                    this.ShowSynchronizationError(InterviewerUIResources.SendToSupervisor_SupervisorNotFound);
                    break;
            }
        }

        protected override void OnDeviceConnected(string name)
        {
            this.StopDiscovery();

            using (new CommunicationSession())
            {
                this.synchronizationMode.Set(SynchronizationMode.Offline);

                this.RunSynchronization();
            }
        }

        protected override void OnDeviceDisconnected(string name)
        {
            if (this.Synchronization.Status != SynchronizationStatus.Success)
            {
                this.ShowSynchronizationError(InterviewerUIResources.SendToSupervisor_SupervisorTerminateTransfering);
                this.Synchronization.SyncBgService?.CurrentProgress?.CancellationTokenSource?.Cancel();
            }
        }

        protected override void OnDeviceFound(string name)
        {
            this.Synchronization.ProcessOperation = string.Format(InterviewerUIResources.SendToSupervisor_MovingToSupervisorFormat, name);
            this.Synchronization.ProcessOperationDescription = InterviewerUIResources.SendToSupervisor_DeviceFound;
        }

        protected override void OnDeviceConnectionAccepting(string name) => 
            this.Synchronization.ProcessOperationDescription = InterviewerUIResources.SendToSupervisor_DeviceConnectionAccepting;

        protected override void OnDeviceConnectionAccepted(string name) => 
            this.Synchronization.ProcessOperationDescription = InterviewerUIResources.SendToSupervisor_DeviceConnectionAccepted;

        private void StopDiscovery() => this.nearbyConnection.StopDiscovery();

        protected override string GetDeviceIdentification() =>
            this.principal.CurrentUserIdentity.SupervisorId.FormatGuid();

        public void ShowSynchronizationError(string error)
        {
            this.Synchronization.Status = SynchronizationStatus.Fail;
            this.Synchronization.ProcessOperation = error;
            this.Synchronization.ProcessOperationDescription = string.Empty;

            this.Synchronization.IsSynchronizationInProgress = false;
        }

        private void Synchronization_OnCancel(object sender, EventArgs e)
        {
        }

        private async void Synchronization_OnProgressChanged(object sender, SharedKernels.Enumerator.Services.Synchronization.SyncProgressInfo e)
        {
            if (this.cancellationTokenSource == null || this.cancellationTokenSource.IsCancellationRequested)
            {
                return;
            }

            if (SynchronizationWithHqEnabled) return;

            var request = new SendSyncProgressInfoRequest
            {
                Info = e,
                InterviewerLogin = this.principal.CurrentUserIdentity.Name
            };

            await syncClient.SendAsync(request, this.cancellationTokenSource.Token);
        }

        #endregion
    }

    public class DashboardViewModelArgs
    {
        public Guid InterviewId { get; set; }
    }
}

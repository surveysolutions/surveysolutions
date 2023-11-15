using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Entities;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.OfflineSync.ViewModels;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Workspace;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Core.SharedKernels.Enumerator.ViewModels.Messages;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public class DashboardViewModel : BaseOfflineSyncViewModel<DashboardViewModelArgs>
    {
        private readonly IInterviewerPrincipal principal;

        private readonly IInterviewerSettings interviewerSettings;
        private readonly IPlainStorage<InterviewView> interviewsRepository;
        private readonly IAuditLogService auditLogService;
        private readonly IOfflineSyncClient syncClient;

        private readonly IMvxMessenger messenger;

        private MvxSubscriptionToken? startingLongOperationMessageSubscriptionToken;
        private MvxSubscriptionToken? stopLongOperationMessageSubscriptionToken;

        public CreateNewViewModel CreateNew { get; }
        public StartedInterviewsViewModel StartedInterviews { get; }
        public CompletedInterviewsViewModel CompletedInterviews { get; }
        public WebInterviewsViewModel WebInterviews { get; }
        public RejectedInterviewsViewModel RejectedInterviews { get; }
        public IUserInteractionService UserInteractionService { get; }
        public IGoogleApiService GoogleApiService { get; }

        public DashboardViewModel(IViewModelNavigationService viewModelNavigationService,
            IInterviewerPrincipal principal,
            LocalSynchronizationViewModel synchronization,
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
            IUserInteractionService userInteractionService,
            IOfflineSyncClient syncClient,
            IGoogleApiService googleApiService,
            IMapInteractionService mapInteractionService,
            DashboardNotificationsViewModel dashboardNotifications,
            IWorkspaceService workspaceService,
            IOnlineSynchronizationService onlineSynchronizationService,
            IWorkspaceMemoryCacheSource memoryCacheSource,
            WebInterviewsViewModel webInterviews,
            IMvxMessenger messenger) : base(principal, viewModelNavigationService, permissionsService,
            nearbyConnection)
        {
            this.messenger = messenger;
            this.principal = principal;
            this.interviewerSettings = interviewerSettings;
            this.interviewsRepository = interviewsRepository;
            this.auditLogService = auditLogService;
            this.syncClient = syncClient;
            this.Synchronization = synchronization;
            this.DashboardNotifications = dashboardNotifications;
            
            this.syncSubscription = synchronizationCompleteSource.SynchronizationEvents.Subscribe(async r =>
            {
                await this.RefreshDashboard();
            });

            this.CreateNew = createNewViewModel;
            this.StartedInterviews = startedInterviewsViewModel;
            this.CompletedInterviews = completedInterviewsViewModel;
            this.RejectedInterviews = rejectedInterviewsViewModel;
            this.WebInterviews = webInterviews;

            UserInteractionService = userInteractionService;
            GoogleApiService = googleApiService;
            this.mapInteractionService = mapInteractionService;
            this.workspaceService = workspaceService;
            this.onlineSynchronizationService = onlineSynchronizationService;
            this.memoryCacheSource = memoryCacheSource;

            SubscribeOnMessages();

            this.Synchronization.Init();
            this.Synchronization.OnCancel += Synchronization_OnCancel;
            this.Synchronization.OnProgressChanged += Synchronization_OnProgressChanged;
            this.StartedInterviews.OnInterviewRemoved += this.OnInterviewRemoved;
            this.CompletedInterviews.OnInterviewRemoved += this.OnInterviewRemoved;
            this.WebInterviews.OnInterviewRemoved += this.OnInterviewRemoved;

            this.StartedInterviews.OnItemsLoaded += this.OnItemsLoaded;
            this.RejectedInterviews.OnItemsLoaded += this.OnItemsLoaded;
            this.CompletedInterviews.OnItemsLoaded += this.OnItemsLoaded;
            this.CreateNew.OnItemsLoaded += this.OnItemsLoaded;
            this.WebInterviews.OnItemsLoaded += this.OnItemsLoaded;
        }

        public DashboardNotificationsViewModel DashboardNotifications { get; set; }

        public override void Prepare(DashboardViewModelArgs parameter)
        {
            this.LastVisitedInterviewId = parameter.InterviewId;
            this.SelectTypeOfInterviewsByInterviewId(this.LastVisitedInterviewId);
        }

        public override async Task Initialize()
        {
            await base.Initialize().ConfigureAwait(false);
            await this.RefreshDashboard(this.LastVisitedInterviewId);
            this.SelectTypeOfInterviewsByInterviewId(this.LastVisitedInterviewId);
        }
        
        public override void ViewAppeared()
        {
            if (!this.principal.IsAuthenticated)
            {
                this.ViewModelNavigationService.NavigateToLoginAsync().WaitAndUnwrapException();
                return;
            }

            base.ViewAppeared();

            SubscribeOnMessages();
            this.SynchronizationWithHqEnabled = this.interviewerSettings.AllowSyncWithHq;

            DashboardNotifications.CheckTabletTimeAndWarn();
        }
        
        public override void ViewDisappeared()
        {
            UnsubscribeFromMessages();
            base.ViewDisappeared();
        }

        private void SubscribeOnMessages()
        {
            startingLongOperationMessageSubscriptionToken =
                messenger.Subscribe<StartingLongOperationMessage>(this.DashboardItemOnStartingLongOperation);
            stopLongOperationMessageSubscriptionToken =
                messenger.Subscribe<StopingLongOperationMessage>(this.DashboardItemOnStopLongOperation);
        }

        private void UnsubscribeFromMessages()
        {
            startingLongOperationMessageSubscriptionToken?.Dispose();
            stopLongOperationMessageSubscriptionToken?.Dispose();
        }

        public bool SynchronizationWithHqEnabled
        {
            get => synchronizationWithHqEnabled;
            private set => SetProperty(ref synchronizationWithHqEnabled, value);
        }

        private Guid? LastVisitedInterviewId { set; get; }

        private IMvxCommand? synchronizationCommand;
        public IMvxCommand SynchronizationCommand
        {
            get
            {
                return synchronizationCommand ??= new MvxCommand(this.RunSynchronization,
                    () => !this.Synchronization.IsSynchronizationInProgress);
            }
        }

        public IMvxAsyncCommand SignOutCommand => new MvxAsyncCommand(async () =>
        {
            await this.SignOut();
        });

        public IMvxAsyncCommand NavigateToDiagnosticsPageCommand => new MvxAsyncCommand(async () =>
        {
            this.Synchronization.CancelSynchronizationCommand.Execute();
            await this.ViewModelNavigationService.NavigateToAsync<DiagnosticsViewModel>();
            this.Dispose();
        });

        public IMvxAsyncCommand NavigateToMapsCommand => new MvxAsyncCommand(async() =>
        {
            await this.NavigateToMaps();
        });

        public IMvxAsyncCommand NavigateToMapDashboardCommand =>
            new MvxAsyncCommand(async () => await NavigateToMapDashboard(), 
                () => !string.IsNullOrEmpty(CurrentWorkspace));

        private async Task NavigateToMapDashboard()
        {
            try
            {
                this.Synchronization.CancelSynchronizationCommand.Execute();
                await mapInteractionService.OpenInterviewerMapDashboardAsync();
                this.Dispose();
            }
            catch (MissingPermissionsException e)
            {
                UserInteractionService.ShowToast(e.Message);
            }
        }

        private async Task NavigateToMaps()
        {
            this.Synchronization.CancelSynchronizationCommand.Execute();
            await this.ViewModelNavigationService.NavigateToAsync<MapsViewModel>();
            this.Dispose();
        }

        private bool isInProgress;
        public bool IsInProgress
        {
            get => this.isInProgress;
            set => SetProperty(ref this.isInProgress, value);
        }

        private DashboardGroupType typeOfInterviews;
        private bool synchronizationWithHqEnabled;
        private readonly IDisposable syncSubscription;

        public DashboardGroupType TypeOfInterviews
        {
            get => this.typeOfInterviews;
            set => SetProperty(ref this.typeOfInterviews, value);
        }

        private int NumberOfAssignedInterviews => this.StartedInterviews.ItemsCount
                                                  + this.CompletedInterviews.ItemsCount
                                                  + this.RejectedInterviews.ItemsCount
                                                  + this.WebInterviews.ItemsCount;

        public LocalSynchronizationViewModel Synchronization { get; set; }

        public string DashboardTitle
            => EnumeratorUIResources.Dashboard_Title.FormatString(this.NumberOfAssignedInterviews.ToString(),
                Principal.CurrentUserIdentity.Name);

        private async void OnInterviewRemoved(object? sender, InterviewRemovedArgs e)
        {
            await this.RaisePropertyChanged(() => this.DashboardTitle);
            this.CreateNew.UpdateAssignment(e.AssignmentId);
            await this.CreateNew.LoadAsync(this.Synchronization);
        }

        private void OnItemsLoaded(object? sender, EventArgs e) =>
            this.IsInProgress = !(this.StartedInterviews.IsItemsLoaded
                                  && this.RejectedInterviews.IsItemsLoaded
                                  && this.CompletedInterviews.IsItemsLoaded
                                  && this.CreateNew.IsItemsLoaded
                                  && this.WebInterviews.IsItemsLoaded);

        private async Task RefreshDashboard(Guid? lastVisitedInterviewId = null)
        {
            this.IsInProgress = true;

            await Task.WhenAll(this.CreateNew.LoadAsync(this.Synchronization),
                this.StartedInterviews.LoadAsync(lastVisitedInterviewId),
                this.RejectedInterviews.LoadAsync(lastVisitedInterviewId),
                this.CompletedInterviews.LoadAsync(lastVisitedInterviewId),
                this.WebInterviews.LoadAsync(lastVisitedInterviewId));
            
            DashboardNotifications.CheckTabletTimeAndWarn();

            await this.RaisePropertyChanged(() => this.DashboardTitle);
        }

        private void SelectTypeOfInterviewsByInterviewId(Guid? lastVisitedInterviewId)
        {
            if (!lastVisitedInterviewId.HasValue)
            {
                this.TypeOfInterviews = this.CreateNew.DashboardType;
                return;
            }

            var interviewView = this.interviewsRepository.GetById(lastVisitedInterviewId.FormatGuid());
            if (interviewView == null) return;

            this.TypeOfInterviews = (interviewView.Mode, interviewView.Status) switch
            {
                (InterviewMode.CAPI, InterviewStatus.RejectedBySupervisor) => this.RejectedInterviews.DashboardType,
                (InterviewMode.CAPI, InterviewStatus.Completed) => this.CompletedInterviews.DashboardType,
                (InterviewMode.CAPI, InterviewStatus.InterviewerAssigned) => this.StartedInterviews.DashboardType,
                (InterviewMode.CAPI, InterviewStatus.Restarted) => this.StartedInterviews.DashboardType,
                (InterviewMode.CAWI, InterviewStatus.InterviewerAssigned) => this.WebInterviews.DashboardType,
                (InterviewMode.CAWI, InterviewStatus.Completed) => this.WebInterviews.DashboardType,
                (InterviewMode.CAWI, InterviewStatus.RejectedBySupervisor) => this.WebInterviews.DashboardType,
                (InterviewMode.CAWI, InterviewStatus.Restarted) => this.WebInterviews.DashboardType,
                _ => this.TypeOfInterviews
            };
        }

        private void RunSynchronization()
        {
            if (this.ViewModelNavigationService.HasPendingOperations)
            {
                this.ViewModelNavigationService.ShowWaitMessage();
                return;
            }

            this.Synchronization.IsSynchronizationInProgress = true;
            this.Synchronization.Synchronize();
        }

        private async Task SignOut()
        {
            this.Synchronization.CancelSynchronizationCommand.Execute();
            var userName = this.Principal.CurrentUserIdentity.Name;
            this.auditLogService.Write(new LogoutAuditLogEntity(userName));
            await this.ViewModelNavigationService.SignOutAndNavigateToLoginAsync();
            this.Dispose();
        }

        private void DashboardItemOnStartingLongOperation(StartingLongOperationMessage message)
        {
            IsInProgress = true;
        }

        private void DashboardItemOnStopLongOperation(StopingLongOperationMessage message)
        {
            IsInProgress = false;
        }

        private bool isDisposed = false;
        
        public override void Dispose()
        {
            if (isDisposed) return;
            isDisposed = true;

            UnsubscribeFromMessages();
            syncSubscription.Dispose();

            this.StartedInterviews.OnInterviewRemoved -= this.OnInterviewRemoved;
            this.CompletedInterviews.OnInterviewRemoved -= this.OnInterviewRemoved;
            this.WebInterviews.OnInterviewRemoved -= this.OnInterviewRemoved;
            this.StartedInterviews.OnItemsLoaded -= this.OnItemsLoaded;
            this.RejectedInterviews.OnItemsLoaded -= this.OnItemsLoaded;
            this.CompletedInterviews.OnItemsLoaded -= this.OnItemsLoaded;
            this.WebInterviews.OnItemsLoaded -= this.OnItemsLoaded;
            this.CreateNew.OnItemsLoaded -= this.OnItemsLoaded;
            this.Synchronization.OnCancel -= this.Synchronization_OnCancel;
            this.Synchronization.OnProgressChanged -= this.Synchronization_OnProgressChanged;
            
            base.Dispose();
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
            if (this.LastVisitedInterviewId.HasValue)
            {
                bundle.Data[nameof(LastVisitedInterviewId)] = this.LastVisitedInterviewId.Value.ToString();
            }
        }

        public IMvxAsyncCommand ShowSearchCommand =>
            new MvxAsyncCommand(async()=>
            {
                this.Synchronization.CancelSynchronizationCommand.Execute();
                await ViewModelNavigationService.NavigateToAsync<SearchViewModel>();
                this.Dispose();
            });

        #region Offline synchronization

        public IMvxCommand StartOfflineSyncCommand => new MvxCommand(this.StartOfflineSynchronization);
        public bool DoesSupportMaps => mapInteractionService.DoesSupportMaps;

        public Action? OnOfflineSynchronizationStarted;
        private readonly IMapInteractionService mapInteractionService;
        private readonly IWorkspaceService workspaceService;
        private readonly IOnlineSynchronizationService onlineSynchronizationService;
        private readonly IWorkspaceMemoryCacheSource memoryCacheSource;

        private void StartOfflineSynchronization()
        {
            this.Synchronization.CancelSynchronizationCommand.Execute();

            this.cancellationTokenSource = new CancellationTokenSource();

            this.Synchronization.Status = SynchronizationStatus.Started;
            this.Synchronization.ProcessOperation = InterviewerUIResources.SendToSupervisor_LookingForSupervisor;
            this.Synchronization.ProcessOperationDescription = InterviewerUIResources.SendToSupervisor_CheckSupervisorDevice;

            this.Synchronization.IsSynchronizationInProgress = true;
            this.Synchronization.IsSynchronizationInfoShowed = true;

            this.OnOfflineSynchronizationStarted?.Invoke();
        }

        protected override async Task<bool> TryNearbyWifiDevicesPermission()
        {
            try
            {
                await permissions.AssureHasNearbyWifiDevicesPermissionOrThrow().ConfigureAwait(false);
            }
            catch (MissingPermissionsException)
            {
                ShouldStartAdvertising = false;
                this.OnConnectionError(EnumeratorUIResources.NearbyPermissionRequired,
                    ConnectionStatusCode.MissingPermissionNearbyWifiDevices);
                return false;
            }

            return true;
        }

        protected override async Task OnStartDiscovery()
        {
            var discoveryStatus = await this.nearbyConnection.StartDiscoveryAsync(
                this.GetServiceName(), cancellationTokenSource.Token);

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
                case ConnectionStatusCode.MissingPermissionBluetoothAdvertise:
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
            ((InterviewerIdentity)this.principal.CurrentUserIdentity).SupervisorId.FormatGuid() + CurrentWorkspace;

        public void ShowSynchronizationError(string error)
        {
            this.Synchronization.Status = SynchronizationStatus.Fail;
            this.Synchronization.ProcessOperation = error;
            this.Synchronization.ProcessOperationDescription = string.Empty;

            this.Synchronization.IsSynchronizationInProgress = false;
        }

        private void Synchronization_OnCancel(object? sender, EventArgs e)
        {
            this.nearbyConnection.StopAll();
        }

        private async void Synchronization_OnProgressChanged(object? sender, SharedKernels.Enumerator.Services.Synchronization.SyncProgressInfo e)
        {
            if (e.Status == SynchronizationStatus.Fail
                || e.Status == SynchronizationStatus.Canceled
                || e.Status == SynchronizationStatus.Stopped
                || e.Status == SynchronizationStatus.Success)
            {
                WorkspaceListUpdated?.Invoke(this, EventArgs.Empty);
            }

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

        public string? CurrentWorkspace => principal?.CurrentUserIdentity?.Workspace;
        public WorkspaceView[] GetWorkspaces()
        {
            return workspaceService.GetAll();
        }

        public async Task ChangeWorkspace(string workspaceName)
        {
            var workspaceView = workspaceService.GetByName(workspaceName);
            if (workspaceView?.SupervisorId == null)
                throw new ArgumentException("Can't change workspace. Refresh list from server");

            var interviewerIdentity = (InterviewerIdentity)principal.CurrentUserIdentity;
            interviewerIdentity.Workspace = workspaceView.Name;
            interviewerIdentity.SupervisorId = workspaceView.SupervisorId.Value;
            principal.SaveInterviewer(interviewerIdentity);
            
            memoryCacheSource.ClearAll();

            await ViewModelNavigationService.NavigateToDashboardAsync();
            this.Dispose();
        }

        public event EventHandler? WorkspaceListUpdated;
        
        public async Task RefreshWorkspaces()
        {
            try
            {
                this.Synchronization.IsSynchronizationInProgress = true;
                this.Synchronization.IsSynchronizationInfoShowed = true;

                this.Synchronization.Status = SynchronizationStatus.Started;
                this.Synchronization.ProcessOperation = EnumeratorUIResources.Dashboard_RefreshWorkspaces;
                this.Synchronization.ProcessOperationDescription = string.Empty;
                
                var interviewerApiView = await onlineSynchronizationService.GetInterviewerAsync();
                workspaceService.Save(interviewerApiView.Workspaces.Select(w => new WorkspaceView()
                {
                    Id = w.Name,
                    DisplayName = w.DisplayName,
                    SupervisorId = w.SupervisorId,
                    Disabled = w.Disabled,
                }).ToArray());

                this.Synchronization.ProcessOperation = EnumeratorUIResources.Dashboard_RefreshWorkspacesFinished;

                if (interviewerApiView.Workspaces.All(w => CurrentWorkspace != w.Name))
                {
                    await ViewModelNavigationService.NavigateToDashboardAsync();
                    this.Dispose();
                    return;
                }
                
                WorkspaceListUpdated?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception)
            {
                this.Synchronization.Status = SynchronizationStatus.Fail;
                this.Synchronization.SynchronizationErrorOccured = true;
                this.Synchronization.ProcessOperation = EnumeratorUIResources.Dashboard_RefreshWorkspacesError;
            }
            finally
            {
                this.Synchronization.IsSynchronizationInProgress = false;
            }
        }
    }

    public class DashboardViewModelArgs
    {
        public Guid InterviewId { get; set; }
    }
}

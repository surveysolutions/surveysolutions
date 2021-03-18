using System;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Navigation.EventArguments;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Supervisor.Properties;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard.Services;
using WB.Core.BoundedContexts.Supervisor.Views;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Services.Workspace;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard
{
    public class DashboardViewModelArgs
    {
        public Guid? InterviewId { get; set; }
    }

    public class DashboardViewModel : BaseViewModel<DashboardViewModelArgs>
    {
        private readonly IMvxNavigationService mvxNavigationService;
        private readonly IWorkspaceService workspaceService;
        private readonly ISupervisorSynchronizationService supervisorSynchronizationService;
        private readonly IPlainStorage<SupervisorIdentity> supervisorPlainStorage;
        public Guid? LastVisitedInterviewId { get; set; }

        public IDashboardItemsAccessor DashboardItemsAccessor { get; }
        public LocalSynchronizationViewModel Synchronization { get; set; }

        public string DashboardTitle
        {
            get => dashboardTitle;
            set => SetProperty(ref dashboardTitle, value);
        }

        public GroupStatus TypeOfInterviews
        {
            get => typeOfInterviews;
            set => SetProperty(ref typeOfInterviews, value);
        }

        public DashboardViewModel(IPrincipal principal,
            IViewModelNavigationService viewModelNavigationService,
            IDashboardItemsAccessor dashboardItemsAccessor,
            IMvxNavigationService mvxNavigationService,
            IMvxMessenger messenger,
            LocalSynchronizationViewModel synchronization,
            DashboardNotificationsViewModel dashboardNotifications,
            IWorkspaceService workspaceService,
            ISupervisorSynchronizationService supervisorSynchronizationService,
            IPlainStorage<SupervisorIdentity> supervisorPlainStorage)
            : base(principal, viewModelNavigationService)
        {
            this.mvxNavigationService = mvxNavigationService;
            this.workspaceService = workspaceService;
            this.supervisorSynchronizationService = supervisorSynchronizationService;
            this.supervisorPlainStorage = supervisorPlainStorage;
            DashboardItemsAccessor = dashboardItemsAccessor;
            this.Synchronization = synchronization;
            this.Synchronization.Init();
            this.DashboardNotifications = dashboardNotifications;
            
            this.mvxNavigationService.AfterNavigate += OnAfterNavigate;
            this.Synchronization.OnProgressChanged += SynchronizationOnProgressChanged;

            messengerSubscribtion = messenger.Subscribe<RequestSynchronizationMsg>(msg => SynchronizationCommand.Execute());
        }

        public DashboardNotificationsViewModel DashboardNotifications { get; set; }

        private void OnAfterNavigate(object sender, IMvxNavigateEventArgs args)
        {
            if (args.ViewModel is InterviewTabPanel interviewTabPanel)
            {
                DashboardTitle = interviewTabPanel.Title;
                TypeOfInterviews = interviewTabPanel.InterviewStatus;
            }
        }

        private readonly MvxSubscriptionToken messengerSubscribtion;
        private string dashboardTitle;
        private GroupStatus typeOfInterviews;

        public override void Prepare(DashboardViewModelArgs parameter)
        {
            this.LastVisitedInterviewId = parameter.InterviewId;
        }

        public override async Task Initialize()
        {
            await base.Initialize();
            this.DashboardTitle = SupervisorDashboard.ToBeAssigned;
        }

        public IMvxAsyncCommand SynchronizationCommand => new MvxAsyncCommand(this.RunSynchronization);

        public IMvxCommand SignOutCommand => new MvxAsyncCommand(this.SignOut);

        public IMvxCommand NavigateToDiagnosticsPageCommand => new MvxAsyncCommand(this.NavigateToDiagnostics);

        public IMvxCommand ShowDefaultListCommand =>
            new MvxAsyncCommand(async () =>
                await ViewModelNavigationService.NavigateToAsync<ToBeAssignedItemsViewModel>());

        public IMvxCommand ShowOutboxCommand =>
            new MvxAsyncCommand(async () =>
                await mvxNavigationService.Navigate<OutboxViewModel, Guid?>(this.LastVisitedInterviewId));

        public IMvxAsyncCommand ShowSentCommand => 
            new MvxAsyncCommand(async () => await mvxNavigationService.Navigate<SentToInterviewerViewModel>());

        public IMvxCommand ShowWaitingSupervisorActionCommand =>
            new MvxAsyncCommand(async () =>
                await mvxNavigationService.Navigate<WaitingForSupervisorActionViewModel, Guid?>(this.LastVisitedInterviewId));

        public IMvxAsyncCommand ShowMenuViewModelCommand => new MvxAsyncCommand(async () =>
            await ViewModelNavigationService.NavigateToAsync<DashboardMenuViewModel>());

        public IMvxCommand NavigateToOfflineSyncCommand => new MvxAsyncCommand(this.NavigateToOfflineSync);

        private Task NavigateToOfflineSync()
        {
            this.Synchronization.CancelSynchronizationCommand.Execute();
            return this.ViewModelNavigationService.NavigateToAsync<SupervisorOfflineSyncViewModel>();
        }

        public IMvxCommand NavigateToMapsCommand => new MvxAsyncCommand(() =>
        {
            this.Synchronization.CancelSynchronizationCommand.Execute();
            return this.ViewModelNavigationService.NavigateToAsync<MapsViewModel>();
        });

        public IMvxAsyncCommand ShowSearchCommand => new MvxAsyncCommand(this.ViewModelNavigationService.NavigateToAsync<SearchViewModel>);

        public override void ViewAppeared()
        {
            base.ViewAppeared();
            DashboardNotifications.CheckTabletTimeAndWarn();
        }

        private Task RunSynchronization()
        {
            if (this.Synchronization.IsSynchronizationInProgress)
                return Task.CompletedTask;

            if (this.ViewModelNavigationService.HasPendingOperations)
            {
                this.ViewModelNavigationService.ShowWaitMessage();
            }
            else
            {
                this.Synchronization.IsSynchronizationInProgress = true;
                this.Synchronization.Synchronize();
            }

            return Task.CompletedTask;
        }

        private Task NavigateToDiagnostics()
        {
            this.Synchronization.CancelSynchronizationCommand.Execute();
            return this.ViewModelNavigationService.NavigateToAsync<DiagnosticsViewModel>();
        }

        private Task SignOut()
        {
            this.Synchronization.CancelSynchronizationCommand.Execute();
            return this.ViewModelNavigationService.SignOutAndNavigateToLoginAsync();
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

        public void Dispose()
        {
            messengerSubscribtion.Dispose();
            this.mvxNavigationService.AfterNavigate -= OnAfterNavigate;
            this.Synchronization.OnProgressChanged -= SynchronizationOnProgressChanged;
        }

        private void SynchronizationOnProgressChanged(object sender, SyncProgressInfo e)
        {
            if (e.Status == SynchronizationStatus.Fail
                || e.Status == SynchronizationStatus.Canceled
                || e.Status == SynchronizationStatus.Stopped
                || e.Status == SynchronizationStatus.Success)
            {
                WorkspaceListUpdated?.Invoke(this, EventArgs.Empty);
            }
        }

        public string CurrentWorkspace => Principal.CurrentUserIdentity.Workspace;
        public WorkspaceView[] GetWorkspaces()
        {
            return workspaceService.GetAll();
        }

        public void ChangeWorkspace(string workspaceName)
        {
            var workspaceView = workspaceService.GetByName(workspaceName);

            var supervisorIdentity = (SupervisorIdentity)Principal.CurrentUserIdentity;
            supervisorIdentity.Workspace = workspaceView.Name;
            supervisorPlainStorage.Store(supervisorIdentity);

            ViewModelNavigationService.NavigateToDashboardAsync();
        }

        public event EventHandler WorkspaceListUpdated;
        
        public async Task RefreshWorkspaces()
        {
            try
            {
                this.Synchronization.IsSynchronizationInProgress = true;
                this.Synchronization.IsSynchronizationInfoShowed = true;

                this.Synchronization.Status = SynchronizationStatus.Started;
                this.Synchronization.ProcessOperationDescription = EnumeratorUIResources.Dashboard_RefreshWorkspaces;
                
                var supervisorApiView = await supervisorSynchronizationService.GetSupervisorAsync();
                workspaceService.Save(supervisorApiView.Workspaces.Select(w => new WorkspaceView()
                {
                    Id = w.Name,
                    DisplayName = w.DisplayName,
                    SupervisorId = w.SupervisorId,
                    Disabled = w.Disabled,
                }).ToArray());

                this.Synchronization.ProcessOperationDescription = EnumeratorUIResources.Dashboard_RefreshWorkspacesFinished;

                if (supervisorApiView.Workspaces.All(w => CurrentWorkspace != w.Name))
                {
                    await ViewModelNavigationService.NavigateToDashboardAsync();
                    return;
                }

                WorkspaceListUpdated?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception)
            {
                this.Synchronization.ProcessOperationDescription = EnumeratorUIResources.Dashboard_RefreshWorkspacesError;
            }
            finally
            {
                this.Synchronization.IsSynchronizationInProgress = false;
            }
        }
    }
}

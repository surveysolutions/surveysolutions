using System;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard
{
    public class DashboardViewModelArgs
    {
        public Guid? InterviewId { get; set; }
    }

    public class DashboardViewModel : BaseViewModel<DashboardViewModelArgs>
    {
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly IMvxNavigationService mvxNavigationService;
        public Guid? LastVisitedInterviewId { get; set; }

        public SynchronizationViewModel Synchronization { get; set; }
        //public MapSynchronizationViewModel MapSynchronization { get; set; }
        
        public string DashboardTitle => "dashboard title :)";

        public DashboardViewModel(IPrincipal principal, 
            IViewModelNavigationService viewModelNavigationService,
            IMvxNavigationService mvxNavigationService,
            SynchronizationViewModel synchronization) 
            : base(principal, viewModelNavigationService)
        {
            this.viewModelNavigationService = viewModelNavigationService;
            this.mvxNavigationService = mvxNavigationService;
            this.Synchronization = synchronization;
            this.Synchronization.Init();
        }

        public override void Prepare(DashboardViewModelArgs parameter)
        {
            this.LastVisitedInterviewId = parameter.InterviewId;
        }

        private IMvxAsyncCommand synchronizationCommand;
        public IMvxAsyncCommand SynchronizationCommand
        {
            get
            {
                return synchronizationCommand ?? (synchronizationCommand = new MvxAsyncCommand(this.RunSynchronization, () => !this.Synchronization.IsSynchronizationInProgress));
            }
        }

        public IMvxAsyncCommand MapsSynchronizationCommand
            => new MvxAsyncCommand(this.RunMapsSynchronization, () => !this.Synchronization.IsSynchronizationInProgress);

        public IMvxCommand SignOutCommand => new MvxAsyncCommand(this.SignOut);

        public IMvxCommand NavigateToDiagnosticsPageCommand => new MvxAsyncCommand(this.NavigateToDiagnostics);

        public IMvxCommand ShowDefaultListCommand => 
            new MvxAsyncCommand(async () => await viewModelNavigationService.NavigateToAsync<ToBeAssignedItemsViewModel>());

        public IMvxCommand ShowOutboxCommand =>
            new MvxAsyncCommand(async () => await mvxNavigationService.Navigate<OutboxViewModel, Guid?>(this.LastVisitedInterviewId));

        public IMvxAsyncCommand ShowMenuViewModelCommand => new MvxAsyncCommand(async () => await viewModelNavigationService.NavigateToAsync<DashboardMenuViewModel>());

        public IMvxCommand NavigateToOfflineSyncCommand => new MvxAsyncCommand(this.NavigateToOfflineSync);
        private Task NavigateToOfflineSync()
        {
            this.Synchronization.CancelSynchronizationCommand.Execute();
            return this.viewModelNavigationService.NavigateToAsync<OfflineSupervisorSyncViewModel>();
        }

        private Task RunSynchronization()
        {
            if (this.viewModelNavigationService.HasPendingOperations)
            {
                this.viewModelNavigationService.ShowWaitMessage();
            }
            else
            {
                this.Synchronization.IsSynchronizationInProgress = true;
                this.Synchronization.Synchronize();
            }
            return Task.CompletedTask;
        }

        private Task RunMapsSynchronization()
        {
            if (this.viewModelNavigationService.HasPendingOperations)
            {
                this.viewModelNavigationService.ShowWaitMessage();
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
            return this.viewModelNavigationService.NavigateToAsync<DiagnosticsViewModel>();
        }

        private Task SignOut()
        {
            this.Synchronization.CancelSynchronizationCommand.Execute();
            return this.viewModelNavigationService.SignOutAndNavigateToLoginAsync();
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
    }
}

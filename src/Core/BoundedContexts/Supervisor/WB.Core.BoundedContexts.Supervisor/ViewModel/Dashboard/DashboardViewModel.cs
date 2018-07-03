using System;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.Plugin.Messenger;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard
{
    public class DashboardViewModelArgs
    {
        public Guid InterviewId { get; set; }
    }

    public class DashboardViewModel : BaseViewModel<DashboardViewModelArgs>
    {
        private readonly IViewModelNavigationService viewModelNavigationService;
        private Guid LastVisitedInterviewId { get; set; }

        public SynchronizationViewModel Synchronization { get; set; }
        
        public string DashboardTitle => "dashboard title :)";

        public DashboardViewModel(IPrincipal principal, 
            IViewModelNavigationService viewModelNavigationService,
            SynchronizationViewModel synchronization) 
            : base(principal, viewModelNavigationService)
        {
            this.viewModelNavigationService = viewModelNavigationService;
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

        public IMvxCommand SignOutCommand => new MvxAsyncCommand(this.SignOut);

        public IMvxCommand NavigateToDiagnosticsPageCommand => new MvxAsyncCommand(this.NavigateToDiagnostics);

        public IMvxCommand ShowDefaultListCommand => 
            new MvxAsyncCommand(async () => await viewModelNavigationService.NavigateToAsync<ToBeAssignedItemsViewModel>());

        public IMvxAsyncCommand ShowMenuViewModelCommand => new MvxAsyncCommand(async () => await viewModelNavigationService.NavigateToAsync<DashboardMenuViewModel>());

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
    }
}

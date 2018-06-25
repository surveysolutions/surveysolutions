using System;
using System.Threading.Tasks;
using MvvmCross.Commands;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel
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


        private IMvxCommand synchronizationCommand;
        public IMvxCommand SynchronizationCommand
        {
            get
            {
                return synchronizationCommand ?? (synchronizationCommand = new MvxCommand(this.RunSynchronization, () => !this.Synchronization.IsSynchronizationInProgress));
            }
        }

        public IMvxCommand SignOutCommand => new MvxAsyncCommand(this.SignOut);

        public IMvxCommand NavigateToDiagnosticsPageCommand => new MvxAsyncCommand(this.NavigateToDiagnostics);

        public IMvxCommand ShowInterviewsCommand => 
            new MvxAsyncCommand(async () => await viewModelNavigationService.NavigateToAsync<DashboardCompletedInterviewsViewModel>());

        public IMvxAsyncCommand ShowMenuViewModelCommand => new MvxAsyncCommand(async () => await viewModelNavigationService.NavigateToAsync<DashboardMenuViewModel>());

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
            return this.viewModelNavigationService.SignOutAndNavigateToLoginAsync();
        }
    }
}

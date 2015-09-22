using Cirrious.MvvmCross.ViewModels;

using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class TroubleshootingViewModel : BaseViewModel
    {
        private readonly IPrincipal principal;
        private readonly IViewModelNavigationService viewModelNavigationService;

        public TroubleshootingViewModel(
            IViewModelNavigationService viewModelNavigationService, 
            IPrincipal principal)
        {
            this.viewModelNavigationService = viewModelNavigationService;
            this.principal = principal;
        }
        public IMvxCommand NavigateToLoginCommand
        {
            get { return new MvxCommand(() => this.viewModelNavigationService.NavigateTo<LoginViewModel>()); }
        }
        
        public IMvxCommand NavigateToDashboardCommand
        {
            get { return new MvxCommand(() => this.viewModelNavigationService.NavigateToDashboard()); }
        }

        private IMvxCommand signOutCommand;
        public IMvxCommand SignOutCommand
        {
            get { return this.signOutCommand ?? (this.signOutCommand = new MvxCommand(this.SignOut)); }
        }

        void SignOut()
        {
            this.principal.SignOut();
            this.viewModelNavigationService.NavigateTo<LoginViewModel>();
        }

        public bool IsAuthenticated
        {
            get
            {
                return this.principal.IsAuthenticated;
            }
        }
    }
}

using MvvmCross.ViewModels;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public abstract class BasePrincipalViewModel : BaseViewModel
    {
        protected readonly IPrincipal Principal;
        protected readonly IViewModelNavigationService ViewModelNavigationService;
        private readonly bool isAuthenticationRequired;

        protected BasePrincipalViewModel(IPrincipal principal, 
            IViewModelNavigationService viewModelNavigationService, 
            bool isAuthenticationRequired = true)
        {
            this.Principal = principal;
            this.ViewModelNavigationService = viewModelNavigationService;
            this.isAuthenticationRequired = isAuthenticationRequired;
        }
        
        public override void ViewAppearing()
        {
            base.ViewAppearing();
            BaseViewModelSetupMethods.CheckAuthentication(isAuthenticationRequired, this.Principal, this.ViewModelNavigationService);
        }
        
        protected override void ReloadFromBundle(IMvxBundle parameters)
        {
            base.ReloadFromBundle(parameters);
            BaseViewModelSetupMethods.ReloadStateFromBundle(this.Principal, parameters);
        }

        protected override void SaveStateToBundle(IMvxBundle bundle)
        {
            base.SaveStateToBundle(bundle);
            BaseViewModelSetupMethods.SaveStateToBundle(this.Principal, bundle);
        }
    }
}

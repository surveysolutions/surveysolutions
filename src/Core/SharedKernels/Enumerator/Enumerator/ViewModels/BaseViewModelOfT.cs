using System;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public abstract class BaseViewModel<T> : MvxViewModel<T>
    {
        protected readonly IPrincipal Principal;
        protected readonly IViewModelNavigationService ViewModelNavigationService;
        
        protected BaseViewModel(IPrincipal principal,
            IViewModelNavigationService viewModelNavigationService,
            bool isAuthenticationRequired = true)
        {
            this.Principal = principal ?? throw new ArgumentNullException(nameof(principal));
            this.ViewModelNavigationService = viewModelNavigationService ?? throw new ArgumentNullException(nameof(viewModelNavigationService));
            
            BaseViewModelSetupMethods.CheckAuthentication(isAuthenticationRequired, this.Principal, this.ViewModelNavigationService);
        }

        protected override void ReloadFromBundle(IMvxBundle state)
        {
            BaseViewModelSetupMethods.ReloadStateFromBundle(this.Principal, state);
        }

        protected override void SaveStateToBundle(IMvxBundle bundle)
        {
            BaseViewModelSetupMethods.SaveStateToBundle(this.Principal, bundle);
        }
    }
}

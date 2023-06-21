using System;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public abstract class BaseViewModel<T> : MvxViewModel<T>, IDisposable
    {
        protected readonly IPrincipal Principal;
        protected readonly IViewModelNavigationService ViewModelNavigationService;
        private readonly bool isAuthenticationRequired;

        protected BaseViewModel(IPrincipal principal,
            IViewModelNavigationService viewModelNavigationService,
            bool isAuthenticationRequired = true)
        {
            this.Principal = principal ?? throw new ArgumentNullException(nameof(principal));
            this.ViewModelNavigationService = viewModelNavigationService ?? throw new ArgumentNullException(nameof(viewModelNavigationService));
            this.isAuthenticationRequired = isAuthenticationRequired;
        }
        
        public override void ViewAppearing()
        {
            base.ViewAppearing();
            BaseViewModelSetupMethods.CheckAuthentication(isAuthenticationRequired, this.Principal, this.ViewModelNavigationService);
        }
        
        protected override void ReloadFromBundle(IMvxBundle state)
        {
            base.ReloadFromBundle(state);
            BaseViewModelSetupMethods.ReloadStateFromBundle(this.Principal, state);
        }

        protected override void SaveStateToBundle(IMvxBundle bundle)
        {
            base.SaveStateToBundle(bundle);
            BaseViewModelSetupMethods.SaveStateToBundle(this.Principal, bundle);
        }

        public virtual void Dispose()
        {
        }
        
        public override void ViewDestroy(bool viewFinishing = true)
        {
            if(viewFinishing)
                Dispose();
            
            base.ViewDestroy(viewFinishing);
        }
    }
}

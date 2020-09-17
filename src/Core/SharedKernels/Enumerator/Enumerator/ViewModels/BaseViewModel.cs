using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public abstract class BaseViewModel : MvxViewModel
    {
        protected readonly IPrincipal Principal;
        protected readonly IViewModelNavigationService ViewModelNavigationService;
        private readonly bool isAuthenticationRequired;

        protected BaseViewModel(IPrincipal principal, 
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

        // it's much more performant, as original extension call new Action<...> on every call
        protected void RaiseAndSetIfChanged<TReturn>(ref TReturn backingField, TReturn newValue, 
            [CallerMemberName] string propertyName = null)
        {
            SetProperty(ref backingField, newValue, propertyName);
        }
    }
}

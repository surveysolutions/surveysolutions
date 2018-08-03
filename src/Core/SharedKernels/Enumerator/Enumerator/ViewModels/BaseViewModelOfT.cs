using System;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public abstract class BaseViewModel<T> : MvxViewModel<T>
    {
        protected readonly IPrincipal Principal;
        protected readonly IViewModelNavigationService viewModelNavigationService;
        protected virtual bool IsAuthenticationRequired => true;

        protected BaseViewModel(IPrincipal principal, IViewModelNavigationService viewModelNavigationService)
        {
            this.Principal = principal ?? throw new ArgumentNullException(nameof(principal));
            this.viewModelNavigationService = viewModelNavigationService ?? throw new ArgumentNullException(nameof(viewModelNavigationService));
        }

        public override void Prepare()
        {
            base.Prepare();
            BaseViewModelSetupMethods.Prepare(this.IsAuthenticationRequired, this.Principal, this.viewModelNavigationService);
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

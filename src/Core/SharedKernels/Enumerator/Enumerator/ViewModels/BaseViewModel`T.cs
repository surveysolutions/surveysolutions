using System;
using MvvmCross.Core.ViewModels;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public abstract class BaseViewModel<T> : MvxViewModel<T>
    {
        protected readonly IPrincipal Principal;
        private readonly IViewModelNavigationService viewModelNavigationService;

        protected BaseViewModel(IPrincipal principal, IViewModelNavigationService viewModelNavigationService)
        {
            this.Principal = principal ?? throw new ArgumentNullException(nameof(principal));
            this.viewModelNavigationService = viewModelNavigationService ?? throw new ArgumentNullException(nameof(viewModelNavigationService));
        }

        public override void Prepare()
        {
            base.Prepare();
            if (!this.Principal.IsAuthenticated)
            {
                this.viewModelNavigationService.NavigateToSplashScreen();
            }
        }

        protected override void SaveStateToBundle(IMvxBundle bundle)
        {
            if (this.Principal?.IsAuthenticated ?? false)
            {
                bundle.Data["userName"] = this.Principal.CurrentUserIdentity.Name;
                bundle.Data["passwordHash"] = this.Principal.CurrentUserIdentity.PasswordHash;
            }
        }

        protected override void ReloadFromBundle(IMvxBundle state)
        {
            if (state.Data.ContainsKey("userName") && !this.Principal.IsAuthenticated)
            {
                this.Principal.SignInWithHash(state.Data["userName"], state.Data["passwordHash"], true);
            }
        }
    }
}

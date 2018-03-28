using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MvvmCross.Core.ViewModels;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public abstract class BaseViewModel : MvxViewModel
    {
        protected readonly IPrincipal principal;
        private readonly IViewModelNavigationService viewModelNavigationService;

        protected BaseViewModel(IPrincipal principal, IViewModelNavigationService viewModelNavigationService)
        {
            this.principal = principal;
            this.viewModelNavigationService = viewModelNavigationService;
        }

        public virtual bool IsAuthenticationRequired => true;

        public override void Prepare()
        {
            base.Prepare();
            
            if (this.IsAuthenticationRequired && !this.principal.IsAuthenticated)
            {
                this.viewModelNavigationService.NavigateToSplashScreen();
            }
        }

        protected override void ReloadFromBundle(IMvxBundle parameters)
        {
            base.ReloadFromBundle(parameters);
            if (parameters.Data.ContainsKey("userName") && !this.principal.IsAuthenticated)
            {
                this.principal.SignInWithHash(parameters.Data["userName"], parameters.Data["passwordHash"], true);
            }
        }

        protected override void SaveStateToBundle(IMvxBundle bundle)
        {
            base.SaveStateToBundle(bundle);
            if (this.principal?.IsAuthenticated ?? false)
            {
                bundle.Data["userName"] = this.principal.CurrentUserIdentity.Name;
                bundle.Data["passwordHash"] = this.principal.CurrentUserIdentity.PasswordHash;
            }
        }

        // it's much more performant, as original extension call new Action<...> on every call
        protected void RaiseAndSetIfChanged<TReturn>(ref TReturn backingField, TReturn newValue, 
            [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<TReturn>.Default.Equals(backingField, newValue)) return;
            backingField = newValue;
            this.RaisePropertyChanged(propertyName);
        }
    }
}

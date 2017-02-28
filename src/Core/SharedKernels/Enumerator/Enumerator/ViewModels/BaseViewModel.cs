using MvvmCross.Core.ViewModels;
using MvvmCross.Platform;
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

        public virtual void Load() { }

        protected override void InitFromBundle(IMvxBundle parameters)
        {
            base.InitFromBundle(parameters);
            if (parameters.Data.ContainsKey("userName") && !this.principal.IsAuthenticated)
            {
                this.principal.SignIn(parameters.Data["userName"], parameters.Data["passwordHash"], true);
            }
        }

        protected override void SaveStateToBundle(IMvxBundle bundle)
        {
            base.SaveStateToBundle(bundle);
            if (this.principal.IsAuthenticated)
            {
                bundle.Data["userName"] = this.principal.CurrentUserIdentity.Name;
                bundle.Data["passwordHash"] = this.principal.CurrentUserIdentity.Password;
            }
        }

        public override void Start()
        {
            base.Start();

            if (this.IsAuthenticationRequired && !this.principal.IsAuthenticated)
            {
                this.viewModelNavigationService.NavigateToLogin();
                return;
            }

            this.Load();
        }
    }
}
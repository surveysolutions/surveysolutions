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

        public virtual void Load() { }

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
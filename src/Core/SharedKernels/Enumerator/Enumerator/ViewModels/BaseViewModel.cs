using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using Nito.AsyncEx.Synchronous;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public abstract class BaseViewModel : MvxViewModel
    {
        private readonly IPrincipal principal;
        private readonly IViewModelNavigationService viewModelNavigationService;

        protected BaseViewModel(IPrincipal principal, IViewModelNavigationService viewModelNavigationService)
        {
            this.principal = principal;
            this.viewModelNavigationService = viewModelNavigationService;
        }

        public virtual bool IsAuthenticationRequired => true;

        public virtual Task StartAsync()
        {
            return Task.FromResult(true);
        }

        public override void Start()
        {
            base.Start();

            if (this.IsAuthenticationRequired && !this.principal.IsAuthenticated)
            {
                this.viewModelNavigationService.NavigateToLoginAsync().WaitAndUnwrapException();
                return;
            }

            this.StartAsync().WaitAndUnwrapException();
        }
    }
}
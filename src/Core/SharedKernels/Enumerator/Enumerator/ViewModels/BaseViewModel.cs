using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;
using MvvmCross.Core.ViewModels;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public abstract class BaseViewModel : MvxViewModel
    {
        private IPrincipal principal => ServiceLocator.Current.GetInstance<IPrincipal>();

        private IViewModelNavigationService viewModelNavigationService
            => ServiceLocator.Current.GetInstance<IViewModelNavigationService>();

        public virtual bool IsAuthenticationRequired => true;

        public virtual Task StartAsync()
        {
            return Task.FromResult(true);
        }

        public override async void Start()
        {
            base.Start();

            if (this.IsAuthenticationRequired && !this.principal.IsAuthenticated)
            {
                await this.viewModelNavigationService.NavigateToLoginAsync();
                return;
            }

            await this.StartAsync();
        }
    }
}
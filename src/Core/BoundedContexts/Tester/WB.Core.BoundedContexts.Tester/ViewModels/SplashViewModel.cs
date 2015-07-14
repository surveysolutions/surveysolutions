using System.Threading.Tasks;
using WB.Core.BoundedContexts.Tester.Infrastructure;
using WB.Core.BoundedContexts.Tester.Services;

namespace WB.Core.BoundedContexts.Tester.ViewModels
{
    public class SplashViewModel : BaseViewModel
    {
        private readonly IPrincipal principal;
        private readonly IViewModelNavigationService viewModelNavigationService;

        public SplashViewModel(IPrincipal principal, IViewModelNavigationService viewModelNavigationService)
        {
            this.principal = principal;
            this.viewModelNavigationService = viewModelNavigationService;
        }

        public override async void Start()
        {
            await Task.Delay(3000);

            if (this.principal.IsAuthenticated)
            {
                this.viewModelNavigationService.NavigateTo<DashboardViewModel>();
            }
            else
                this.viewModelNavigationService.NavigateTo<LoginViewModel>();
        }

        public override void NavigateToPreviousViewModel()
        {
            
        }
    }
}
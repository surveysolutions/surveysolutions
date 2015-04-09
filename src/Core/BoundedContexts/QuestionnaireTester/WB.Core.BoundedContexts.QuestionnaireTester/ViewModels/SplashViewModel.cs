using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Utils.Services;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class SplashViewModel : BaseViewModel
    {
        private readonly IPrincipal principal;

        public SplashViewModel(IPrincipal principal)
        {
            this.principal = principal;
        }

        public override async void Start()
        {
            await Task.Delay(3000);

            if (this.principal.CurrentUserIdentity.IsAuthenticated)
            {
                this.ShowViewModel<DashboardViewModel>();
            }
            else
                this.ShowViewModel<LoginViewModel>();
        }

        public override void NavigateToPreviousViewModel()
        {
            
        }
    }
}
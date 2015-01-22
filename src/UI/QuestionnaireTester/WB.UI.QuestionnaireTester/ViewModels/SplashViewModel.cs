using WB.Core.GenericSubdomains.Utils.Services;
using WB.UI.QuestionnaireTester.Services;

namespace WB.UI.QuestionnaireTester.ViewModels
{
    public class SplashViewModel : BaseViewModel
    {
        private readonly IPrincipal principal;

        public SplashViewModel(IPrincipal principal, ILogger logger) : base(logger, principal: principal)
        {
            this.principal = principal;
        }

        public void Load()
        {
            if (this.principal.CurrentIdentity.IsAuthenticated)
                this.ShowViewModel<DashboardViewModel>();
            else
                this.ShowViewModel<LoginViewModel>();
        }
    }
}
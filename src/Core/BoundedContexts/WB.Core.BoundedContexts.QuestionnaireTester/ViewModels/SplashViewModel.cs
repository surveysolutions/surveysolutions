using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Utils.Services;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class SplashViewModel : BaseViewModel
    {
        public SplashViewModel(IPrincipal principal) : base(principal: principal, logger: null)
        {
        }

        public void Init()
        {
            Task.Run(() =>
            {
                Thread.Sleep(3000);

                if (this.Principal.CurrentIdentity.IsAuthenticated)
                {
                    this.ShowViewModel<DashboardViewModel>();
                }
                else
                {
                    this.ShowViewModel<LoginViewModel>();
                }
            });
        }
    }
}
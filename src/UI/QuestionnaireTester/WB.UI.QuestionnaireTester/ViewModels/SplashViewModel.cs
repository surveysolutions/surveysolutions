using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Utils.Services;

namespace WB.UI.QuestionnaireTester.ViewModels
{
    public class SplashViewModel : BaseViewModel
    {
        private readonly IApplicationInitializer applicationInitializer;

        public SplashViewModel(IApplicationInitializer applicationInitializer) : base(null, null)
        {
            this.applicationInitializer = applicationInitializer;
        }

        public void Init()
        {
            Task.Run(() =>
            {
                this.applicationInitializer.Init();
                this.ShowViewModel<DashboardViewModel>();
            });
        }
    }
}
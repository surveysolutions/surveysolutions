using System.Threading.Tasks;
using WB.UI.QuestionnaireTester.Ninject;

namespace WB.UI.QuestionnaireTester.ViewModels
{
    public class SplashViewModel : BaseViewModel
    {
        public SplashViewModel() : base(null, null)
        {
        }

        public void Init()
        {
            Task.Run(() =>
            {
                NinjectInitializer.Initialize();
                MvxInitializer.Initialize();

                this.ShowViewModel<DashboardViewModel>();
            });
        }
    }
}
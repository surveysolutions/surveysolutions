using System.Threading;
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
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
                Thread.Sleep(6000);
                this.ShowViewModel<DashboardViewModel>();
            });
        }
    }
}
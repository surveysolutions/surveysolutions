using Cirrious.MvvmCross.ViewModels;
using WB.UI.Capi.ViewModel;

namespace WB.UI.Capi
{
    public class App : MvxApplication
    {
        public override void Initialize()
        {
            RegisterAppStart<SplashViewModel>();
        }
    }
}
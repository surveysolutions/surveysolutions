using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Tester.ViewModels;

namespace WB.UI.Tester
{
    public class App : MvxApplication
    {
        public override void Initialize()
        {
            RegisterAppStart<SplashViewModel>();
        }
    }
}
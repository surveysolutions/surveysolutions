using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using WB.UI.QuestionnaireTester.ViewModels;

namespace WB.UI.QuestionnaireTester
{
    public class App : MvxApplication
    {
        public App()
        {
            Mvx.RegisterSingleton<IMvxAppStart>(new MvxAppStart<SplashViewModel>());
        }
    }
}
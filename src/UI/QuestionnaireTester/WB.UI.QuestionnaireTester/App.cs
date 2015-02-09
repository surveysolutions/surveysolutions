using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.UI.QuestionnaireTester.Implementation.Services;

namespace WB.UI.QuestionnaireTester
{
    public class App : MvxApplication
    {
        public App()
        {
            Mvx.RegisterSingleton<IApplicationInitializer>(new ApplicationInitializer());
            Mvx.RegisterSingleton<IMvxAppStart>(new MvxAppStart<SplashViewModel>());
        }
    }
}
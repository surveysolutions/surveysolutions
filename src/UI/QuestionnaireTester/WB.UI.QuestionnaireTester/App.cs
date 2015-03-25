using Cirrious.CrossCore;
using Cirrious.CrossCore.IoC;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;

namespace WB.UI.QuestionnaireTester
{
    public class App : MvxApplication
    {
        public override void Initialize()
        {
            RegisterAppStart<SplashViewModel>();
        }
    }
}
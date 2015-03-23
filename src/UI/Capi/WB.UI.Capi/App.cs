using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Capi.Views.Login;

namespace WB.UI.Capi
{
    public class App: MvxApplication
    {
        public App()
        {
            Mvx.RegisterSingleton<IMvxAppStart>(new MvxAppStart<LoginViewModel>());
        }
    }
}
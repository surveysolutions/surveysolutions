using CAPI.Android.Settings;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Capi.Views.Login;
using WB.UI.Capi.Settings;
using FinishIntallationViewModel = WB.UI.Capi.Views.FinishIntallationViewModel;

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
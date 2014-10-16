using CAPI.Android.Settings;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Capi.Views.FinishInstallation;
using WB.Core.BoundedContexts.Capi.Views.Login;
using WB.UI.Capi.Settings;

namespace WB.UI.Capi
{
    public class App: MvxApplication
    {
        public App()
        {
            if (string.IsNullOrWhiteSpace(SettingsManager.GetSetting(SettingsNames.RegistrationKeyName)))
            {
                Mvx.RegisterSingleton<IMvxAppStart>(new MvxAppStart<FinishIntallationViewModel>());
            }
            else
            {
                Mvx.RegisterSingleton<IMvxAppStart>(new MvxAppStart<LoginViewModel>());
            }
        }
    }
}
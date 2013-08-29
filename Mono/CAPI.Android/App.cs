

using CAPI.Android.Core.Model.ViewModel.Login;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;

namespace CAPI.Android
{
    public class App: MvxApplication
    {
        public App()
        {
            Mvx.RegisterSingleton<IMvxAppStart>(new MvxAppStart<LoginViewModel>());
        }
    }
}
using Android.Content;
using CAPI.Android.Core.Model.ViewModel.Login;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Views;
using WB.UI.Shared.Android;

namespace WB.UI.Capi.Tester
{
    public class Setup : CapiSharedSetup
    {
        public Setup(Context applicationContext) : base(applicationContext)
        {
        }

        protected override void InitializeViewLookup()
        {
            base.InitializeViewLookup();
            var container = Mvx.Resolve<IMvxViewsContainer>();
#warning: change viewModelType
            container.Add(typeof(LoginViewModel), typeof(HomeActivity));
        }

        protected override IMvxApplication CreateApp()
        {
            return new App();
        }
    }
}
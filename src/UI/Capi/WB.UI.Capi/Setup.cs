using Android.Content;
using CAPI.Android.Core.Model.ViewModel.Login;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Views;
using WB.Core.BoundedContexts.Capi.Views.FinishInstallation;
using WB.Core.BoundedContexts.Capi.Views.Login;
using WB.UI.Shared.Android;

namespace WB.UI.Capi
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
           // container.Add(typeof(LoginViewModel), typeof(LoginActivity));
            container.Add(typeof(FinishIntallationViewModel), typeof(FinishInstallationActivity));
        }

        protected override IMvxApplication CreateApp()
        {
            return new App();
        }
    }
}
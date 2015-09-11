using Android.App;
using Android.Content.PM;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Droid.Views;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.UI.Interviewer.Activities
{
    [Activity(NoHistory = true, MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait, Theme = "@style/AppTheme")]
    public class SplashActivity : MvxSplashScreenActivity
    {
        public SplashActivity() : base(Resource.Layout.splash)
        {
        }

        protected override void TriggerFirstNavigate()
        {
            IViewModelNavigationService viewModelNavigationService = Mvx.Resolve<IViewModelNavigationService>();

            if (Mvx.Resolve<IPrincipal>().IsAuthenticated)
            {
                viewModelNavigationService.NavigateToDashboard();
            }
            
            viewModelNavigationService.NavigateTo<LoginViewModel>();
        }
    }
}
using Android.App;
using Android.Content.PM;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Droid.Views;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.UI.Interviewer.ViewModel;

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
            IInterviewerSettings interviewerSettings = Mvx.Resolve<IInterviewerSettings>();
            IViewModelNavigationService viewModelNavigationService = Mvx.Resolve<IViewModelNavigationService>();

            if (Mvx.Resolve<IDataCollectionAuthentication>().IsLoggedIn)
            {
                viewModelNavigationService.NavigateToDashboard();
            }
            else if (interviewerSettings.GetClientRegistrationId() == null)
            {
                viewModelNavigationService.NavigateTo<FinishIntallationViewModel>();
            }
            else
            {
                viewModelNavigationService.NavigateTo<LoginActivityViewModel>();
            }
        }
    }
}
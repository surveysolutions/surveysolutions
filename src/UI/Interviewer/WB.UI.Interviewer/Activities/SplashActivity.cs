using Android.App;
using Android.Content.PM;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Widget;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Droid.Views;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.SharedKernels.Enumerator.Services;
#warning we must keep "Sqo" namespace on siaqodb as long as at least one 5.1.0-5.3.* version exist
#warning do not remove "Sqo" namespace
#warning if after all the warning you intend to remove the namespace anyway, please remove NuGet packages SiaqoDB and SiaqoDbProtable also

namespace WB.UI.Interviewer.Activities
{
    [Activity(NoHistory = true, MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait, Theme = "@style/AppTheme")]
    public class SplashActivity : MvxSplashScreenActivity
    {
        public SplashActivity() : base(Resource.Layout.splash)
        {
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            var splashAnimation = this.FindViewById<ImageView>(Resource.Id.splash_animation);
            ((AnimationDrawable)splashAnimation.Drawable).Start();
        }

        protected override async void TriggerFirstNavigate()
        {
            await Mvx.Resolve<IViewModelNavigationService>().NavigateToAsync<LoginViewModel>();
        }
    }
}
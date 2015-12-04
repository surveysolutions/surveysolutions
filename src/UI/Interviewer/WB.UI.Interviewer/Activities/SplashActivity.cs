using System;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Widget;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Droid.Views;
using Java.Lang;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.UI.Interviewer.ViewModel.Login;

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
            #warning remove this a little bit later
            //await this.BackwardCompatibilityWithPotatoidAsync();

            await Mvx.Resolve<IViewModelNavigationService>().NavigateToAsync<LoginViewModel>();
        }

        //private async Task BackwardCompatibilityWithPotatoidAsync()
        //{
        //    var newInterviewersPlainStorage = Mvx.Resolve<IAsyncPlainStorage<InterviewerIdentity>>();

        //    if (newInterviewersPlainStorage.Query(interviewers => interviewers.Any())) return;

        //    var oldUsersStorage = Mvx.Resolve<IFilterableReadSideRepositoryReader<LoginDTO>>();
        //    if (oldUsersStorage == null) return;

        //    var firstUserFromOldStorage = oldUsersStorage.Filter(oldUser => true).FirstOrDefault();
        //    if (firstUserFromOldStorage != null)
        //    {
        //        await newInterviewersPlainStorage.StoreAsync(new InterviewerIdentity()
        //        {
        //            Id = firstUserFromOldStorage.Id,
        //            UserId = Guid.Parse(firstUserFromOldStorage.Id),
        //            Password = firstUserFromOldStorage.Password,
        //            Name = firstUserFromOldStorage.Login,
        //            SupervisorId = Guid.Parse(firstUserFromOldStorage.Supervisor)
        //        });
        //    }
        //}
    }
}
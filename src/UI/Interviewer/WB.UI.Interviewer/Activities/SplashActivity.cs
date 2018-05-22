using Android.App;
using Android.Content.PM;
using MvvmCross.Droid.Support.V7.AppCompat;
using MvvmCross.Platforms.Android.Views;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities;

namespace WB.UI.Interviewer.Activities
{
    [Activity(NoHistory = true, MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait, Theme = "@style/AppTheme")]
    public class SplashActivity : MvxSplashScreenAppCompatActivity<InterviewerSetup, InterviewerMvxApplication>
    {
        public SplashActivity() : base(Resource.Layout.splash)
        {
        }

        public override void InitializationComplete()
        {
            base.InitializationComplete();
            var auditLogService = ServiceLocator.Current.GetInstance<IAuditLogService>();
            auditLogService.Write(new OpenApplicationAuditLogEntity());

        }
    }
}

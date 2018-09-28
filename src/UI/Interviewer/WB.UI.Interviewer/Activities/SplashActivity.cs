using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using MvvmCross.Droid.Support.V7.AppCompat;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.UI.Interviewer.Activities
{
    [Activity(NoHistory = true, MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait, Theme = "@style/AppTheme")]
    public class SplashActivity : MvxSplashScreenAppCompatActivity<InterviewerSetup, InterviewerMvxApplication>
    {
        public SplashActivity() : base(Resource.Layout.splash)
        {
        }

        public override async Task InitializationComplete()
        {
            await base.InitializationComplete();
            var auditLogService = ServiceLocator.Current.GetInstance<IAuditLogService>();
            auditLogService.Write(new OpenApplicationAuditLogEntity());
        }
    }
}

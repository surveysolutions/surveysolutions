using System;
using System.Threading;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.UI.Capi.Implementations.Activities;
using WB.UI.Capi.Syncronization;
using WB.UI.Shared.Android.Helpers;

namespace WB.UI.Capi
{
    [Activity(Label = "Loading", NoHistory = true,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
    public class LoadingActivity : Activity
    {
        private CancellationTokenSource cancellationToken;
        //protected ILogger logger = ServiceLocator.Current.GetInstance<ILogger>();
        private ISyncPackageApplier packageApplier = ServiceLocator.Current.GetInstance<ISyncPackageApplier>();

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            this.ActionBar.SetDisplayShowHomeEnabled(false);
            this.cancellationToken = this.WaitForLongOperation((ct) => this.Restore(ct, Guid.Parse(this.Intent.GetStringExtra("publicKey"))));
        }

        public override void OnBackPressed()
        {
            if (this.cancellationToken != null)
                this.cancellationToken.Cancel();
        }

        protected override void OnStop()
        {
            base.OnStop();
            if (this.cancellationToken != null)
                this.cancellationToken.Cancel();
        }

        protected void Restore(CancellationToken ct, Guid publicKey)
        {
            var applyingResult = this.packageApplier.CheckAndApplySyncPackage(publicKey);

            if (!applyingResult || ct.IsCancellationRequested)
            {
                this.RunOnUiThread(this.Finish);
                return;
            }

            InterviewViewModel interview = CapiApplication.LoadView<QuestionnaireScreenInput, InterviewViewModel>(
                new QuestionnaireScreenInput(publicKey));

            if (interview == null || ct.IsCancellationRequested)
            {
                this.RunOnUiThread(this.Finish);
                return;
            }

            var intent = new Intent(this, typeof (DataCollectionDetailsActivity));
            intent.PutExtra("publicKey", publicKey.ToString());
            this.StartActivity(intent);
        }
    }
}
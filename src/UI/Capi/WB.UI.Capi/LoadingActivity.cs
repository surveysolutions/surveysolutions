using System;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Microsoft.Practices.ServiceLocation;
using Ninject;
using WB.Core.BoundedContexts.Capi.Services;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.GenericSubdomains.ErrorReporting.Services.TabletInformationSender;
using WB.Core.SharedKernel.Utils;
using WB.UI.Capi.Controls;
using WB.UI.Capi.Implementations.Activities;
using WB.UI.Shared.Android.Helpers;

namespace WB.UI.Capi
{
    [Activity(Label = "Loading", NoHistory = true, ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
    public class LoadingActivity : Activity
    {
        private CancellationTokenSource cancellationToken;
        private ISyncPackageRestoreService packageRestoreService = ServiceLocator.Current.GetInstance<ISyncPackageRestoreService>();
        protected ITabletInformationSender tabletInformationSender;
        protected ITabletInformationSenderFactory tabletInformationSenderFactory;
        protected ProgressDialog progressDialog;

        protected TabletInformationReportButton btnSendTabletInfo
        {
            get { return this.FindViewById<TabletInformationReportButton>(Resource.Id.btnSendTabletInfo); }
        }
        protected TextView tvSyncResult
        {
            get { return this.FindViewById<TextView>(Resource.Id.tvSyncResult); }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            this.ActionBar.SetDisplayShowHomeEnabled(false);
            this.cancellationToken =
                this.WaitForLongOperation(
                    (ct) => this.Restore(ct, Guid.Parse(this.Intent.GetStringExtra("publicKey")),
                            this.Intent.GetBooleanExtra("createdOnClient", false)));
        }

        public override void OnBackPressed()
        {
            if (this.cancellationToken != null)
            {
                this.cancellationToken.Cancel();
                this.cancellationToken = null;
                return;
            }
            base.OnBackPressed();
        }

        protected override void OnStop()
        {
            if (this.cancellationToken != null)
            {
                this.cancellationToken.Cancel();
                this.cancellationToken = null;
            }
            base.OnStop();
        }

        protected void ShowErrorMassageToUser()
        {
            this.RunOnUiThread(() =>
            {
                this.cancellationToken = null;
                this.SetContentView(Resource.Layout.ErrorScreen);
                this.btnSendTabletInfo.Click += this.btnSendTabletInfo_Click;
                this.btnSendTabletInfo.ProcessFinished += this.btnSendTabletInfo_ProcessFinished;
                this.btnSendTabletInfo.ProcessCanceled += this.btnSendTabletInfo_ProcessCanceled;
                this.ActionBar.SetDisplayShowHomeEnabled(true);
                this.Title = Resources.GetText(Resource.String.InterviewLoadingError);
            });
        }

        protected void Restore(CancellationToken ct, Guid publicKey, bool createdOnClient)
        {
            var applyingResult = this.packageRestoreService.CheckAndApplySyncPackage(publicKey);

            if (!applyingResult || ct.IsCancellationRequested)
            {
                ShowErrorMassageToUser();
                return;
            }

            InterviewViewModel interview = CapiApplication.LoadView<QuestionnaireScreenInput, InterviewViewModel>(
                new QuestionnaireScreenInput(publicKey));

            if (interview == null || ct.IsCancellationRequested)
            {
                ShowErrorMassageToUser();
                return;
            }

            if (createdOnClient)
            {
                var intent = new Intent(this, typeof (CreateInterviewActivity));
                intent.PutExtra("publicKey", publicKey.ToString());
                intent.AddFlags(ActivityFlags.NoHistory);
                this.StartActivity(intent);
            }
            else
            {
                var intent = new Intent(this, typeof (DataCollectionDetailsActivity));
                intent.PutExtra("publicKey", publicKey.ToString());
                this.StartActivity(intent);
            }
        }

        private void btnSendTabletInfo_Click(object sender, EventArgs e)
        {
            this.tvSyncResult.Text = string.Empty;
        }

        void btnSendTabletInfo_ProcessFinished(object sender, EventArgs e)
        {
            this.tvSyncResult.Visibility = ViewStates.Visible;
            this.tvSyncResult.Text = Resources.GetText(Resource.String.InformationPackageIsSuccessfullySent) + " " + Resources.GetText(Resource.String.ThankYouForPackage);
        }

        private void btnSendTabletInfo_ProcessCanceled(object sender, EventArgs e)
        {
            this.tvSyncResult.Visibility = ViewStates.Visible;
            this.tvSyncResult.Text = Resources.GetText(Resource.String.SendingOfInformationPackageIsCanceled);
        }
    }
}
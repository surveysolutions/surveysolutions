using System;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Microsoft.Practices.ServiceLocation;
using Mono.CSharp;
using Ninject;
using WB.Core.BoundedContexts.Interviewer.ErrorReporting.Services.TabletInformationSender;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Backup;

using WB.UI.Interviewer.CustomControls;
using WB.UI.Interviewer.Implementations.Services;
using WB.UI.Shared.Enumerator.Activities;
using OperationCanceledException = System.OperationCanceledException;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace WB.UI.Interviewer.Activities
{
    [Activity(ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize, Theme = "@style/GrayAppTheme")]
    public class TroubleshootingActivity : BaseActivity<TroubleshootingViewModel>
    {
        const string ApplicationFileName = "interviewer.apk";
        const string SyncGetlatestVersion = "/api/InterviewerSync/GetLatestVersion";

        #region find for ui controls from xml

        protected TextView tvSyncResult
        {
            get { return this.FindViewById<TextView>(Resource.Id.tvSyncResult); }
        }

        protected Button btnBackup
        {
            get { return this.FindViewById<Button>(Resource.Id.btnBackup); }
        }

        protected TabletInformationReportButton btnSendTabletInfo
        {
            get { return this.FindViewById<TabletInformationReportButton>(Resource.Id.btnSendTabletInfo); }
        }

        protected Button btnRestore
        {
            get { return this.FindViewById<Button>(Resource.Id.btnRestore); }
        }

        protected LinearLayout llContainer
        {
            get { return this.FindViewById<LinearLayout>(Resource.Id.llContainer); }
        }

        protected Button btnVersion
        {
            get { return this.FindViewById<Button>(Resource.Id.btnVersion); }
        }

        protected ProgressDialog progressDialog;

        private ILogger Logger
        {
            get { return ServiceLocator.Current.GetInstance<ILogger>(); }
        }

        private INetworkService networkService
        {
            get { return ServiceLocator.Current.GetInstance<INetworkService>(); }
        }

        private IInterviewerSettings interviewerSettings
        {
            get { return ServiceLocator.Current.GetInstance<IInterviewerSettings>(); }
        }
        private ISynchronizationService synchronizationService
        {
            get { return ServiceLocator.Current.GetInstance<ISynchronizationService>(); }
        }

        private ILogger logger
        {
            get { return ServiceLocator.Current.GetInstance<ILogger>(); }
        }

        private ProgressDialog progress;

        private PendingImplementation.Operation? currentOperation;
        private IBackup backupManager;

        #endregion

        private int clickCount = 0;

        const int NUMBER_CLICK = 10;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            var toolbar = this.FindViewById<Toolbar>(Resource.Id.toolbar);
            toolbar.Title = "";
            this.SetSupportActionBar(toolbar);

            this.backupManager = InterviewerApplication.Kernel.Get<IBackup>();
            this.btnBackup.Click += this.btnBackup_Click;
            this.btnRestore.Click += this.btnRestore_Click;
            this.btnSendTabletInfo.Click += this.btnSendTabletInfo_Click;
            this.btnSendTabletInfo.ProcessFinished += this.btnSendTabletInfo_ProcessFinished;
            this.btnSendTabletInfo.ProcessCanceled += this.btnSendTabletInfo_ProcessCanceled;
            this.btnSendTabletInfo.SenderCanceled += this.btnSendTabletInfo_SenderCanceled;
            this.tvSyncResult.Click += this.tvSyncResult_Click;
            this.llContainer.Click += this.llContainer_Click;
            this.btnVersion.Click += this.btnVersion_Click;
            this.btnVersion.Text = string.Format("Version: {0}. Check for a new version.", this.interviewerSettings.GetApplicationVersionName());
        }

        private async void btnVersion_Click(object sender, EventArgs evnt)
        {
            if (!this.networkService.IsNetworkEnabled())
            {
                Toast.MakeText(this, "Network is unavailable", ToastLength.Long).Show();
                return;
            }
            this.progress = ProgressDialog.Show(this, "Checking", "Please Wait...", true, true);
            await this.CheckVersion();
        }

        private async Task CheckVersion()
        {
            bool? newVersionExists = null;
            try
            {
                var updater = new UpdateProcessor(logger: this.logger, synchronizationService: synchronizationService, interviewerSettings: this.interviewerSettings);
                newVersionExists = await updater.CheckNewVersion();
            }
            catch (Exception exc)
            {
                this.logger.Error("Error on new version check.", exc);
            }

            this.RunOnUiThread(() =>
            {
                if (this.progress != null)
                    this.progress.Dismiss();

                AlertDialog.Builder alert = new AlertDialog.Builder(this);
                alert.SetTitle("Checking for a new version");

                if (!newVersionExists.HasValue)
                {
                    alert.SetMessage("Error occurred on version check. Please, check settings or try again later.");
                }
                else if (newVersionExists.Value)
                {
                    alert.SetPositiveButton("Yes", this.btnUpdateConfirmed_Click);
                    alert.SetNegativeButton("No", this.btnUpdateDeclined_Click);
                    alert.SetMessage("New version exists. Would you like to download and update application?");
                }
                else
                {
                    alert.SetMessage("You have the latest version of application.");
                    alert.SetNegativeButton("Close", this.btnUpdateDeclined_Click);
                }

                alert.Show();
            });
        }

        private void btnUpdateDeclined_Click(object sender, DialogClickEventArgs e)
        {
        }

        private void btnUpdateConfirmed_Click(object sender, DialogClickEventArgs e)
        {
            var updater = new UpdateProcessor(logger: this.logger, synchronizationService: this.synchronizationService, interviewerSettings: this.interviewerSettings);
            this.progress = ProgressDialog.Show(this, "Downloading", "Please Wait...", true, true);

            Task.Factory.StartNew(() =>
            {
                try
                {
                    var uri = new Uri(new Uri(this.interviewerSettings.Endpoint), SyncGetlatestVersion);
                    updater.GetLatestVersion(uri, ApplicationFileName);
                    updater.StartUpdate(ApplicationFileName);
                }
                catch (Exception ex)
                {
                    this.logger.Error("Error on application update", ex);
                }
                finally
                {
                    this.RunOnUiThread(() =>
                    {
                        // hide the progress bar
                        if (this.progress != null)
                            this.progress.Dismiss();
                    });
                }
            });
        }

        void llContainer_Click(object sender, EventArgs e)
        {
            this.clickCount = 0;
        }

        private void tvSyncResult_Click(object sender, EventArgs e)
        {
            this.clickCount++;
            if (this.clickCount >= NUMBER_CLICK)
            {
                this.btnRestore.Visibility = ViewStates.Visible;
            }
        }

        private void btnRestoreConfirmed_Click(object sender, DialogClickEventArgs e)
        {
            try
            {
                this.backupManager.Restore();

                AlertDialog.Builder alert = new AlertDialog.Builder(this);
                alert.SetTitle("Success");
                alert.SetMessage("Tablet was successfully restored");
                alert.Show();
            }
            catch (Exception ex)
            {
                AlertDialog.Builder alert = new AlertDialog.Builder(this);
                alert.SetTitle("Restore Error");
                alert.SetMessage(ex.Message + " " + ex.StackTrace);
                alert.Show();
            }
        }

        private void btnRestore_Click(object sender, EventArgs e)
        {
            var alertWarningAboutRestore = this.CreateYesNoDialog(this,
               this.btnRestoreConfirmed_Click, this.btnRestoreDeclined_Click,
               this.Resources.GetString(Resource.String.Warning), message: string.Format(this.Resources.GetString(Resource.String.AreYouSureYouWantToRestore), this.backupManager.RestorePath));

            alertWarningAboutRestore.Show();
        }

        private void btnRestoreDeclined_Click(object sender, DialogClickEventArgs e)
        {
        }

        void btnBackup_Click(object sender, EventArgs e)
        {
            string path = string.Empty;
            try
            {
                path = this.backupManager.Backup();
            }
            catch (Exception exception)
            {
                this.Logger.Fatal("Error occured during Backup. ", exception);
            }

            var alert = new AlertDialog.Builder(this);
            if (string.IsNullOrWhiteSpace(path))
            {
                alert.SetTitle("Error");
                alert.SetMessage(string.Format("Something went wrong and backup failed to be created."));
            }
            else
            {
                alert.SetTitle("Success");
                alert.SetMessage(string.Format("Backup was saved to {0}", path));
            }

            alert.Show();
        }

        private void btnSendTabletInfo_Click(object sender, EventArgs e)
        {
            this.PrepareUI();
        }

        void btnSendTabletInfo_ProcessFinished(object sender, EventArgs e)
        {
            this.tvSyncResult.Text = this.Resources.GetText(Resource.String.InformationPackageIsSuccessfullySent);
        }

        private void btnSendTabletInfo_ProcessCanceled(object sender, InformationPackageCancellationEventArgs e)
        {
            this.tvSyncResult.Text = string.Format(this.Resources.GetText(Resource.String.SendingOfInformationPackageIsCanceled), e.Reason);
        }

        private void btnSendTabletInfo_SenderCanceled(object sender, EventArgs e)
        {
            this.tvSyncResult.Text = this.Resources.GetText(Resource.String.SendingOfInformationPackageIsCanceling);
        }

        protected override void OnStart()
        {
            base.OnStart();
         
            this.PrepareUI();
        }

        public AlertDialog CreateYesNoDialog(Activity activity, EventHandler<DialogClickEventArgs> yesHandler, EventHandler<DialogClickEventArgs> noHandler, string title = null, string message = null)
        {
            var builder = new AlertDialog.Builder(activity);
            builder.SetNegativeButton(this.Resources.GetString(Resource.String.No), noHandler);
            builder.SetPositiveButton(this.Resources.GetString(Resource.String.Yes), yesHandler);
            builder.SetCancelable(false);
            if (!string.IsNullOrWhiteSpace(title))
            {
                builder.SetTitle(title);
            }
            if (!string.IsNullOrWhiteSpace(message))
            {
                builder.SetMessage(message);
            }
            return builder.Create();
        }

        private void PrepareUI()
        {
            this.tvSyncResult.Text = string.Empty;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            this.llContainer.Click -= this.llContainer_Click;
            this.btnVersion.Click -= this.btnVersion_Click;

            GC.Collect();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            this.MenuInflater.Inflate(Resource.Menu.settings, menu);
            return base.OnCreateOptionsMenu(menu);
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.menu_dashboard:
                    this.ViewModel.NavigateToDashboardCommand.Execute();
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }

        protected override int ViewResourceId
        {
            get { return Resource.Layout.sync_dialog; }
        }
    }
}
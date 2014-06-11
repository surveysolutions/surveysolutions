using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using System;
using System.Text;
using System.Threading;
using CAPI.Android.Core.Model;
using CAPI.Android.Core.Model.Authorization;
using CAPI.Android.Core.Model.ViewModel.Login;
using Main.Core.Utility;
using Main.Core.View;
using Microsoft.Practices.ServiceLocation;
using Ninject;
using WB.Core.BoundedContexts.Capi.Synchronization.Synchronization.ChangeLog;
using WB.Core.BoundedContexts.Capi.Synchronization.Synchronization.SyncCacher;
using WB.Core.BoundedContexts.Capi.Synchronization.Views.Login;
using WB.Core.GenericSubdomains.Rest;
using WB.Core.GenericSubdomains.ErrorReporting.Services.TabletInformationSender;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure.Backup;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Utils;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.UI.Capi.Extensions;
using WB.UI.Capi.Settings;
using WB.UI.Capi.Syncronization;
using WB.UI.Capi.Utils;

namespace WB.UI.Capi
{
    [Activity(ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
    public class SynchronizationActivity : Activity
    {
        #region find for ui controls from xml

        protected TextView tvSyncResult
        {
            get { return this.FindViewById<TextView>(Resource.Id.tvSyncResult); }
        }

        protected Button btnSync
        {
            get { return this.FindViewById<Button>(Resource.Id.btnSync); }
        }

        protected Button btnBackup
        {
            get { return this.FindViewById<Button>(Resource.Id.btnBackup); }
        }

        protected Button btnSendTabletInfo
        {
            get { return this.FindViewById<Button>(Resource.Id.btnSendTabletInfo); }
        }

        protected Button btnRestore
        {
            get { return this.FindViewById<Button>(Resource.Id.btnRestore); }
        }

        protected LinearLayout llContainer
        {
            get { return this.FindViewById<LinearLayout>(Resource.Id.llContainer); }
        }

        protected ProgressDialog progressDialog;
        protected SynchronozationProcessor synchronizer;
        protected ITabletInformationSender tabletInformationSender;
        protected ITabletInformationSenderFactory tabletInformationSenderFactory;
        private ILogger Logger
        {
            get { return ServiceLocator.Current.GetInstance<ILogger>(); }
        }

        private Operation? currentOperation;
        private IBackup backupManager;

        #endregion

        private int clickCount = 0;

        const int NUMBER_CLICK = 10;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            this.SetContentView(Resource.Layout.sync_dialog);

            this.backupManager = CapiApplication.Kernel.Get<IBackup>();
            this.tabletInformationSenderFactory = CapiApplication.Kernel.Get<ITabletInformationSenderFactory>();
            this.btnSync.Click += this.ButtonSyncClick;
            this.btnBackup.Click += this.btnBackup_Click;
            this.btnRestore.Click += this.btnRestore_Click;
            this.btnSendTabletInfo.Click += this.btnSendTabletInfo_Click;
            this.tvSyncResult.Click += this.tvSyncResult_Click;
            this.llContainer.Click += this.llContainer_Click;
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
            AlertDialog.Builder alertWarningAboutRestore = new AlertDialog.Builder(this);
            alertWarningAboutRestore.SetTitle("Warning");
            alertWarningAboutRestore.SetMessage(
                string.Format(
                    "All current data will be erased. Are you sure you want to proceed to restore. If Yes, please make sure restore data is presented at {0}",
                    this.backupManager.RestorePath));
            alertWarningAboutRestore.SetPositiveButton("Yes", this.btnRestoreConfirmed_Click);
            alertWarningAboutRestore.SetNegativeButton("No", this.btnRestoreDeclined_Click);
            alertWarningAboutRestore.Show();
        }

        private void btnRestoreDeclined_Click(object sender, DialogClickEventArgs e)
        {
        }

        void btnBackup_Click(object sender, EventArgs e)
        {
            var path = this.backupManager.Backup();
            AlertDialog.Builder alert = new AlertDialog.Builder(this);
            alert.SetTitle("Success");
            alert.SetMessage(string.Format("Backup was saved to {0}", path));
            alert.Show();
        }

        private void btnSendTabletInfo_Click(object sender, EventArgs e)
        {
            this.ThrowExeptionIfDialogIsOpened();

            this.PreperaUI();

            tabletInformationSender =
                this.tabletInformationSenderFactory.CreateTabletInformationSender(SettingsManager.GetSyncAddressPoint(),
                    SettingsManager.GetRegistrationKey(), SettingsManager.AndroidId);

            tabletInformationSender.InformationPackageCreated += this.tabletInformationSender_InformationPackageCreated;
            tabletInformationSender.ProcessCanceled += this.tabletInformationSender_ProcessCanceled;
            tabletInformationSender.ProcessFinished += this.tabletInformationSender_ProcessFinished;
            this.CreateDialog(ProgressDialogStyle.Spinner, "Creating information package", true, tabletInformationSender_Cancel, "Information package");
            tabletInformationSender.Run();
        }

        void tabletInformationSender_ProcessFinished(object sender, EventArgs e)
        {
            this.RunOnUiThread(() =>
            {
                this.DestroyDialog();
                this.tvSyncResult.Text = "Information package is successfully sent.";
            });
            this.DestroyReportSending();
        }

        void tabletInformationSender_ProcessCanceled(object sender, EventArgs e)
        {
            this.RunOnUiThread(() =>
            {
                this.DestroyDialog();

                this.tvSyncResult.Text = "Sending of information package is canceled.";
            });
            this.DestroyReportSending();
        }

        void tabletInformationSender_InformationPackageCreated(object sender, InformationPackageEventArgs e)
        {
            var remoteCommandDoneEvent = new AutoResetEvent(false);
            
            this.RunOnUiThread(() =>
            {
                var builder = new AlertDialog.Builder(this);

                builder.SetMessage(
                    string.Format("Information package of size {0} will be sent via network. Are you sure you want to send it?",
                        FileSizeUtils.SizeSuffix(e.FileSize)));

                builder.SetPositiveButton("Yes", (s, positiveEvent) => { this.progressDialog.SetMessage("Sending information package"); remoteCommandDoneEvent.Set(); });
                builder.SetNegativeButton("No", (s, negativeEvent) =>
                {
                    this.tabletInformationSender.Cancel();
                    remoteCommandDoneEvent.Set();
                });
                builder.Show();
            });

            remoteCommandDoneEvent.WaitOne();
        }

        protected override void OnStart()
        {
            base.OnStart();
            this.CreateActionBar();
        }

        private void ButtonSyncClick(object sender, EventArgs e)
        {
            if (!NetworkHelper.IsNetworkEnabled(this))
            {
                Toast.MakeText(this, "Network is unavailable", ToastLength.Long).Show();
                return;
            }

            this.ThrowExeptionIfDialogIsOpened();

            this.PreperaUI();
            try
            {
                this.synchronizer = new SynchronozationProcessor(this, this.CreateAuthenticator(),
                    CapiApplication.Kernel.Get<IChangeLogManipulator>(), CapiApplication.Kernel.Get<IViewFactory<LoginViewInput, LoginView>>(),
                    CapiApplication.Kernel.Get<IRestServiceWrapperFactory>(), CapiApplication.Kernel.Get<IPlainQuestionnaireRepository>(), CapiApplication.Kernel.Get<ISyncCacher>());
            }
            catch (Exception ex)
            {

                Logger.Error("Error on Sync: " + ex.Message, ex);
                this.tvSyncResult.Text = ex.Message;
                return;
            }

            this.synchronizer.StatusChanged += this.synchronizer_StatusChanged;
            this.synchronizer.ProcessFinished += this.synchronizer_ProcessFinished;
            this.synchronizer.ProcessCanceling += this.synchronizer_ProcessCanceling;
            this.synchronizer.ProcessCanceled += this.synchronizer_ProcessCanceled;

            this.CreateDialog(ProgressDialogStyle.Spinner, "Initializing", false, this.progressDialog_Cancel);

            this.synchronizer.Run();
        }


        protected ISyncAuthenticator CreateAuthenticator()
        {
            var authentificator = new RestAuthenticator();
            authentificator.RequestCredentialsCallback += this.RequestCredentialsCallBack;
            return authentificator;
        }

        protected SyncCredentials? RequestCredentialsCallBack(object sender)
        {
            if (CapiApplication.Membership.IsLoggedIn)
            {
                return CapiApplication.Membership.RequestSyncCredentials();
            }

            SyncCredentials? result = null;
            bool actionCompleted = false;
            this.RunOnUiThread(
                () =>
                {
                    if (this.progressDialog != null)
                        this.progressDialog.Dismiss();
                    AlertDialog.Builder alert = new AlertDialog.Builder(this);

                    var view = this.LayoutInflater.Inflate(Resource.Layout.SyncLogin, null);
                    var teLogin = view.FindViewById<EditText>(Resource.Id.teLogin);
                    var tePassword = view.FindViewById<EditText>(Resource.Id.tePassword);
                    var btnLogin = view.FindViewById<Button>(Resource.Id.btnLogin);
                    var btnCancel = view.FindViewById<Button>(Resource.Id.btnCancel);
                    alert.SetView(view);
                    var loginDialog = alert.Show();
                    loginDialog.SetCancelable(false);

                    btnLogin.Click += (s, e) =>
                    {
                        loginDialog.Hide();
                        if (this.progressDialog != null)
                            this.progressDialog.Show();
                        result = new SyncCredentials(teLogin.Text, SimpleHash.ComputeHash(tePassword.Text));
                        actionCompleted = true;
                    };

                    btnCancel.Click += (s, e) =>
                    {
                        loginDialog.Hide();
                        actionCompleted = true;
                        this.synchronizer.Cancel();
                    };
                });
            while (!actionCompleted)
            {
                Thread.Sleep(200);
            }
            return result;
        }

        private void PreperaUI()
        {
            this.tvSyncResult.Text = string.Empty;
        }

        private void ThrowExeptionIfDialogIsOpened()
        {
            if (this.progressDialog != null)
                throw new InvalidOperationException();
        }

        private void synchronizer_ProcessFinished(object sender, EventArgs e)
        {
            this.RunOnUiThread(() =>
                {
                    this.DestroyDialog();
                    this.tvSyncResult.Text = "Sync is finished.";
                });
            this.DestroySynchronizer();
        }

        void synchronizer_ProcessCanceling(object sender, EventArgs e)
        {
            this.RunOnUiThread(() => this.CreateDialog(ProgressDialogStyle.Spinner, "Canceling....", false, null));
        }

        private void synchronizer_ProcessCanceled(object sender, SynchronizationCanceledEventArgs evt)
        {
            this.RunOnUiThread(() =>
                {
                    this.DestroyDialog();


                    if (evt.Exceptions != null || evt.Exceptions.Count > 0)
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (var exception in evt.Exceptions)
                        {
                            sb.AppendLine(exception.Message);

                            if (exception.InnerException != null)
                            {
                                sb.AppendLine(exception.InnerException.Message);

                                if (exception.InnerException.InnerException != null)
                                {
                                    sb.AppendLine(exception.InnerException.InnerException.Message);
                                }
                            }
                        }
                        this.tvSyncResult.Text = sb.ToString();
                    }
                });
            this.DestroySynchronizer();
        }

        private void DestroySynchronizer()
        {
            this.synchronizer.ProcessCanceled -= this.synchronizer_ProcessCanceled;
            this.synchronizer.ProcessFinished -= this.synchronizer_ProcessFinished;
            this.synchronizer.StatusChanged -= this.synchronizer_StatusChanged;
            this.synchronizer.ProcessCanceling -= this.synchronizer_ProcessCanceling;
            this.synchronizer = null;
        }



        private void DestroyReportSending()
        {
            tabletInformationSender.InformationPackageCreated -= this.tabletInformationSender_InformationPackageCreated;
            tabletInformationSender.ProcessCanceled -= this.tabletInformationSender_ProcessCanceled;
            tabletInformationSender.ProcessFinished -= this.tabletInformationSender_ProcessFinished;
            tabletInformationSender = null;
        }

        private void synchronizer_StatusChanged(object sender, SynchronizationEventArgs e)
        {
            this.RunOnUiThread(() =>
                {
                    var
                        messageWithPersents = e as SynchronizationEventArgsWithPercent;
                    var currentStyle = messageWithPersents == null
                                           ? ProgressDialogStyle.Spinner
                                           : ProgressDialogStyle.Horizontal;


                    if (this.currentOperation != e.OperationType)
                        this.CreateDialog(currentStyle, e.OperationTitle, e.Cancelable, this.progressDialog_Cancel);
                    else
                        this.progressDialog.SetMessage(e.OperationTitle);

                    this.currentOperation = e.OperationType;

                    if (messageWithPersents != null)
                    {
                        this.progressDialog.Progress = messageWithPersents.Percent;
                    }
                });

        }

        #region diialog manipulation

        private void CreateDialog(ProgressDialogStyle style, string message, bool cancelable, EventHandler<DialogClickEventArgs> cancelHandler, string title = "Synchronizing")
        {
            this.DestroyDialog();
            this.progressDialog = new ProgressDialog(this);

            this.progressDialog.SetTitle(title);
            this.progressDialog.SetProgressStyle(style);
            this.progressDialog.SetMessage(message);
            this.progressDialog.SetCancelable(false);

            if (cancelable)
                this.progressDialog.SetButton("Cancel",cancelHandler);

            this.progressDialog.Show();
        }

        private void progressDialog_Cancel(object sender, DialogClickEventArgs e)
        {
            this.synchronizer.Cancel();
        }

        private void tabletInformationSender_Cancel(object sender, DialogClickEventArgs e)
        {
            this.tabletInformationSender.Cancel();
        }

        private void DestroyDialog()
        {

            if (this.progressDialog == null)
                return;
            this.progressDialog.Dismiss();
            this.progressDialog.Dispose();
            this.progressDialog = null;

        }

        #endregion
    }
}
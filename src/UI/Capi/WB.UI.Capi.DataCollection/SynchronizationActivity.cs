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
using Microsoft.Practices.ServiceLocation;
using Ninject;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure.Backup;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.UI.Capi.DataCollection.Extensions;
using WB.UI.Capi.DataCollection.Syncronization;
using WB.UI.Capi.DataCollection.Utils;

namespace WB.UI.Capi.DataCollection
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
        protected ILogger logger = ServiceLocator.Current.GetInstance<ILogger>();

        #endregion

        private int clickCount = 0;

        const int NUMBER_CLICK = 10;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            this.SetContentView(Resource.Layout.sync_dialog);

            this.backupManager = CapiApplication.Kernel.Get<IBackup>();
            this.btnSync.Click += this.ButtonSyncClick;

            //btnSync.Enabled = NetworkHelper.IsNetworkEnabled(this);


            this.btnBackup.Click += this.btnBackup_Click;
            this.btnRestore.Click += this.btnRestore_Click;
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

        private void btnRestoreDeclined_Click(object sender, DialogClickEventArgs e)
        {
            
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

        void btnBackup_Click(object sender, EventArgs e)
        {

            var path = this.backupManager.Backup();
            AlertDialog.Builder alert = new AlertDialog.Builder(this);
            alert.SetTitle("Success");
            alert.SetMessage(string.Format("Backup was saved to {0}", path));
            alert.Show();
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
                                                            CapiApplication.Kernel.Get<IChangeLogManipulator>(), CapiApplication.Kernel.Get<IReadSideRepositoryReader<LoginDTO>>());
            }
            catch (Exception ex)
            {
                
                this.logger.Error("Error on Sync: " + ex.Message, ex);
                this.tvSyncResult.Text = ex.Message;
                return;
            }

            this.synchronizer.StatusChanged += this.synchronizer_StatusChanged;
            this.synchronizer.ProcessFinished += this.synchronizer_ProcessFinished;
            this.synchronizer.ProcessCanceling += this.synchronizer_ProcessCanceling;
            this.synchronizer.ProcessCanceled += this.synchronizer_ProcessCanceled;

            this.CreateDialog(ProgressDialogStyle.Spinner, "Initializing", false);

            this.synchronizer.Run();
        }



        protected ISyncAuthenticator CreateAuthenticator()
        {
            var authentificator = new RestAuthenticator();
            authentificator.RequestCredentialsCallback += RequestCredentialsCallBack;
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
            this.RunOnUiThread(() => this.CreateDialog(ProgressDialogStyle.Spinner, "Canceling....", false));
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

        private Operation? currentOperation;
        private IBackup backupManager;

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
                        this.CreateDialog(currentStyle, e.OperationTitle, e.Cancelable);
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

        private void CreateDialog(ProgressDialogStyle style, string title, bool cancelable)
        {
            this.DestroyDialog();
            this.progressDialog = new ProgressDialog(this);
            
            this.progressDialog.SetTitle("Synchronizing");
            this.progressDialog.SetProgressStyle(style);
            this.progressDialog.SetMessage(title);
            this.progressDialog.SetCancelable(false);

            if (cancelable)
                this.progressDialog.SetButton("Cancel", this.progressDialog_Cancel);

            this.progressDialog.Show();
        }

        private void progressDialog_Cancel(object sender, DialogClickEventArgs e)
        {
            this.synchronizer.Cancel();
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
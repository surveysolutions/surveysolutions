
using System.IO;
using CAPI.Android.Core.Model;
using CAPI.Android.Core.Model.Authorization;
using WB.Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using CAPI.Android.Core.Model.ChangeLog;
using CAPI.Android.Extensions;
using CAPI.Android.Syncronization;
using CAPI.Android.Utils;
using Main.Core.Utility;
using Ninject;
using WB.Core.Infrastructure.Backup;

namespace CAPI.Android
{
    [Activity(Icon = "@drawable/capi",
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
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
        #endregion

        private int clickCount = 0;

        const int NUMBER_CLICK = 10;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            this.SetContentView(Resource.Layout.sync_dialog);

            backupManager = CapiApplication.Kernel.Get<IBackup>();
            btnSync.Click += this.ButtonSyncClick;

            btnSync.Enabled = NetworkHelper.IsNetworkEnabled(this);


            btnBackup.Click += btnBackup_Click;
            btnRestore.Click += btnRestore_Click;
            tvSyncResult.Click += tvSyncResult_Click;
            llContainer.Click += llContainer_Click;
        }

        void llContainer_Click(object sender, EventArgs e)
        {
            clickCount = 0;
        }

        private void tvSyncResult_Click(object sender, EventArgs e)
        {
            clickCount++;
            if (clickCount >= NUMBER_CLICK)
            {
                btnRestore.Visibility = ViewStates.Visible;
            }
        }

        void btnRestore_Click(object sender, EventArgs e)
        {
            try
            {
                backupManager.Restore("string empty");

                AlertDialog.Builder alert = new AlertDialog.Builder(this);
                alert.SetTitle("Success");
                alert.SetMessage("Tablet was successefully restored");
                alert.Show();
            }
            catch (Exception ex)
            {
                AlertDialog.Builder alert = new AlertDialog.Builder(this);
                alert.SetTitle("Restor Error");
                alert.SetMessage(ex.Message + " " + ex.StackTrace);
                alert.Show();
            }
        }

        void btnBackup_Click(object sender, EventArgs e)
        {

            var path = backupManager.Backup();
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
            ThrowExeptionIfDialogIsOpened();

            PreperaUI();
            try
            {
                synchronizer = new SynchronozationProcessor(this, CreateAuthenticator(),
                                                            CapiApplication.Kernel.Get<IChangeLogManipulator>());
            }
            catch (Exception ex)
            {
                tvSyncResult.Text = ex.Message;
                return;
            }

            synchronizer.StatusChanged += synchronizer_StatusChanged;
            synchronizer.ProcessFinished += synchronizer_ProcessFinished;
            synchronizer.ProcessCanceling += synchronizer_ProcessCanceling;
            synchronizer.ProcessCanceled += synchronizer_ProcessCanceled;

            CreateDialog(ProgressDialogStyle.Spinner, "Initializing", false);

            synchronizer.Run();
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
                    if (progressDialog != null)
                        progressDialog.Dismiss();
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
                        if (progressDialog != null)
                            progressDialog.Show();
                        result = new SyncCredentials(teLogin.Text, SimpleHash.ComputeHash(tePassword.Text));
                        actionCompleted = true;
                    };

                    btnCancel.Click += (s, e) =>
                    {
                        loginDialog.Hide();
                        actionCompleted = true;
                        synchronizer.Cancel();
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
            tvSyncResult.Text = string.Empty;
        }

        private void ThrowExeptionIfDialogIsOpened()
        {
            if (progressDialog != null)
                throw new InvalidOperationException();
        }

        private void synchronizer_ProcessFinished(object sender, EventArgs e)
        {
            this.RunOnUiThread(DestroyDialog);
            DestroySynchronizer();
        }

        void synchronizer_ProcessCanceling(object sender, EventArgs e)
        {
            this.RunOnUiThread(() => CreateDialog(ProgressDialogStyle.Spinner, "Canceling....", false));
        }

        private void synchronizer_ProcessCanceled(object sender, SynchronizationCanceledEventArgs evt)
        {
            this.RunOnUiThread(() =>
                {
                    DestroyDialog();


                    if (evt.Exceptions != null || evt.Exceptions.Count > 0)
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (var exception in evt.Exceptions)
                        {
                            sb.AppendLine(exception.Message);
                        }
                        tvSyncResult.Text = sb.ToString();
                    }
                });
            DestroySynchronizer();
        }

        private void DestroySynchronizer()
        {
            synchronizer.ProcessCanceled -= synchronizer_ProcessCanceled;
            synchronizer.ProcessFinished -= synchronizer_ProcessFinished;
            synchronizer.StatusChanged -= synchronizer_StatusChanged;
            synchronizer.ProcessCanceling -= synchronizer_ProcessCanceling;
            synchronizer = null;
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


                    if (currentOperation != e.OperationType)
                        CreateDialog(currentStyle, e.OperationTitle, e.Cancelable);
                    else
                        progressDialog.SetMessage(e.OperationTitle);

                    currentOperation = e.OperationType;

                    if (messageWithPersents != null)
                    {
                        progressDialog.Progress = messageWithPersents.Percent;
                    }
                });

        }

        #region diialog manipulation

        private void CreateDialog(ProgressDialogStyle style, string title, bool cancelable)
        {
            DestroyDialog();
            progressDialog = new ProgressDialog(this);
            
            progressDialog.SetTitle("Synchronizing");
            progressDialog.SetProgressStyle(style);
            progressDialog.SetMessage(title);
            progressDialog.SetCancelable(false);

            if (cancelable)
                progressDialog.SetButton("Cancel", progressDialog_Cancel);

            progressDialog.Show();
        }

        private void progressDialog_Cancel(object sender, DialogClickEventArgs e)
        {
            synchronizer.Cancel();
        }

        private void DestroyDialog()
        {

            if (progressDialog == null)
                return;
            progressDialog.Dismiss();
            progressDialog.Dispose();
            progressDialog = null;

        }

        #endregion
    }
}
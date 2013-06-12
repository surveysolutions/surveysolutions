
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
using Main.Synchronization.Credentials;
using Ninject;

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

        protected ProgressDialog progressDialog;
        protected SynchronozationProcessor synchronizer;
        #endregion


        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            this.SetContentView(Resource.Layout.sync_dialog);

            var buttonSync = this.FindViewById<Button>(Resource.Id.btnSync);
            if (buttonSync != null)
            {
                buttonSync.Click += this.ButtonSyncClick;
                buttonSync.Enabled = NetworkHelper.IsNetworkEnabled(this);
            }
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
                    alert.SetView(view);
                    var loginDialog = alert.Show();
                    loginDialog.SetCancelable(false);
                    btnLogin.Click += (s, e) =>
                    {
                        loginDialog.Hide();
                        if (progressDialog != null)
                            progressDialog.Show();
                        result = new SyncCredentials(teLogin.Text, SimpleHash.ComputeHash(tePassword.Text));
                    };
                });
            while (!result.HasValue)
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
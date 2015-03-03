using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Net;
using Android.Net.Wifi;
using Android.OS;
using Android.Text;
using Android.Text.Method;
using Android.Views;
using Android.Widget;
using System;
using System.Text;
using System.Threading;
using Microsoft.Practices.ServiceLocation;
using Ninject;
using WB.Core.BoundedContexts.Capi.ChangeLog;
using WB.Core.BoundedContexts.Capi.Implementation.Authorization;
using WB.Core.BoundedContexts.Capi.Implementation.Services;
using WB.Core.BoundedContexts.Capi.Implementation.Synchronization;
using WB.Core.BoundedContexts.Capi.Services;
using WB.Core.BoundedContexts.Capi.Views.Login;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Implementation;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.Backup;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.UI.Capi.Controls;
using WB.UI.Capi.Extensions;
using WB.UI.Capi.Syncronization;
using WB.UI.Shared.Android.Extensions;

using SynchronizationEventArgs = WB.Core.BoundedContexts.Capi.Implementation.Synchronization.SynchronizationEventArgs;
using SynchronizationEventArgsWithPercent = WB.Core.BoundedContexts.Capi.Implementation.Synchronization.SynchronizationEventArgsWithPercent;

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

        protected ProgressDialog progressDialog;
        protected SynchronozationProcessor synchronizer;
        private ILogger Logger
        {
            get { return ServiceLocator.Current.GetInstance<ILogger>(); }
        }
        private INetworkService networkService
        {
            get { return ServiceLocator.Current.GetInstance<INetworkService>(); }
        }

        private Operation? currentOperation;
        private IBackup backupManager;
        private IPasswordHasher passwordHasher;

        #endregion

        private int clickCount = 0;

        const int NUMBER_CLICK = 10;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            this.SetContentView(Resource.Layout.sync_dialog);

            this.backupManager = CapiApplication.Kernel.Get<IBackup>();
            this.passwordHasher = CapiApplication.Kernel.Get<IPasswordHasher>();
            this.btnSync.Click += this.ButtonSyncClick;
            this.btnBackup.Click += this.btnBackup_Click;
            this.btnRestore.Click += this.btnRestore_Click;
            this.btnSendTabletInfo.Click += this.btnSendTabletInfo_Click;
            this.btnSendTabletInfo.ProcessFinished += this.btnSendTabletInfo_ProcessFinished;
            this.btnSendTabletInfo.ProcessCanceled += this.btnSendTabletInfo_ProcessCanceled;
            this.tvSyncResult.Click += this.tvSyncResult_Click;
            this.llContainer.Click += this.llContainer_Click;

            string login = Intent.GetStringExtra("Login");
            string passwordHash = Intent.GetStringExtra("PasswordHash");

            if (!string.IsNullOrWhiteSpace(login) && !string.IsNullOrWhiteSpace(passwordHash))
            {
                var syncCredentials = new SyncCredentials(login, passwordHash);
                bool isThisFirstTimeAuthorizationRequested = true;
                var authentificator = new RestAuthenticator();
                authentificator.RequestCredentialsCallback += sender =>
                {
                    if (isThisFirstTimeAuthorizationRequested)
                    {
                        isThisFirstTimeAuthorizationRequested = false;
                        return syncCredentials;
                    }
                    return this.RequestCredentialsCallBack(sender);
                };

                this.StartSynctionization(authentificator, (sender, args) =>
                {
                    this.RunOnUiThread(() =>
                    {
                        this.DestroyDialog();
                        this.tvSyncResult.Text = Resources.GetString(Resource.String.SyncIsFinished);
                        bool result = CapiApplication.Membership.LogOnAsync(login, passwordHash, wasPasswordHashed: true).Result;
                        if (result)
                        {
                            this.ClearAllBackStack<DashboardActivity>();
                        }
                        else
                        {
                            this.ClearAllBackStack<LoginActivity>();
                        }
                    });
                    this.DestroySynchronizer();
                });
            }
        }

        private void ButtonSyncClick(object sender, EventArgs e)
        {
            this.StartSynctionization(this.CreateAuthenticator(), synchronizer_ProcessFinished);
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
            var alertWarningAboutRestore =  this.CreateYesNoDialog(this, 
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
                Logger.Fatal("Error occured during Backup. ", exception);
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
            this.tvSyncResult.Text = Resources.GetText(Resource.String.InformationPackageIsSuccessfullySent);
        }

        private void btnSendTabletInfo_ProcessCanceled(object sender, EventArgs e)
        {
            this.tvSyncResult.Text = Resources.GetText(Resource.String.SendingOfInformationPackageIsCanceled);
        }

        protected override void OnStart()
        {
            base.OnStart();
            this.CreateActionBar();
            this.PrepareUI();
        }

        private async void StartSynctionization(ISyncAuthenticator authenticator, EventHandler synchronizerProcessFinished)
        {
            if (this.progressDialog != null) return;

            this.PrepareUI();
            try
            {
                var deviceChangeVerifier = this.CreateDeviceChangeVerifier();
                var changeLogManipulator = CapiApplication.Kernel.Get<IChangeLogManipulator>();
                var plainFileRepository = CapiApplication.Kernel.Get<IPlainInterviewFileStorage>();
                var cleaner = new CapiCleanUpService(changeLogManipulator, plainFileRepository, CapiApplication.Kernel.Get<ISyncPackageIdsStorage>());
                this.synchronizer = new SynchronozationProcessor(
                    deviceChangeVerifier,
                    authenticator,
                    new CapiDataSynchronizationService(
                        changeLogManipulator,
                        CapiApplication.Kernel.Get<ICommandService>(),
                        CapiApplication.Kernel.Get<IViewFactory<LoginViewInput, LoginView>>(),
                        CapiApplication.Kernel.Get<IPlainQuestionnaireRepository>(),
                        cleaner,
                        ServiceLocator.Current.GetInstance<ILogger>(),
                        CapiApplication.Kernel.Get<ICapiSynchronizationCacheService>(),
                        CapiApplication.Kernel.Get<IJsonUtils>(),
                        CapiApplication.Kernel.Get<IQuestionnaireAssemblyFileAccessor>()),
                    cleaner,
                    CapiApplication.Kernel.Get<IInterviewSynchronizationFileStorage>(),
                    CapiApplication.Kernel.Get<ISyncPackageIdsStorage>(),
                    CapiApplication.Kernel.Get<ILogger>(),
                    CapiApplication.Kernel.Get<ISynchronizationService>(),
                    CapiApplication.Kernel.Get<IInterviewerSettings>());
            }
            catch (Exception ex)
            {
                this.Logger.Error("Error on Sync: " + ex.Message, ex);
                this.tvSyncResult.Text = ex.Message;
                return;
            }

            this.synchronizer.StatusChanged += this.synchronizer_StatusChanged;
            this.synchronizer.ProcessFinished += synchronizerProcessFinished;
            this.synchronizer.ProcessCanceling += this.synchronizer_ProcessCanceling;
            this.synchronizer.ProcessCanceled += this.synchronizer_ProcessCanceled;

            this.CreateDialog(ProgressDialogStyle.Spinner, Resources.GetString(Resource.String.Initializing), false, this.progressDialog_Cancel);

            await this.synchronizer.Run();
        }

        protected IDeviceChangingVerifier CreateDeviceChangeVerifier()
        {
            var deviceChangeVerifier = new DeviceChangingVerifier();
            deviceChangeVerifier.ConfirmDeviceChangeCallback += this.ConfirmDeviceChangeCallback;
            return deviceChangeVerifier;
        }

        protected ISyncAuthenticator CreateAuthenticator()
        {
            var authentificator = new RestAuthenticator();
            authentificator.RequestCredentialsCallback += this.RequestCredentialsCallBack;
            return authentificator;
        }

        protected bool ConfirmDeviceChangeCallback(object sender)
        {
            bool shouldThisDeviceBeLinkedToUser = false;
            bool actionCompleted = false;
            this.RunOnUiThread(
                () =>
                {
                    if (this.progressDialog != null) this.progressDialog.Dismiss();

                    EventHandler<DialogClickEventArgs> noHandler = (s, ev) => { actionCompleted = true; this.synchronizer.Cancel(); };

                    var firstConfirmationDialog = this.CreateYesNoDialog(this, 
                        yesHandler: (s, ev) =>
                        {
                            var secondConfirmationDialog = this.CreateYesNoDialog(this, 
                                yesHandler: (s1, evnt) =>
                                {
                                    if (this.progressDialog != null) 
                                        this.progressDialog.Show();
                                    shouldThisDeviceBeLinkedToUser = true;
                                    actionCompleted = true;
                                }, noHandler: noHandler, title: this.Resources.GetString(Resource.String.ConfirmDeviceChanging), message: this.Resources.GetString(Resource.String.AllYourDataWillBeDeleted));
                            secondConfirmationDialog.Show();

                        }, noHandler: noHandler, title: this.Resources.GetString(Resource.String.ConfirmDeviceChanging), message: this.Resources.GetString(Resource.String.MakeThisTabletWorkingDevice));
                        firstConfirmationDialog.Show();
                });
            while (!actionCompleted)
            {
                Thread.Sleep(200);
            }
            return shouldThisDeviceBeLinkedToUser;
        }

        public AlertDialog CreateYesNoDialog(Activity activity, EventHandler<DialogClickEventArgs> yesHandler, EventHandler<DialogClickEventArgs> noHandler, string title = null, string message = null)
        {
            var builder = new AlertDialog.Builder(activity);
            builder.SetNegativeButton(Resources.GetString(Resource.String.No), noHandler);
            builder.SetPositiveButton(Resources.GetString(Resource.String.Yes), yesHandler);
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
                        result = new SyncCredentials(teLogin.Text, passwordHasher.Hash(tePassword.Text));
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

        private void PrepareUI()
        {
            this.tvSyncResult.Text = string.Empty;
        }

        private void synchronizer_ProcessFinished(object sender, EventArgs e)
        {
            this.RunOnUiThread(() =>
                {
                    this.DestroyDialog();
                    this.tvSyncResult.Text =  Resources.GetString(Resource.String.SyncIsFinished);
                });
            this.DestroySynchronizer();
        }

        void synchronizer_ProcessCanceling(object sender, EventArgs e)
        {
            var remoteCommandDoneEvent = new AutoResetEvent(false);
            this.RunOnUiThread(() =>
            {
                this.CreateDialog(ProgressDialogStyle.Spinner, "Canceling....", false, null);
                remoteCommandDoneEvent.Set();
            });
            remoteCommandDoneEvent.WaitOne();
        }

        private void synchronizer_ProcessCanceled(object sender, SynchronizationCanceledEventArgs evt)
        {
            var remoteCommandDoneEvent = new AutoResetEvent(false);
            this.RunOnUiThread(() =>
                {
                    this.DestroyDialog();
                    if (evt.Exceptions != null && evt.Exceptions.Count > 0)
                    {
                        var settingsManager = ServiceLocator.Current.GetInstance<IInterviewerSettings>();
                        var sb = new StringBuilder();
                        foreach (var exception in evt.Exceptions)
                        {
                            var restException = exception as RestException;
                            if (restException != null)
                            {
                                if (string.IsNullOrEmpty(restException.Message))
                                {
                                    sb.AppendLine(
                                        string.Format(
                                            Resources.GetString(Resource.String.PleaseCheckURLInSettingsFormat),
                                            settingsManager.GetSyncAddressPoint(), GetNetworkDescription(),
                                            GetNetworkStatus((int)restException.StatusCode)));
                                }
                                else
                                {
                                    sb.AppendLine(restException.Message);
                                }
                                sb.AppendLine(Resources.GetString(Resource.String.NewHtmlLine));
                                continue;
                            }
                            sb.AppendLine(exception.Message);

                            sb.AppendLine(Resources.GetString(Resource.String.NewHtmlLine));
                        }
                        tvSyncResult.MovementMethod = LinkMovementMethod.Instance;
                        this.tvSyncResult.SetText(Html.FromHtml(sb.ToString()), TextView.BufferType.Spannable);
                    }
                    remoteCommandDoneEvent.Set();
                });
            remoteCommandDoneEvent.WaitOne();
            this.DestroySynchronizer();
        }

        private string GetNetworkStatus(int status)
        {
            return string.Format(Resources.GetString(Resource.String.NetworkStatus), status);
        }

        private string GetNetworkDescription()
        {
            var connectivityManager = (ConnectivityManager)this.GetSystemService(ConnectivityService);

            var networkInfo = connectivityManager.GetNetworkInfo(ConnectivityType.Wifi);
            if (networkInfo.IsConnected)
            {
                var wifiManager = (WifiManager)this.GetSystemService(WifiService);
                var connectionInfo = wifiManager.ConnectionInfo;
                if (connectionInfo != null && !string.IsNullOrEmpty(connectionInfo.SSID))
                {
                    return string.Format(Resources.GetString(Resource.String.NowYouareConnectedToWifiNetwork), connectionInfo.SSID);
                }
            }
            var mobileState = connectivityManager.GetNetworkInfo(ConnectivityType.Mobile).GetState();
            if (mobileState == NetworkInfo.State.Connected)
            {
                return Resources.GetString(Resource.String.NowYouareConnectedToMobileNetwork);
            }
            return Resources.GetString(Resource.String.YouAreNotConnectedToAnyNetwork);
        }

        private void DestroySynchronizer()
        {
            this.synchronizer.ProcessCanceled -= this.synchronizer_ProcessCanceled;
            this.synchronizer.ProcessFinished -= this.synchronizer_ProcessFinished;
            this.synchronizer.StatusChanged -= this.synchronizer_StatusChanged;
            this.synchronizer.ProcessCanceling -= this.synchronizer_ProcessCanceling;
            this.synchronizer = null;
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
                this.progressDialog.SetButton("Cancel", cancelHandler);

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
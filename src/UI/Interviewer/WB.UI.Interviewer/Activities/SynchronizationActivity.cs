using System;
using System.Net;
using System.Threading;
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
using Cirrious.CrossCore;
using Flurl.Http;
using Microsoft.Practices.ServiceLocation;
using Ninject;
using WB.Core.BoundedContexts.Interviewer.ErrorReporting.Services.TabletInformationSender;
using WB.Core.BoundedContexts.Interviewer.Implementation.Authorization;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Implementation.Synchronization;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Backup;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.UI.Interviewer.Controls;
using WB.UI.Interviewer.Syncronization;
using WB.UI.Interviewer.ViewModel;
using WB.UI.Shared.Enumerator.Activities;
using OperationCanceledException = System.OperationCanceledException;
using SynchronizationEventArgs = WB.Core.BoundedContexts.Interviewer.Implementation.Synchronization.SynchronizationEventArgs;
using SynchronizationEventArgsWithPercent = WB.Core.BoundedContexts.Interviewer.Implementation.Synchronization.SynchronizationEventArgsWithPercent;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace WB.UI.Interviewer.Activities
{
    [Activity(ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize, Theme = "@style/GrayAppTheme")]
    public class SynchronizationActivity : BaseActivity<SynchronizationViewModel>
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
        protected SynchronizationProcessor synchronizer;
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

            var toolbar = this.FindViewById<Toolbar>(Resource.Id.toolbar);
            toolbar.Title = "";
            this.SetSupportActionBar(toolbar);

            this.backupManager = InterviewerApplication.Kernel.Get<IBackup>();
            this.passwordHasher = InterviewerApplication.Kernel.Get<IPasswordHasher>();
            this.btnSync.Click += this.ButtonSyncClick;
            this.btnBackup.Click += this.btnBackup_Click;
            this.btnRestore.Click += this.btnRestore_Click;
            this.btnSendTabletInfo.Click += this.btnSendTabletInfo_Click;
            this.btnSendTabletInfo.ProcessFinished += this.btnSendTabletInfo_ProcessFinished;
            this.btnSendTabletInfo.ProcessCanceled += this.btnSendTabletInfo_ProcessCanceled;
            this.btnSendTabletInfo.SenderCanceled += this.btnSendTabletInfo_SenderCanceled;
            this.tvSyncResult.Click += this.tvSyncResult_Click;
            this.llContainer.Click += this.llContainer_Click;
        }

        private void ButtonSyncClick(object sender, EventArgs e)
        {
            this.StartSynctionization(this.CreateAuthenticator(), this.synchronizer_ProcessFinished);
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

            var login = this.ViewModel.Login;
            var passwordHash = this.ViewModel.Password;

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
                    var wasSynchronizationSuccessfull = this.synchronizer.WasSynchronizationSuccessfull;
                    this.RunOnUiThread(() =>
                    {
                        this.DestroyDialog();
                        this.tvSyncResult.Text = this.GetFinishSynchronizationMessageByItsStatus(wasSynchronizationSuccessfull);
                        if (!wasSynchronizationSuccessfull)
                        {
                            return;
                        }

                        bool result = Mvx.Resolve<IDataCollectionAuthentication>().LogOnAsync(login, passwordHash, wasPasswordHashed: true).Result;
                        if (result)
                        {
                            ServiceLocator.Current.GetInstance<IViewModelNavigationService>().NavigateToDashboard();
                        }
                        else
                        {
                            ServiceLocator.Current.GetInstance<IViewModelNavigationService>().NavigateTo<LoginActivityViewModel>();
                        }
                    });
                    this.DestroySynchronizer();
                });
            }

            this.PrepareUI();
        }

        private async void StartSynctionization(ISyncAuthenticator authenticator, EventHandler synchronizerProcessFinished)
        {
            if (this.progressDialog != null) return;

            var interviewerSettings = InterviewerApplication.Kernel.Get<IInterviewerSettings>();

            if (!interviewerSettings.GetSyncAddressPoint().IsValidWebAddress())
            {
                this.tvSyncResult.Text = Properties.Resources.InvalidSyncPointAddressUrl;
                return;
            }

            this.PrepareUI();
            try
            {
                var deviceChangeVerifier = this.CreateDeviceChangeVerifier();

                this.synchronizer = new SynchronizationProcessor(
                    deviceChangeVerifier,
                    authenticator,
                    ServiceLocator.Current.GetInstance<CapiDataSynchronizationService>(),
                    ServiceLocator.Current.GetInstance<CapiCleanUpService>(),
                    ServiceLocator.Current.GetInstance<IInterviewSynchronizationFileStorage>(),
                    ServiceLocator.Current.GetInstance<ISyncPackageIdsStorage>(),
                    ServiceLocator.Current.GetInstance<ILogger>(),
                    ServiceLocator.Current.GetInstance<ISynchronizationService>(),
                    interviewerSettings);
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

            this.CreateDialog(ProgressDialogStyle.Spinner, this.Resources.GetString(Resource.String.Initializing), false, this.progressDialog_Cancel);

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

        protected SyncCredentials? RequestCredentialsCallBack(object sender)
        {
            if (Mvx.Resolve<IDataCollectionAuthentication>().IsLoggedIn)
            {
                return Mvx.Resolve<IDataCollectionAuthentication>().RequestSyncCredentials();
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
                        result = new SyncCredentials(teLogin.Text, this.passwordHasher.Hash(tePassword.Text));
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
            var wasSynchronizationSuccessfull = this.synchronizer.WasSynchronizationSuccessfull;
            this.RunOnUiThread(() =>
                {
                    this.DestroyDialog();
                    this.tvSyncResult.Text = this.GetFinishSynchronizationMessageByItsStatus(wasSynchronizationSuccessfull);
                });
            this.DestroySynchronizer();
        }

        private string GetFinishSynchronizationMessageByItsStatus(bool wasSynchronizationSuccessfull)
        {
            return wasSynchronizationSuccessfull
                ? this.Resources.GetString(Resource.String.SyncIsFinished)
                : this.Resources.GetString(Resource.String.SyncIsFinishedUnsuccessfully);
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
                    if (evt.Exception != null)
                    {
                        var errorMessage = this.GetUserFriendlyErrorMessage(evt.Exception);

                        this.tvSyncResult.MovementMethod = LinkMovementMethod.Instance;
                        this.tvSyncResult.SetText(Html.FromHtml(errorMessage), TextView.BufferType.Spannable);
                    }
                    remoteCommandDoneEvent.Set();
                });
            remoteCommandDoneEvent.WaitOne();
            this.DestroySynchronizer();
        }

        private string GetUserFriendlyErrorMessage(Exception exception)
        {
            var errorMessage = Properties.Resources.SynchronizationUnhandledExceptionMessage;

            var taskCancellationException = exception as OperationCanceledException;
            if (taskCancellationException != null)
            {
                errorMessage = Properties.Resources.SynchronizationCanceledExceptionMessage;
            }

            var restException = exception as RestException;
            var flurException = exception as FlurlHttpException;

            if (restException != null || flurException != null)
            {
                HttpStatusCode? statusCode =
                    restException != null
                        ? restException.StatusCode
                        : flurException.Call.Response != null
                            ? flurException.Call.Response.StatusCode
                            : null as HttpStatusCode?;

                string responseMessage =
                    flurException != null && flurException.Call.Response != null && !string.IsNullOrEmpty(flurException.Call.Response.ReasonPhrase)
                        ? flurException.Call.Response.ReasonPhrase
                        : exception.Message;

                switch (statusCode)
                {
                    case HttpStatusCode.UpgradeRequired:
                    case HttpStatusCode.Unauthorized:
                    case HttpStatusCode.NotAcceptable:
                        errorMessage = responseMessage;
                        break;
                    case HttpStatusCode.Conflict:
                        errorMessage = Properties.Resources.OldInterviewerNeedsCleanup;
                        break;
                    case HttpStatusCode.NotFound:
                    case HttpStatusCode.InternalServerError:
                        errorMessage = Properties.Resources.SynchronizationInternalServerError;
                        break;
                    case HttpStatusCode.RequestTimeout:
                        errorMessage = Properties.Resources.SynchronizationRequestTimeout;
                        break;
                    case HttpStatusCode.ServiceUnavailable:
                        errorMessage = responseMessage.Contains("maintenance")
                            ? Properties.Resources.SynchronizationMaintenance
                            : responseMessage;
                        break;
                    case null:
                        break;
                    default:
                        var settingsManager = ServiceLocator.Current.GetInstance<IInterviewerSettings>();

                        errorMessage = string.Format(Properties.Resources.PleaseCheckURLInSettingsFormat,
                            settingsManager.GetSyncAddressPoint(), this.GetNetworkDescription(),
                            this.GetNetworkStatus((int) statusCode));
                        break;
                }
            }
            return errorMessage;
        }

        private string GetNetworkStatus(int status)
        {
            return string.Format(this.Resources.GetString(Resource.String.NetworkStatus), status);
        }

        private string GetNetworkDescription()
        {
            var connectivityManager = (ConnectivityManager)this.GetSystemService(ConnectivityService);

            if (connectivityManager != null)
            {
                var networkInfoWiFi = connectivityManager.GetNetworkInfo(ConnectivityType.Wifi);
                if (networkInfoWiFi != null && networkInfoWiFi.IsConnected)
                {
                    var wifiManager = (WifiManager) this.GetSystemService(WifiService);
                    if (wifiManager != null && wifiManager.ConnectionInfo != null && !string.IsNullOrEmpty(wifiManager.ConnectionInfo.SSID))
                    {
                        return string.Format(this.Resources.GetString(Resource.String.NowYouareConnectedToWifiNetwork),
                            wifiManager.ConnectionInfo.SSID);
                    }
                }

                var mobileInfoMobile = connectivityManager.GetNetworkInfo(ConnectivityType.Mobile);
                if (mobileInfoMobile != null && mobileInfoMobile.GetState() == NetworkInfo.State.Connected)
                {
                    return this.Resources.GetString(Resource.String.NowYouareConnectedToMobileNetwork);
                }
            }
            return this.Resources.GetString(Resource.String.YouAreNotConnectedToAnyNetwork);
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

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            this.MenuInflater.Inflate(Resource.Menu.synchronization, menu);
            return base.OnCreateOptionsMenu(menu);
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.menu_settings:
                    this.ViewModel.NavigateToSettingsCommand.Execute();
                    break;
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
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SynchronizationActivity.cs" company="">
//   
// </copyright>
// <summary>
//   The synchronization activity.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using Android.Text.Method;
using CAPI.Android.Core.Model.ProjectionStorage;
using CAPI.Android.Core.Model.Syncronization;
using CAPI.Android.Core.Model.ViewModel.Dashboard;
using CAPI.Android.Syncronization;
using Main.Core.Utility;
using Main.Core.View.User;
using Main.Synchronization.Credentials;

namespace CAPI.Android
{
    using System;
    using System.IO;
    using System.Threading;

    using AndroidMain.Synchronization;

    using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
    using CAPI.Android.Extensions;
    using CAPI.Android.Settings;
    using CAPI.Android.Utils;
    using Main.DenormalizerStorage;
    using Main.Synchronization.SyncManager;
    using Main.Synchronization.SyncSreamProvider;
    using Main.Synchronization.SyncStreamCollector;

    using Newtonsoft.Json;

    using Ninject;

    using global::Android.App;
    using global::Android.Content;
    using global::Android.Content.PM;
    using global::Android.Content.Res;
    using global::Android.OS;
    using global::Android.Views;
    using global::Android.Widget;

    using Environment = global::Android.OS.Environment;

    [Activity(Icon = "@drawable/capi",
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
    public class SynchronizationActivity : Activity
    {
        #region Constants

        private const string CAPI = "Capi";

        #endregion

        #region Public Methods and Operators

        protected override void OnStart()
        {
            base.OnStart();
            this.CreateActionBar();

        }

        #endregion

        #region Methods

        /// <summary>
        ///     The on create.
        /// </summary>
        /// <param name="bundle">
        ///     The bundle.
        /// </param>
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
            
            var buttonBackup = this.FindViewById<Button>(Resource.Id.btnBackup);
            if (buttonBackup != null)
            {
                buttonBackup.Click += this.buttonBackup_Click;
            }

            var buttonRestore = this.FindViewById<Button>(Resource.Id.btnRestore);
            if (buttonRestore != null)
            {
                buttonRestore.Click += this.buttonRestore_Click;
            }
        }

        private void ButtonSyncClick(object sender, EventArgs e)
        {
            this.DoSync(PumpimgType.Sync);
        }

        /// <summary>
        ///     The on destroy.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();
            GC.Collect();
        }

        protected override void OnResume()
        {
            base.OnResume();
        }

        /// <summary>
        ///     The pull.
        /// </summary>e
        /// <param name="remoteSyncNode">
        ///     The remote Sync Node.
        /// </param>
        /// <param name="status">
        ///     The status.
        /// </param>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        private bool Backup(SyncronizationStatus status)
        {
            Guid processKey = Guid.NewGuid();
            var provider = new AllIntEventsStreamProvider();
            var collector = new CompressedStreamStreamCollector(processKey);

            bool result = this.Process(provider, collector, "Backup", status, processKey);

            if (result)
            {
                var extStorage = GetBackupDirectory();

                string filename = Path.Combine(
                    extStorage, string.Format("backup-{0:yyyy-MM-dd_hh-mm-ss-tt}.acapi", DateTime.UtcNow));

                WriteBackupToFile(filename, collector); // is was WriteExportedStreamToFile before
            }

            status.Progress = 99;
            return result;
        }

        private static void WriteBackupToFile(string filename, CompressedStreamStreamCollector collector)
        {
            using (var writer = new StreamWriter(filename))
            {
                foreach (var @event in collector.GetEventStoreForBackup())
                {
                    string serializedEvent = JsonConvert.SerializeObject(@event,
                        Formatting.None,
                        new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects });

                    writer.WriteLine(serializedEvent);
                    writer.WriteLine("===== ===== ===== ===== =====");
                }

                writer.Flush();
            }
        }

        private static void WriteExportedStreamToFile(string filename, CompressedStreamStreamCollector collector)
        {
            using (FileStream file = File.Open(filename, FileMode.Create))
            {
                var buf = new byte[1024];
                int r;
                MemoryStream stream = collector.GetExportedStream();
                while ((r = stream.Read(buf, 0, buf.Length)) > 0)
                {
                    file.Write(buf, 0, r);
                }

                file.Flush();
            }

            /*filename = Path.Combine(extStorage, string.Format("backup{0:yyyy-MM-dd_hh-mm-ss-tt}--1.acapi", DateTime.UtcNow));
            File.WriteAllBytes(filename, collector.GetExportedStream().ToArray());
*/
        }

        private static string GetBackupDirectory()
        {
            string extStorage = Environment.ExternalStorageDirectory.AbsolutePath;
            if (Directory.Exists(extStorage))
            {
                extStorage = Path.Combine(extStorage, CAPI);
                if (!Directory.Exists(extStorage))
                {
                    Directory.CreateDirectory(extStorage);
                }
            }
            else
            {
                extStorage = global::System.Environment.GetFolderPath(global::System.Environment.SpecialFolder.Personal);
            }
            return extStorage;
        }

        /// <summary>
        ///     The cancel clicked.
        /// </summary>
        /// <param name="sender">
        ///     The sender.
        /// </param>
        /// <param name="dialogClickEventArgs">
        ///     The dialog click event args.
        /// </param>
        private void CancelClicked(object sender, DialogClickEventArgs dialogClickEventArgs)
        {
            // this.context.StartActivity(this.membership.IsLoggedIn ? typeof(DashboardActivity) : typeof(LoginActivity));
        }

        private bool CheckSyncPoint()
        {
            string syncPoint = SettingsManager.GetSyncAddressPoint();
            
            if (!SettingsManager.ValidateAddress(syncPoint))
            {
                var builder = new AlertDialog.Builder(this);
                builder.SetTitle("Incorrect Address Point.");
                builder.SetMessage("Please set in settings.");
                AlertDialog dialog = builder.Create();
                dialog.Show();
                return false;
            }
            return true;
        }

        private ProgressDialog progressDialog = null;
        /// <summary>
        ///     The do sync.
        /// </summary>
        /// <param name="isPush">
        ///     The is push.
        /// </param>
        private void DoSync(PumpimgType pumpingType)
        {
            this.FindViewById<TextView>(Resource.Id.tvSyncResult).Text = string.Empty;

            if (!this.CheckSyncPoint())
            {
                this.FindViewById<TextView>(Resource.Id.tvSyncResult).Text = "Sync point is set incorrect.";
                return;
            }

            if (!NetworkHelper.IsNetworkEnabled(this))
            {
                this.FindViewById<TextView>(Resource.Id.tvSyncResult).Text = "Network is not avalable.";
                return;
            }

            this.FindViewById<TextView>(Resource.Id.tvSyncResult).Text = string.Empty;

            progressDialog = CreateDialog();
            progressDialog.Show();

            ThreadPool.QueueUserWorkItem(
                state =>
                    {
                        /*this.RunOnUiThread(
                            () =>
                                {
                                    progressDialog.Show();
                                    progressDialog.IncrementProgressBy(1);
                                });
*/
                        var result = new SyncronizationStatus();
                        int currentProgress = result.Progress;

                        ThreadPool.QueueUserWorkItem(
                            proc =>
                                {
                                    try
                                    {
                                        if (pumpingType == PumpimgType.Backup)
                                        {
                                            this.Backup(result);
                                        }
                                        else if (pumpingType == PumpimgType.Sync)
                                        {
                                            this.Push(SettingsManager.GetSyncAddressPoint(), result);

                                            if (result.Result)
                                                this.Pull(SettingsManager.GetSyncAddressPoint(), result);
                                        }
                                    }
                                    catch (Exception exc)
                                    {
                                        result.Result = false;
                                        if (string.IsNullOrWhiteSpace(result.ErrorMessage))
                                            result.ErrorMessage = "Unknown Error";
                                        //throw;
                                    }
                                    finally
                                    {
                                        result.Progress = 100;
                                        GC.Collect();
                                    }
                                });

                        while (currentProgress < 100)
                        {
                            int diff = result.Progress - currentProgress;

                            if (diff > 0)
                            {
                                this.RunOnUiThread(
                                    () =>
                                        {
                                            progressDialog.IncrementProgressBy(diff);
                                            progressDialog.SetMessage(result.CurrentStageDescription);
                                        });
                                currentProgress = result.Progress;
                            }

                            Thread.Sleep(200);
                        }

                        this.RunOnUiThread(
                            () =>
                                {
                                    var syncResult = this.FindViewById<TextView>(Resource.Id.tvSyncResult);
                                    syncResult.Text = result.Result
                                                          ? "Process is finished."
                                                          : "Error occured during the process. \r\n"
                                                            + result.ErrorMessage;
                                    progressDialog.Hide();
                                    progressDialog = null;
                                });
                    });

            //this.RequestedOrientation = ScreenOrientation.Unspecified;
        }

        private bool Process(
            ISyncEventStreamProvider provider,
            ISyncStreamCollector collector,
            string syncMessage,
            SyncronizationStatus status,
            Guid processKey)
        {
            var manager = new SyncManager(provider, collector, processKey, syncMessage, null, status);
            manager.StartPump();
            return true;
         
        }
         private ProgressDialog CreateDialog()
        {
            var progressDialogResult = new ProgressDialog(this);

            progressDialogResult.SetTitle("Synchronizing");
            progressDialogResult.SetProgressStyle(ProgressDialogStyle.Spinner);
            progressDialogResult.SetMessage("Initialyzing");
            progressDialogResult.SetCancelable(false);

            return progressDialogResult;
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

        /// <summary>
        ///     The pull.
        /// </summary>
        /// <param name="remoteSyncNode">
        ///     The remote Sync Node.
        /// </param>
        /// <param name="status">
        ///     The status.
        /// </param>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        private bool Pull(string remoteSyncNode, SyncronizationStatus status)
        {
            Guid processKey = Guid.NewGuid();
            var provider = new RemoteServiceEventStreamRestProvider(processKey, remoteSyncNode, CreateAuthenticator());
            var collector = new LocalStorageStreamCollector(CapiApplication.Kernel, processKey);

            bool result = this.Process(provider, collector, "Remote sync (Pulling)", status, processKey);
            
            status.Progress = 99;
            return result;
        }

        /// <summary>
        ///     The pull.
        /// </summary>
        /// <param name="remoteSyncNode">
        ///     The remote Sync Node.
        /// </param>
        /// <param name="status">
        ///     The status.
        /// </param>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        private bool Push(string remoteSyncNode, SyncronizationStatus status)
        {
            Guid processKey = Guid.NewGuid();
            var provider =
                new AClientEventStreamProvider(
                    CapiApplication.Kernel.Get<IFilterableDenormalizerStorage<QuestionnaireDTO>>());
            var collector = new RemoteCollector(remoteSyncNode, processKey, CreateAuthenticator());

            bool result = this.Process(provider, collector, "Remote sync (Pushing)", status, processKey);
            status.Progress = 99;
            return result;
        }

        private void buttonBackup_Click(object sender, EventArgs e)
        {
            this.DoSync(PumpimgType.Backup);
        }
        
        private void buttonRestore_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public enum PumpimgType
    {
        Push = 0,
        Pull = 1,
        Backup = 2,
        Sync = 4
    }
}
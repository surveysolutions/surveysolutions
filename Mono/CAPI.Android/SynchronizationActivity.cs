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
using CAPI.Android.Core.Model.ProjectionStorage;
using CAPI.Android.Core.Model.ViewModel.Dashboard;
using Main.Core.View.User;

namespace CAPI.Android
{
    using System;
    using System.IO;
    using System.Threading;

    using AndroidMain.Synchronization;

    using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
    using CAPI.Android.Extensions;
    using CAPI.Android.Settings;
    using CAPI.Android.Syncronization;

    using Main.DenormalizerStorage;
    using Main.Synchronization.SyncManager;
    using Main.Synchronization.SyncSreamProvider;
    using Main.Synchronization.SyncStreamCollector;

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

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
        }

        /// <summary>
        ///     The on create options menu.
        /// </summary>
        /// <param name="menu">
        ///     The menu.
        /// </param>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            this.CreateActionBar();
            return base.OnCreateOptionsMenu(menu);
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
            }

            var buttonPull = this.FindViewById<Button>(Resource.Id.btnPull);
            if (buttonPull != null)
            {
                buttonPull.Click += this.buttonPull_Click;
            }

            var buttonPush = this.FindViewById<Button>(Resource.Id.btnPush);
            if (buttonPush != null)
            {
                buttonPush.Click += this.buttonPush_Click;
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
            this.DoSync(PumpimgType.Push);
            this.DoSync(PumpimgType.Pull);
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
        private bool Backup(SyncronizationStatus status)
        {
            Guid processKey = Guid.NewGuid();
            var provider = new AllIntEventsStreamProvider();
            var collector = new CompressedStreamStreamCollector(processKey);

            bool result = this.Process(provider, collector, "Backup", status, processKey);

            if (result)
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
                    extStorage =
                        global::System.Environment.GetFolderPath(global::System.Environment.SpecialFolder.Personal);
                }

                string filename = Path.Combine(
                    extStorage, string.Format("backup-{0:yyyy-MM-dd_hh-mm-ss-tt}.acapi", DateTime.UtcNow));
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

            status.Progress = 100;
            return result;
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

            Uri test = null;
            bool valid = Uri.TryCreate(syncPoint, UriKind.Absolute, out test)
                         && (test.Scheme == "http" || test.Scheme == "https");

            if (!valid)
            {
                var builder = new AlertDialog.Builder(this);
                builder.SetTitle("Incorrect Address Point");
                builder.SetMessage("Please set in settings");
                AlertDialog dialog = builder.Create();
                dialog.Show();
                return false;
            }
            return true;
        }

        /// <summary>
        ///     The do sync.
        /// </summary>
        /// <param name="isPush">
        ///     The is push.
        /// </param>
        private void DoSync(PumpimgType pumpingType)
        {
            if (!this.CheckSyncPoint())
            {
                return;
            }

            // async task protection
            //ScreenOrientation oldOrientation = this.RequestedOrientation;
            //this.RequestedOrientation = ScreenOrientation.Nosensor;

            var progressDialog = new ProgressDialog(this);

            progressDialog.SetTitle("Synchronization process");
            progressDialog.SetProgressStyle(ProgressDialogStyle.Horizontal);
            progressDialog.SetMessage("Initialyzing");
            progressDialog.SetCancelable(false);

            this.FindViewById<TextView>(Resource.Id.tvSyncResult).Text = string.Empty;

            int currentProgress = 0;
            ThreadPool.QueueUserWorkItem(
                state =>
                    {
                        this.RunOnUiThread(
                            () =>
                                {
                                    progressDialog.Show();
                                    progressDialog.IncrementProgressBy(1);
                                });

                        var result = new SyncronizationStatus();
                        currentProgress = result.Progress;

                        ThreadPool.QueueUserWorkItem(
                            proc =>
                                {
                                    try
                                    {
                                        if (pumpingType == PumpimgType.Push)
                                        {
                                            this.Push(SettingsManager.GetSyncAddressPoint(), result);
                                        }
                                        else if (pumpingType == PumpimgType.Pull)
                                        {
                                            this.Pull(SettingsManager.GetSyncAddressPoint(), result);
                                        }
                                        else if (pumpingType == PumpimgType.Backup)
                                        {
                                            this.Backup(result);
                                        }
                                    }
                                    catch (Exception exc)
                                    {
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
                                    string infoMess = result.Result
                                                          ? "Process is finished"
                                                          : "Error occured during the process. \r\n"
                                                            + result.ErrorMessage;

                                    syncResult.Text = infoMess + result.ProgressLog;
                                    progressDialog.Hide();
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
            try
            {
                var manager = new SyncManager(provider, collector, processKey, syncMessage, null, status);
                manager.StartPump();
                CapiApplication.SaveProjections();
                return true;
            }
            catch (Exception ex)
            {
                // log
                status.IsWorking = false;
                return false;
            }
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
            var provider = new RemoteServiceEventStreamRestProvider(CapiApplication.Kernel, processKey, remoteSyncNode);
            var collector = new LocalStorageStreamCollector(CapiApplication.Kernel, processKey);

            bool result = this.Process(provider, collector, "Remote sync (Pulling)", status, processKey);

            status.ProgressLog = provider.ProcessLogging;
            status.Progress = 100;
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
                    CapiApplication.Kernel.Get<IDenormalizerStorage<CompleteQuestionnaireView>>());
            var collector = new RemoteCollector(remoteSyncNode, processKey);

            bool result = this.Process(provider, collector, "Remote sync (Pushing)", status, processKey);
            status.Progress = 100;
            return result;
        }

        private void buttonBackup_Click(object sender, EventArgs e)
        {
            this.DoSync(PumpimgType.Backup);
        }

        /// <summary>
        ///     The button pull_ click.
        /// </summary>
        /// <param name="sender">
        ///     The sender.
        /// </param>
        /// <param name="e">
        ///     The e.
        /// </param>
        private void buttonPull_Click(object sender, EventArgs e)
        {
            return;
            this.DoSync(PumpimgType.Pull);
        }

        /// <summary>
        ///     The button push_ click.
        /// </summary>
        /// <param name="sender">
        ///     The sender.
        /// </param>
        /// <param name="e">
        ///     The e.
        /// </param>
        private void buttonPush_Click(object sender, EventArgs e)
        {
            return;
            this.DoSync(PumpimgType.Push);
        }

        private void buttonRestore_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public enum PumpimgType
    {
        Push,

        Pull,

        Backup
    }
}
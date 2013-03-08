// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SynchronizationActivity.cs" company="">
//   
// </copyright>
// <summary>
//   The synchronization activity.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace AndroidApp
{
    using System;
    using System.IO;
    using System.Threading;

    using Android.App;
    using Android.Content;
    using Android.Content.PM;
    using Android.OS;
    using Android.Views;
    using Android.Widget;

    using AndroidApp.Core.Model.ViewModel.QuestionnaireDetails;
    using AndroidApp.Extensions;
    using AndroidApp.Settings;
    using AndroidApp.Syncronization;

    using AndroidMain.Synchronization;

    using Main.DenormalizerStorage;
    using Main.Synchronization.SyncManager;
    using Main.Synchronization.SyncSreamProvider;
    using Main.Synchronization.SyncStreamCollector;

    using Ninject;

    /// <summary>
    /// The synchronization activity.
    /// </summary>
    [Activity(/*NoHistory = true, */Icon = "@drawable/capi")]
    public class SynchronizationActivity : Activity
    {
        #region Public Methods and Operators

        /// <summary>
        /// The on create options menu.
        /// </summary>
        /// <param name="menu">
        /// The menu.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            this.CreateActionBar();
            return base.OnCreateOptionsMenu(menu);
        }

        #endregion

        #region Methods

        /// <summary>
        /// The on create.
        /// </summary>
        /// <param name="bundle">
        /// The bundle.
        /// </param>
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            
            this.SetContentView(Resource.Layout.sync_dialog);

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

        private void buttonRestore_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void buttonBackup_Click(object sender, EventArgs e)
        {
            this.DoSync(PumpimgType.Backup);
        }

        protected override void OnResume()
        {
            base.OnResume();
        }


        /// <summary>
        /// The on destroy.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();
            GC.Collect();
        }

        /// <summary>
        /// The cancel clicked.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="dialogClickEventArgs">
        /// The dialog click event args.
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
        /// The do sync.
        /// </summary>
        /// <param name="isPush">
        /// The is push.
        /// </param>
        private void DoSync(PumpimgType pumpingType)
        {
            if (!CheckSyncPoint())
                return;

            // async task protection
            var oldOrientation = this.RequestedOrientation; 
            this.RequestedOrientation = ScreenOrientation.Nosensor;

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
                                    catch (Exception)
                                    {
                                        //throw;
                                    }
                                    
                                });

                        while (result.IsWorking && currentProgress < 100)
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
                                                          ? "Process is finished"
                                                          : "Error occured during the process. \r\n" + result.ErrorMessage;
                                    progressDialog.Hide();
                                });
                    });

            this.RequestedOrientation = ScreenOrientation.Sensor;

        }

        /// <summary>
        /// The pull.
        /// </summary>
        /// <param name="remoteSyncNode">
        /// The remote Sync Node.
        /// </param>
        /// <param name="status">
        /// The status.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool Pull(string remoteSyncNode, SyncronizationStatus status)
        {
            Guid processKey = Guid.NewGuid();
            var provider = new RemoteServiceEventStreamProvider1(
                    CapiApplication.Kernel, processKey, remoteSyncNode);
                var collector = new LocalStorageStreamCollector(CapiApplication.Kernel, processKey);
            
            return Process(provider, collector, "Remote sync (Pulling)", status, processKey);
        }


        /// <summary>
        /// The pull.
        /// </summary>
        /// <param name="remoteSyncNode">
        /// The remote Sync Node.
        /// </param>
        /// <param name="status">
        /// The status.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool Backup(SyncronizationStatus status)
        {
            Guid processKey = Guid.NewGuid();
            var provider = new AllIntEventsStreamProvider();
            var collector = new CompressedStreamStreamCollector(processKey);

            bool result = Process(provider, collector, "Backup", status, processKey);

            if (result)
            {

                var extStorage = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
                if (Directory.Exists(extStorage))
                {
                    extStorage = System.IO.Path.Combine(extStorage, CAPI);
                    if (!Directory.Exists(extStorage))
                    {
                        Directory.CreateDirectory(extStorage);
                    }
                }
                else
                {
                    extStorage = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                    
                }

                var filename = Path.Combine(
                    extStorage, string.Format("backup{0:yyyy-MM-dd_hh-mm-ss-tt}.acapi", DateTime.UtcNow));
                File.WriteAllBytes(filename, collector.GetExportedStream().ToArray());
            }
            return result;
        }

        private const string CAPI = "Capi";
        
        /// <summary>
        /// The pull.
        /// </summary>
        /// <param name="remoteSyncNode">
        /// The remote Sync Node.
        /// </param>
        /// <param name="status">
        /// The status.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool Push(string remoteSyncNode, SyncronizationStatus status)
        {
            Guid processKey = Guid.NewGuid();
            var provider = new AClientEventStreamProvider(
                        CapiApplication.Kernel.Get<IDenormalizerStorage<CompleteQuestionnaireView>>());
            var collector = new RemoteCollector(remoteSyncNode, processKey);
            
            return Process(provider, collector, "Remote sync (Pushing)", status, processKey);
        }


        private bool Process(ISyncEventStreamProvider provider, ISyncStreamCollector collector, string syncMessage, SyncronizationStatus status, Guid processKey)
        {
            try
            {
                var manager = new SyncManager(provider, collector, processKey, syncMessage, null, status);
                manager.StartPump();
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
        /// The button pull_ click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void buttonPull_Click(object sender, EventArgs e)
        {
            this.DoSync(PumpimgType.Pull);
        }

        /// <summary>
        /// The button push_ click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void buttonPush_Click(object sender, EventArgs e)
        {
            this.DoSync(PumpimgType.Push);
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
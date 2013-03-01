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

        /// <summary>
        /// The do sync.
        /// </summary>
        /// <param name="isPush">
        /// The is push.
        /// </param>
        private void DoSync(bool isPush)
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
                return;
            }

            // async task protection
            var oldOrientation = this.RequestedOrientation; 
            this.RequestedOrientation = ScreenOrientation.Nosensor;

            var progressDialog = new ProgressDialog(this);

            progressDialog.SetTitle("Synchronization status");
            progressDialog.SetProgressStyle(ProgressDialogStyle.Horizontal);
            progressDialog.SetMessage("Starting");
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
                                    if (isPush)
                                    {
                                        this.Push(syncPoint, result);
                                    }
                                    else
                                    {
                                        this.Pull(syncPoint, result);
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
                                                          ? "Synchronization is successfully finished "
                                                          : "Synchronization error. \r\n" + result.ErrorMessage;
                                    progressDialog.Hide();
                                });
                    });

            this.RequestedOrientation = oldOrientation;

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

            const string SyncMessage = "Remote sync (Pulling).";
            try
            {
                var streamProvider = new RemoteServiceEventStreamProvider1(
                    CapiApplication.Kernel, processKey, remoteSyncNode);
                var collector = new LocalStorageStreamCollector(CapiApplication.Kernel, processKey);

                var manager = new SyncManager(streamProvider, collector, processKey, SyncMessage, null, status);
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

            const string SyncMessage = "Remote sync (Pushing).";
            try
            {
                var streamProvider =
                    new AClientEventStreamProvider(
                        CapiApplication.Kernel.Get<IDenormalizerStorage<CompleteQuestionnaireView>>());
                var collector = new RemoteCollector(remoteSyncNode, processKey);

                var manager = new SyncManager(streamProvider, collector, processKey, SyncMessage, null, status);
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
            this.DoSync(false);
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
            this.DoSync(true);
        }

        #endregion
    }
}
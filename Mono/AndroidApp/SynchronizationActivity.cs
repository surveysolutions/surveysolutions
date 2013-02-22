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

    using Ncqrs;
    using Ncqrs.Eventing.Storage;

    using Ninject;

    /// <summary>
    /// The synchronization activity.
    /// </summary>
    [Activity(NoHistory = true, Icon = "@drawable/capi")]
    public class SynchronizationActivity : Activity
    {
        
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
            var syncPoint = SettingsManager.GetSyncAddressPoint();

            Uri test = null;
            bool valid = Uri.TryCreate(syncPoint, UriKind.Absolute, out test)
                         && (test.Scheme == "http" || test.Scheme == "https");
            
            if (!valid)
            {
                AlertDialog.Builder builder = new AlertDialog.Builder(this);
                builder.SetTitle("Incorrect Address Point");
                builder.SetMessage("Please set in settings");
                var dialog = builder.Create();
                dialog.Show();
                return;
            }
            
            var progressDialog = new ProgressDialog(this);

            progressDialog.SetMessage("Synchronization in progress");
            progressDialog.SetProgressStyle(ProgressDialogStyle.Horizontal);
            progressDialog.SetCancelable(false);
            

            this.FindViewById<TextView>(Resource.Id.tvSyncResult).Text = string.Empty;

            int i = 0;
            ThreadPool.QueueUserWorkItem(
                state =>
                    {
                        i++;
                        this.RunOnUiThread(() =>
                            {
                                progressDialog.Show();
                                progressDialog.IncrementProgressBy(1);
                            });

                        bool result = isPush ? this.Push(syncPoint) : this.Pull(syncPoint);



                        this.RunOnUiThread(
                            () =>
                                {
                                    progressDialog.IncrementProgressBy(98);
                                    var syncResult = this.FindViewById<TextView>(Resource.Id.tvSyncResult);
                                    syncResult.Text = result ? "OK" : "Error on sync!";
                                });
                        RunOnUiThread(progressDialog.Hide);
                    });
        }

        /// <summary>
        /// The pull.
        /// </summary>
        /// <param name="remoteSyncNode">
        /// The remote Sync Node.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool Pull(string remoteSyncNode)
        {
            Guid processKey = Guid.NewGuid();

            const string SyncMessage = "Remote sync (Pulling).";
            try
            {
                var streamProvider = new RemoteServiceEventStreamProvider1(
                    CapiApplication.Kernel, processKey, remoteSyncNode);
                var collector = new LocalStorageStreamCollector(CapiApplication.Kernel, processKey);

                var manager = new SyncManager(streamProvider, collector, processKey, SyncMessage, null);
                manager.StartPush();
                return true;
            }
            catch (Exception ex)
            {
                // log
                return false;
            }
        }

        /// <summary>
        /// The pull.
        /// </summary>
        /// <param name="remoteSyncNode">
        /// The remote Sync Node.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool Push(string remoteSyncNode)
        {
            Guid processKey = Guid.NewGuid();

            const string SyncMessage = "Remote sync (Pushing).";
            try
            {
                var streamProvider =
                    new AClientEventStreamProvider(CapiApplication.Kernel.Get<IDenormalizerStorage<CompleteQuestionnaireView>>());
                var collector = new RemoteCollector(remoteSyncNode, processKey);

                var manager = new SyncManager(streamProvider, collector, processKey, SyncMessage, null);
                manager.StartPush();
                return true;
            }
            catch (Exception ex)
            {
                // log
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
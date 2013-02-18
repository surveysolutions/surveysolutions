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
    using Android.Graphics;
    using Android.OS;
    using Android.Widget;

    using AndroidMain.Synchronization;

    using Main.Synchronization.SyncManager;
    using Main.Synchronization.SyncSreamProvider;
    using Main.Synchronization.SyncStreamCollector;

    /// <summary>
    /// The synchronization activity.
    /// </summary>
    [Activity(Label = "Synchronization", Icon = "@drawable/capi")]
    public class SynchronizationActivity : Activity
    {

        private const string remoteSyncNode = 
            //"http://192.168.173.1:8084/";
            "http://10.0.2.2:8084";  
        // "http://217.12.197.135/DEV-Supervisor/";

        #region Public Methods and Operators

        /*/// <summary>
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
        }*/

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

            var syncPoint = this.FindViewById<EditText>(Resource.Id.editSyncPoint);
            syncPoint.Text = remoteSyncNode;
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
        /// The pull.
        /// </summary>
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
                //log
                return false;
            }
        }

        /// <summary>
        /// The pull.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool Push(string remoteSyncNode)
        {
            Guid processKey = Guid.NewGuid();

            const string SyncMessage = "Remote sync (Pushing).";
            try
            {
                var streamProvider = new AllIntEventsStreamProvider();
                var collector = new RemoteCollector(remoteSyncNode, processKey);

                var manager = new SyncManager(streamProvider, collector, processKey, SyncMessage, null);
                manager.StartPush();
                return true;
            }
            catch (Exception ex)
            {
                //log
                return false;
            }
        }


        private void DoSync(bool isPush)
        {
            var syncPoint = this.FindViewById<EditText>(Resource.Id.editSyncPoint);
            
            Uri test = null;
            bool valid = Uri.TryCreate(syncPoint.Text, UriKind.Absolute, out test) && (test.Scheme == "http" || test.Scheme == "https");

            if (!valid)
            {
                syncPoint.SetBackgroundColor(Color.Red);
                return;
            }

            syncPoint.SetBackgroundColor(Color.Transparent);

            var progressDialog = new ProgressDialog(this);

            progressDialog.SetMessage("Synchronization in progress");
            progressDialog.SetProgressStyle(ProgressDialogStyle.Horizontal);
            progressDialog.SetCancelable(false);
            progressDialog.Show();

            int i = 0;
            ThreadPool.QueueUserWorkItem(
                state =>
                {
                    i++;
                    this.RunOnUiThread(delegate { progressDialog.IncrementProgressBy(1); });

                    bool result = isPush ? this.Push(syncPoint.Text) : this.Pull(syncPoint.Text);

                    this.RunOnUiThread(
                            delegate
                            {
                                progressDialog.IncrementProgressBy(98);
                                var syncResult = this.FindViewById<TextView>(Resource.Id.tvSyncResult);
                                syncResult.Text = result ? "OK" : "Error on sync!";
                            });
                    RunOnUiThread(progressDialog.Hide);
                });
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
using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;

namespace AndroidApp
{
    using System.Threading;

    using AndroidMain.Synchronization;

    using Main.Synchronization.SyncManager;
    using Main.Synchronization.SyncStreamCollector;

    [Activity(Label = "Synchronization", Icon = "@drawable/capi")]
    public class SynchronizationActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            
            
            /*syncServiceIntent = new Intent("org.worldbank.capi.sync");
            syncReceiver = new SyncReceiver();*/

            SetContentView(Resource.Layout.sync_dialog);

            var buttonPull = FindViewById<Button>(Resource.Id.btnPull);
            if (buttonPull != null)
            {
                buttonPull.Click += buttonPull_Click;
            }

            var buttonPush = FindViewById<Button>(Resource.Id.btnPush);
            if (buttonPush != null)
            {
                buttonPush.Click += buttonPush_Click;
            }

        }

        private void buttonPush_Click(object sender, EventArgs e)
        {
        }

        private void CancelClicked(object sender, DialogClickEventArgs dialogClickEventArgs)
        {
            //this.context.StartActivity(this.membership.IsLoggedIn ? typeof(DashboardActivity) : typeof(LoginActivity));
        }

        private void buttonPull_Click(object sender, EventArgs e)
        {
            /*var builder = new AlertDialog.Builder(this);
            builder.SetMessage("Synchronization in progress");
            builder.SetNegativeButton("Stop", CancelClicked);
            builder.Create();
            builder.SetCancelable(false);
            builder.Show();*/

            var progressDialog = new ProgressDialog(this);

            progressDialog.SetMessage("Synchronization in progress");
            progressDialog.SetProgressStyle(ProgressDialogStyle.Horizontal);
            
            progressDialog.SetCancelable(false);
            progressDialog.Show();


            int i = 0;
            ThreadPool.QueueUserWorkItem(state =>
                {
                    i++;
                    RunOnUiThread(
                        delegate
                            {
                                progressDialog.IncrementProgressBy(1);
                            });

                    this.Pull();

                    RunOnUiThread(
                        delegate
                        {
                           progressDialog.IncrementProgressBy(98);
                        });
                    
                    /*while (i<100)
                    {
                        RunOnUiThread(
                        delegate
                        {
                            //progressDialog.SetProgressStyle(ProgressDialogStyle.Horizontal);
                            progressDialog.IncrementProgressBy(1);
                        });
                    }*/

                    RunOnUiThread(progressDialog.Hide);
                });
        }
        
        private bool Pull()
        {
            Guid processKey = Guid.NewGuid();
            string remoteSyncNode = "http://217.12.197.135/DEV-Supervisor/";
            string syncMessage = "Remote sync.";
            try
            {
                var streamProvider = new RemoteServiceEventStreamProvider1(CapiApplication.Kernel, processKey, remoteSyncNode);
                var collector = new LocalStorageStreamCollector(CapiApplication.Kernel, processKey);

                var manager = new SyncManager(streamProvider, collector, processKey, syncMessage, null);
                manager.StartPush();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
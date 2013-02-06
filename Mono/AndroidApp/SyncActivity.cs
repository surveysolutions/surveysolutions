
using Android.App;
using Android.OS;

namespace AndroidApp
{
    using System;

    using Android.Content;
    using Android.Widget;

    using AndroidApp.Extensions;
    using AndroidApp.Services;
    using AndroidApp.ViewModel.Synchronization;

    using Cirrious.MvvmCross.Binding.Droid.Simple;
    using Cirrious.MvvmCross.Droid.Views;

    [Activity(Label = "Synchronization", Icon = "@drawable/capi")]
    public class SyncActivity : MvxSimpleBindingActivity<SyncViewModel>
    {
        bool isBound = false;

        private SyncServiceBinder syncServiceBinder;
        private SyncServiceConnection stockServiceConnection;

        SyncReceiver syncReceiver;
        Intent syncServiceIntent;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            syncServiceIntent = new Intent("org.worldbank.capi.sync");
            syncReceiver = new SyncReceiver();

            SetContentView(Resource.Layout.sync_dialog);

            Button buttonRun = FindViewById<Button>(Resource.Id.btnSync);
            buttonRun.Click += buttonSync_Click;

        }

        void buttonSync_Click(object sender, EventArgs e)
        {
            var builder = new AlertDialog.Builder(this);

            //builder.SetTitle("Synchronization");
            builder.SetMessage("Synchronization started");
            //builder.SetPositiveButton("Sync", OkClicked);
            builder.SetNegativeButton("Cancel", CancelClicked);
            builder.Create();
            builder.Show();
        }

        private void CancelClicked(object sender, DialogClickEventArgs dialogClickEventArgs)
        {
            //this.context.StartActivity(this.membership.IsLoggedIn ? typeof(DashboardActivity) : typeof(LoginActivity));
        }

        private void OkClicked(object sender, DialogClickEventArgs dialogClickEventArgs)
        {
            var dialog = sender as AlertDialog;

            if (null != dialog)
            {
                /*var connectionEdit = dialog.FindViewById(Resource.Id.connectionstring_edit) as EditText;

                if (null != connectionEdit)
                    Console.WriteLine("Connection String: {0}", connectionEdit.Text);*/
            }
        }

        protected override void OnStart()
        {
            base.OnStart();

            var intentFilter = new IntentFilter(SyncService.SyncFinishededAction) { Priority = (int)IntentFilterPriority.HighPriority };
            RegisterReceiver(syncReceiver, intentFilter);

            stockServiceConnection = new SyncServiceConnection(this);
            BindService(syncServiceIntent, stockServiceConnection, Bind.AutoCreate);
        }

        protected override void OnStop()
        {
            base.OnStop();

            if (isBound)
            {
                UnbindService(stockServiceConnection);

                isBound = false;
            }

            UnregisterReceiver(syncReceiver);
        }

        public override bool OnCreateOptionsMenu(Android.Views.IMenu menu)
        {
            this.CreateActionBar();
            return base.OnCreateOptionsMenu(menu);
        }


        void DoSync()
        {
            if (isBound)
            {
                RunOnUiThread(() =>
                {
                    var stocks = syncServiceBinder.GetSyncService().GetResult();

                    /*if (stocks != null)
                    {
                        ListAdapter = new ArrayAdapter<Stock>(
                        this,
                        Resource.Layout.StockItemView,
                        stocks
                        );
                    }
                    else
                    {
                        Log.Debug("StockService", "stocks is null");
                    }*/
                }
                );
            }
        }

        class SyncReceiver : BroadcastReceiver
        {
            public override void OnReceive(Context context, Android.Content.Intent intent)
            {
                ((SyncActivity)context).DoSync();

                InvokeAbortBroadcast();
            }
        }

        class SyncServiceConnection : Java.Lang.Object, IServiceConnection
        {
            SyncActivity activity;

            public SyncServiceConnection(SyncActivity activity)
            {
                this.activity = activity;
            }

            public void OnServiceConnected(ComponentName name, IBinder service)
            {
                var syncServiceBinder = service as SyncServiceBinder;
                if (syncServiceBinder != null)
                {
                    var binder = (SyncServiceBinder)service;
                    activity.syncServiceBinder = binder;
                    activity.isBound = true;
                }
            }

            public void OnServiceDisconnected(ComponentName name)
            {
                activity.isBound = false;
            }
        }


    }
}
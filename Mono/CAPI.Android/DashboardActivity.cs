using System;
using System.Threading;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using CAPI.Android.Controls;
using CAPI.Android.Core.Model.ViewModel.Dashboard;
using CAPI.Android.Extensions;
using Cirrious.MvvmCross.Binding.Droid.Simple;

namespace CAPI.Android
{
    using global::Android.Content.PM;

    [Activity(Label = "CAPI", Icon = "@drawable/capi",
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
    public class
        DashboardActivity : ListActivity
    {
        protected override void OnCreate(Bundle bundle)
        {

            base.OnCreate(bundle);
            if (this.FinishIfNotLoggedIn())
                return;
           /* RequestData(() =>
                {*/
            ListAdapter = new DashboardAdapter(this, CapiApplication.Membership.CurrentUser.Id);
                               
           /*         this.RunOnUiThread(() =>
                                       SetContentView(Resource.Layout.Main));
                });*/

        }

        private void RequestData(Action restore)
        {
            var progress = new ProgressDialog(this);
            progress.SetTitle("Loading");
            progress.SetProgressStyle(ProgressDialogStyle.Spinner);
            progress.SetCancelable(false);
            progress.Show();

            ThreadPool.QueueUserWorkItem((s) => { restore(); progress.Dismiss(); });

            
        }

        protected override void OnRestart()
        {
            base.OnRestart();

            RequestData(
                () => (ListAdapter as DashboardAdapter).Update());
        }


        protected override void OnStart()
        {
            base.OnStart();
            this.CreateActionBar();

        }

        public override bool OnCreateOptionsMenu(global::Android.Views.IMenu menu)
        {
            //this.CreateActionBar();
            return base.OnCreateOptionsMenu(menu);
        }
    }
}


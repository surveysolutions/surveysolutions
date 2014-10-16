using System;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using Android.Content.PM;
using Cirrious.MvvmCross.Droid.Views;
using Main.Core.Utility;
using WB.Core.BoundedContexts.Capi.Views.FinishInstallation;
using WB.UI.Capi.Settings;

namespace WB.UI.Capi
{
    [Activity(ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
    public class FinishInstallationActivity : MvxActivity
    {
        protected EditText SyncPointInput
        {
            get { return this.FindViewById<EditText>(Resource.Id.syncEndpoint); }
        }

        protected Button StartSynchronizationButton
        {
            get { return this.FindViewById<Button>(Resource.Id.startSynchronization); }
        }

        protected override void OnCreate(Bundle bundle)
        {
            this.DataContext = new FinishIntallationViewModel();
            base.OnCreate(bundle);
            this.SetContentView(Resource.Layout.FinishInstallation);
            this.StartSynchronizationButton.Click += this.StartSynchronization;

#if DEBUG
            this.SyncPointInput.Text = "http://192.168.173.1/headquarters";
#endif
        }

        private void StartSynchronization(object sender, EventArgs e)
        {
            var dataModel = this.DataContext as FinishIntallationViewModel;
            if (dataModel == null) return;

            if (!SettingsManager.SetSyncAddressPoint(dataModel.SyncEndpoint))
            {
                this.SyncPointInput.SetBackgroundColor(Color.Red);
                return;
            }

            var syncActivity = new Intent(this, typeof(SynchronizationActivity));
            syncActivity.PutExtra("Login", dataModel.Login);
            syncActivity.PutExtra("PasswordHash", SimpleHash.ComputeHash(dataModel.Password));
            this.StartActivity(syncActivity);
        }
    }
}
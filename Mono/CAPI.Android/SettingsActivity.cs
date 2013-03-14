using System;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using CAPI.Android.Extensions;
using CAPI.Android.Settings;

namespace CAPI.Android
{
    [Activity(/*NoHistory = true,*/ Icon = "@drawable/capi")]
    public class SettingsActivity : Activity
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

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            this.SetContentView(Resource.Layout.settings_dialog);

            var buttonChange = this.FindViewById<Button>(Resource.Id.btnSyncPoint);
            if (buttonChange != null)
            {
                buttonChange.Click += this.buttonChange_Click;
            }

            var editSettingsSync = this.FindViewById<EditText>(Resource.Id.editSettingsSyncPoint);
            if (editSettingsSync != null)
            {
                editSettingsSync.Text = SettingsManager.GetSyncAddressPoint();
            }


            var buttonCollect = this.FindViewById<Button>(Resource.Id.btnCollect);
            if (buttonCollect != null)
            {
                buttonCollect.Click += this.buttonCollect_Click;
            }
            var buttonCollectMajor = this.FindViewById<Button>(Resource.Id.btnCollectMajor);
            if (buttonCollectMajor != null)
            {
                buttonCollectMajor.Click += this.buttonCollectMajor_Click;
            }

        }

        private void buttonCollectMajor_Click(object sender, EventArgs e)
        {
            GC.Collect(GC.MaxGeneration);
            GC.Collect(GC.MaxGeneration);
        }

        private void buttonCollect_Click(object sender, EventArgs e)
        {
            GC.Collect(0);
            GC.Collect(0);
        }

        private void buttonChange_Click(object sender, EventArgs e)
        {
            var editSettingsSync = this.FindViewById<EditText>(Resource.Id.editSettingsSyncPoint);
            if (editSettingsSync != null)
            {
               if(SettingsManager.SetSyncAddressPoint(editSettingsSync.Text))
               {
                   editSettingsSync.SetBackgroundColor(Color.Green);
               }
               else
               {
                   editSettingsSync.SetBackgroundColor(Color.Red);
               }
            }
        }
    }
}
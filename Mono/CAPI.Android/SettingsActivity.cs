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
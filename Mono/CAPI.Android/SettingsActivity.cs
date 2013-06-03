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
    using global::Android.Content.PM;

    [Activity(Icon = "@drawable/capi", ConfigurationChanges = ConfigChanges.Orientation |
                               ConfigChanges.KeyboardHidden |
                                ConfigChanges.ScreenSize)]
    public class SettingsActivity : Activity
    {
        protected override void OnStart()
        {
            base.OnStart();
            this.CreateActionBar();

        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            this.SetContentView(Resource.Layout.settings_dialog);

            buttonChange.Click += this.buttonChange_Click;
            editSettingsSync.Text = SettingsManager.GetSyncAddressPoint();
            buttonCollect.Click += this.buttonCollect_Click;
            buttonCollectMajor.Click += this.buttonCollectMajor_Click;
            textSyncPoint.Click += textSyncPoint_Click;
            llContainer.Click += llContainer_Click;

            textMem.Click += textMem_Click;

            textMem.Text = GetResourceUsage();

        }

        private void textMem_Click(object sender, EventArgs e)
        {
            textMem.Text = GetResourceUsage();
        }

        private string GetResourceUsage()
        {
            return String.Format("[{0}] [{1}] [{2}]",
                GC.GetTotalMemory(false),
                GC.GetGeneration(GC.MaxGeneration - 1),
                AppDomain.CurrentDomain.GetAssemblies().Length);
        }


        void llContainer_Click(object sender, EventArgs e)
        {
            clickCount = 0;
        }

        private void textSyncPoint_Click(object sender, EventArgs e)
        {
            clickCount++;
            if (clickCount >= NUMBER_CLICK)
            {
                editSettingsSync.Enabled = true;
                buttonCollectMajor.Visibility = buttonCollect.Visibility = buttonChange.Visibility = ViewStates.Visible;
            }
        }

        private int clickCount = 0;
        const int NUMBER_CLICK=10;

        protected Button buttonChange
        {
            get { return this.FindViewById<Button>(Resource.Id.btnSyncPoint); }
        }
        protected Button buttonCollect
        {
            get { return this.FindViewById<Button>(Resource.Id.btnCollect); }
        }
        protected Button buttonCollectMajor
        {
            get { return this.FindViewById<Button>(Resource.Id.btnCollectMajor); }
        }
        protected TextView textSyncPoint {
            get { return this.FindViewById<TextView>(Resource.Id.textSyncPoint); }
        }

        protected TextView textMem
        {
            get { return this.FindViewById<TextView>(Resource.Id.textMem); }
        }
        protected EditText editSettingsSync
        {
            get { return this.FindViewById<EditText>(Resource.Id.editSettingsSyncPoint); }
        }
        protected LinearLayout llContainer {
            get { return this.FindViewById<LinearLayout>(Resource.Id.llContainer); }
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
                if (SettingsManager.SetSyncAddressPoint(editSettingsSync.Text))
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
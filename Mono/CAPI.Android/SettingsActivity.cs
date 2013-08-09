using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using CAPI.Android.Extensions;
using CAPI.Android.Settings;
using CAPI.Android.Syncronization.Update;
using Microsoft.Practices.ServiceLocation;
using WB.Core.GenericSubdomains.Logging;

namespace CAPI.Android
{
    using global::Android.Content.PM;

    [Activity(Icon = "@drawable/capi", ConfigurationChanges = ConfigChanges.Orientation |
                               ConfigChanges.KeyboardHidden |
                                ConfigChanges.ScreenSize)]
    public class SettingsActivity : Activity
    {
        private ProgressDialog progress;
        protected EventHandler<EventArgs> versionCheckEventHandler;

        protected ILogger logger = ServiceLocator.Current.GetInstance<ILogger>();

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

            btnVersion.Click += this.btnVersion_Click;
            btnVersion.Text = string.Format("Version: {0}. Check for a new version.", SettingsManager.AppVersionName());

            textMem.Click += textMem_Click;
            textMem.Text = GetResourceUsage();
        }

        private void textMem_Click(object sender, EventArgs e)
        {
            textMem.Text = GetResourceUsage();
        }

        private string GetResourceUsage()
        {
            return String.Format("[{0}] [{1}]",
                GC.GetTotalMemory(false),
                AppDomain.CurrentDomain.GetAssemblies().Length);
        }


        void llContainer_Click(object sender, EventArgs e)
        {
            clickCount = 0;
        }

        private void btnVersion_Click(object sender, EventArgs evnt)
        {
            progress = ProgressDialog.Show(this, "Checking", "Please Wait...", true, true);
            Task.Factory.StartNew(CheckVersion);
        }

        private void CheckVersion()
        {
            bool? newVersionExists = null;
            try
            {
                var updater = new UpdateProcessor();
                updater.CheckNewVersion();
            }
            catch (Exception exc)
            {
                logger.Error("Error on version check.", exc);
            }

            RunOnUiThread(() =>
                {
                    if (progress != null)
                        progress.Dismiss();

                    AlertDialog.Builder alert = new AlertDialog.Builder(this);

                    alert.SetTitle("Check for new version");
                    if (!newVersionExists.HasValue)
                    {
                        alert.SetMessage("Error occured on version check. Please, check settings or try again later.");
                        alert.Show();
                        return;
                    }

                    if (newVersionExists.Value)
                    {
                        alert.SetPositiveButton("Yes", btnRestoreConfirmed_Click);
                        alert.SetNegativeButton("No", btnRestoreDeclined_Click);
                        alert.SetMessage("New version exists. Would you like to update?");
                    }
                    else
                    {
                        alert.SetMessage("You have latest version");
                        alert.SetNegativeButton("Close", btnRestoreDeclined_Click);
                    }

                    alert.Show();
                });
        }

        private void btnRestoreDeclined_Click(object sender, DialogClickEventArgs e)
        {
        }

        private void btnRestoreConfirmed_Click(object sender, DialogClickEventArgs e)
        {
            var fileName = "wbcapi.apk";
            var updater = new UpdateProcessor();

            progress = ProgressDialog.Show(this, "Checking", "Please Wait...", true, true);

            Task.Factory.StartNew(() => 
            {
                try
                {
                    updater.GetLatestVersion(SettingsManager.GetSyncAddressPoint() + "/Sync/GetLatestVersion", fileName);
                    updater.StartUpdate(fileName);
                }
                catch (Exception ex)
                {
                    logger.Error("Error on application update", ex);
                }
                
                RunOnUiThread(() =>
                    {
                        // hide the progress bar
                        if (progress != null)
                            progress.Dismiss();
                    });
            });
        }
        
        protected override void OnPause()
        {
            base.OnPause();
            
            if (progress != null)
                progress.Dismiss();
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

        protected Button btnVersion
        {
            get { return this.FindViewById<Button>(Resource.Id.btnVersion); }
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

        protected override void OnDestroy()
        {
            base.OnDestroy();

            buttonChange.Click -= this.buttonChange_Click;
            buttonCollect.Click -= this.buttonCollect_Click;
            buttonCollectMajor.Click -= this.buttonCollectMajor_Click;
            textSyncPoint.Click -= textSyncPoint_Click;
            llContainer.Click -= llContainer_Click;
            textMem.Click -= textMem_Click;

            btnVersion.Click -= this.btnVersion_Click;

            GC.Collect();
        }
    }
}
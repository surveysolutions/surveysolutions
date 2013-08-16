using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using CAPI.Android.Extensions;
using CAPI.Android.GeolocationServices;
using CAPI.Android.Settings;
using CAPI.Android.Syncronization.Update;
using Microsoft.Practices.ServiceLocation;
using WB.Core.GenericSubdomains.Logging;
using Xamarin.Geolocation;

namespace CAPI.Android
{
    using global::Android.Content.PM;

    [Activity(Icon = "@drawable/capi", ConfigurationChanges = ConfigChanges.Orientation |
                               ConfigChanges.KeyboardHidden |
                                ConfigChanges.ScreenSize)]
    public class SettingsActivity : Activity
    {
        private ProgressDialog progress;
        private GeoService geoservice;
        private CancellationTokenSource cancelSource;

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

            btnWhereAmI.Click += btnWhereAmI_Click;

            geoservice = new GeoService(this);

            textMem.Text = GetResourceUsage();
        }

        private void btnWhereAmI_Click(object sender, EventArgs e)
        {
            if (geoservice == null)
                geoservice = new GeoService(this);

            if (!this.geoservice.IsGeolocationAvailable || !this.geoservice.IsGeolocationEnabled)
            {
                Toast.MakeText(this, "Geolocation is unavailable", ToastLength.Long).Show();
                return;
            }
            
            progress = ProgressDialog.Show(this, "Determining location", "Please Wait...", true, true);
            progress.CancelEvent += progressCoordinates_Cancel;
            
            Task.Factory.StartNew(GetLocation);
        }

        private void progressCoordinates_Cancel(object sender, EventArgs e)
        {
            CancellationTokenSource cancel = this.cancelSource;
            if (cancel != null)
                cancel.Cancel();
        }
        
        private void GetLocation()
        {
            cancelSource = new CancellationTokenSource();
            geoservice.GetPositionAsync(20000, cancelSource.Token).ContinueWith(t => RunOnUiThread(() =>
                {
                    if (progress != null)
                        progress.Dismiss();

                    string messageToShow;
                    
                    if (t.IsCanceled)
                        messageToShow = "Canceled or Timeout.";
                    else if (t.IsFaulted)
                        messageToShow = "Error occured on location retrieving. " + ((GeolocationException)t.Exception.InnerException).Error.ToString();
                    else
                    {
                        StringBuilder infoMessageBuilder = new StringBuilder();
                        string format = "{0} : {1}";
                        infoMessageBuilder.AppendLine(String.Format(format, "Accuracy", t.Result.Accuracy.ToString("N2") + "m"));
                        infoMessageBuilder.AppendLine(String.Format(format, "Latitude", t.Result.Latitude.ToString("N4")));
                        infoMessageBuilder.AppendLine(String.Format(format, "Longitude", t.Result.Longitude.ToString("N4")));
                        //infoMessageBuilder.AppendLine(String.Format(format, "Altitude", t.Result.Altitude.ToString("N4")));

                        infoMessageBuilder.AppendLine(String.Format(format, "Time", t.Result.Timestamp.ToString("G")));

                        messageToShow = infoMessageBuilder.ToString();
                    }

                    textWhereAmI.Text = messageToShow;
                }));
        }

        private string GetResourceUsage()
        {
            return String.Format("[{0}] [{1}]",
                (GC.GetTotalMemory(false)/1024).ToString("F2"),
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
                newVersionExists = updater.CheckNewVersion();
            }
            catch (Exception exc)
            {
                logger.Error("Error on new version check.", exc);
            }

            RunOnUiThread(() =>
                {
                    if (progress != null)
                        progress.Dismiss();

                    AlertDialog.Builder alert = new AlertDialog.Builder(this);
                    alert.SetTitle("Checking for a new version");

                    if (!newVersionExists.HasValue)
                    {
                        alert.SetMessage("Error occured on version check. Please, check settings or try again later.");
                    }
                    else if (newVersionExists.Value)
                    {
                        alert.SetPositiveButton("Yes", btnUpdateConfirmed_Click);
                        alert.SetNegativeButton("No", btnUpdateDeclined_Click);
                        alert.SetMessage("New version exists. Would you like to download and update application?");
                    }
                    else
                    {
                        alert.SetMessage("You have the latest version of application.");
                        alert.SetNegativeButton("Close", btnUpdateDeclined_Click);
                    }

                    alert.Show();
                });
        }

        private void btnUpdateDeclined_Click(object sender, DialogClickEventArgs e)
        {
        }

        private void btnUpdateConfirmed_Click(object sender, DialogClickEventArgs e)
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

            if(geoservice.IsListening)
                geoservice.StopListening();
        }

        private void textSyncPoint_Click(object sender, EventArgs e)
        {
            clickCount++;
            if (clickCount >= NUMBER_CLICK)
            {
                editSettingsSync.Enabled = true;
                buttonCollectMajor.Visibility = 
                    buttonCollect.Visibility = 
                    buttonChange.Visibility = 
                    textMem.Visibility =
                    ViewStates.Visible;
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

        protected TextView textWhereAmI
        {
            get { return this.FindViewById<TextView>(Resource.Id.textWhereAmI); }
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

        protected Button btnWhereAmI
        {
            get { return this.FindViewById<Button>(Resource.Id.btnWhereAmI); }
        }

        private void buttonCollectMajor_Click(object sender, EventArgs e)
        {
            GC.Collect(GC.MaxGeneration);
            GC.Collect(GC.MaxGeneration);

            textMem.Text = GetResourceUsage();
        }

        private void buttonCollect_Click(object sender, EventArgs e)
        {
            GC.Collect(0);
            GC.Collect(0);

            textMem.Text = GetResourceUsage();
        }

        private void buttonChange_Click(object sender, EventArgs e)
        {
            var editSettingsSync = this.FindViewById<EditText>(Resource.Id.editSettingsSyncPoint);
            if (editSettingsSync != null)
            {
                if (SettingsManager.SetSyncAddressPoint(editSettingsSync.Text))
                {
                    editSettingsSync.SetBackgroundColor(Color.LightGreen);
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
            btnVersion.Click -= this.btnVersion_Click;
            
            GC.Collect();
        }
    }
}
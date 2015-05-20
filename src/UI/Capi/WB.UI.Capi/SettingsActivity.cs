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
using Android.Content.PM;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Capi.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.UI.Capi.Extensions;
using WB.UI.Capi.Syncronization.Update;
using WB.UI.Shared.Android.GeolocationServices;
using Xamarin.Geolocation;

namespace WB.UI.Capi
{
    [Activity(ConfigurationChanges = ConfigChanges.Orientation |
                               ConfigChanges.KeyboardHidden |
                                ConfigChanges.ScreenSize)]
    public class SettingsActivity : Activity
    {
        private ProgressDialog progress;
        private IGeoService geoservice;
        private CancellationTokenSource cancelSource;

        protected EventHandler<EventArgs> versionCheckEventHandler;

        private ILogger logger
        {
            get { return ServiceLocator.Current.GetInstance<ILogger>(); }
        }

        private IInterviewerSettings interviewerSettings
        {
            get { return ServiceLocator.Current.GetInstance<IInterviewerSettings>(); }
        }

        private ISynchronizationService synchronizationService
        {
            get { return ServiceLocator.Current.GetInstance<ISynchronizationService>(); }
        }

        private INetworkService networkService
        {
            get { return ServiceLocator.Current.GetInstance<INetworkService>(); }
        }

        const string ApplicationFileName = "interviewer.apk";
        const string SyncGetlatestVersion = "/api/InterviewerSync/GetLatestVersion";

        protected override void OnStart()
        {
            base.OnStart();
            this.CreateActionBar();
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            this.SetContentView(Resource.Layout.settings_dialog);

            this.buttonChange.Click += this.buttonChange_Click;
            this.buttonCollect.Click += this.buttonCollect_Click;
            this.buttonCollectMajor.Click += this.buttonCollectMajor_Click;
            this.textSyncPoint.Click += this.textSyncPoint_Click;
            this.llContainer.Click += this.llContainer_Click;
            this.btnWhereAmI.Click += this.btnWhereAmI_Click;
            this.btnVersion.Click += this.btnVersion_Click;
            this.btnVersion.Text = string.Format("Version: {0}. Check for a new version.", interviewerSettings.GetApplicationVersionName());
            
            this.geoservice = new GeoService(this);
            this.editSettingsSync.Text = interviewerSettings.GetSyncAddressPoint();
            this.textMem.Text = this.GetResourceUsage();
        }

        private void btnWhereAmI_Click(object sender, EventArgs e)
        {
            if (this.geoservice == null)
                this.geoservice = new GeoService(this);

            if (!this.geoservice.IsGeolocationAvailable || !this.geoservice.IsGeolocationEnabled)
            {
                Toast.MakeText(this, "Geo location is unavailable", ToastLength.Long).Show();
                return;
            }
            
            this.progress = ProgressDialog.Show(this, "Determining location", "Please Wait...", true, true);
            this.progress.CancelEvent += this.progressCoordinates_Cancel;
            
            Task.Factory.StartNew(this.GetLocation);
        }

        private void progressCoordinates_Cancel(object sender, EventArgs e)
        {
            CancellationTokenSource cancel = this.cancelSource;
            if (cancel != null)
                cancel.Cancel();
        }
        
        private void GetLocation()
        {
            this.cancelSource = new CancellationTokenSource();
            this.geoservice.GetPositionAsync(300000, this.cancelSource.Token).ContinueWith((Task<Position> t) => this.RunOnUiThread(() =>
                {
                    if (this.progress != null)
                        this.progress.Dismiss();

                    string messageToShow;
                    
                    if (t.IsCanceled)
                        messageToShow = "Canceled or Timeout.";
                    else if (t.IsFaulted)
                    {
                        messageToShow = "Error occurred on location retrieving. ";
                        if (t.Exception != null && t.Exception.InnerException != null)
                        {
                            var innerException = t.Exception.InnerException as GeolocationException;
                            if (innerException != null)
                                messageToShow += innerException.Error.ToString();
                        }
                    }
                    else
                    {
                        StringBuilder infoMessageBuilder = new StringBuilder();
                        string format = "{0} : {1}";
                        
                        infoMessageBuilder.AppendLine(String.Format(format, "Latitude", t.Result.Latitude.ToString("N4")));
                        infoMessageBuilder.AppendLine(String.Format(format, "Longitude", t.Result.Longitude.ToString("N4")));
                        infoMessageBuilder.AppendLine(String.Format(format, "Accuracy", t.Result.Accuracy.ToString("N2") + "m"));
                        infoMessageBuilder.AppendLine(String.Format(format, "Altitude", t.Result.Altitude.ToString("N2")) + "m");
                        
                        infoMessageBuilder.AppendLine(String.Format(format, "Time: ", t.Result.Timestamp.ToLocalTime().ToString("G")));

                        messageToShow = infoMessageBuilder.ToString();
                    }

                    this.textWhereAmI.Text = messageToShow;
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
            this.clickCount = 0;
        }

        private void btnVersion_Click(object sender, EventArgs evnt)
        {
            if (!this.networkService.IsNetworkEnabled())
            {
                Toast.MakeText(this, "Network is unavailable", ToastLength.Long).Show();
                return;
            }
            this.progress = ProgressDialog.Show(this, "Checking", "Please Wait...", true, true);
            Task.Factory.StartNew(this.CheckVersion);
        }

        private void CheckVersion()
        {
            bool? newVersionExists = null;
            try
            {
                var updater = new UpdateProcessor(logger: this.logger, synchronizationService: this.synchronizationService);
                newVersionExists = updater.CheckNewVersion();
            }
            catch (Exception exc)
            {
                this.logger.Error("Error on new version check.", exc);
            }

            this.RunOnUiThread(() =>
                {
                    if (this.progress != null)
                        this.progress.Dismiss();

                    AlertDialog.Builder alert = new AlertDialog.Builder(this);
                    alert.SetTitle("Checking for a new version");

                    if (!newVersionExists.HasValue)
                    {
                        alert.SetMessage("Error occurred on version check. Please, check settings or try again later.");
                    }
                    else if (newVersionExists.Value)
                    {
                        alert.SetPositiveButton("Yes", this.btnUpdateConfirmed_Click);
                        alert.SetNegativeButton("No", this.btnUpdateDeclined_Click);
                        alert.SetMessage("New version exists. Would you like to download and update application?");
                    }
                    else
                    {
                        alert.SetMessage("You have the latest version of application.");
                        alert.SetNegativeButton("Close", this.btnUpdateDeclined_Click);
                    }

                    alert.Show();
                });
        }

        private void btnUpdateDeclined_Click(object sender, DialogClickEventArgs e)
        {
        }

        private void btnUpdateConfirmed_Click(object sender, DialogClickEventArgs e)
        {
            var updater = new UpdateProcessor(logger: this.logger, synchronizationService: this.synchronizationService);
            this.progress = ProgressDialog.Show(this, "Downloading", "Please Wait...", true, true);

            Task.Factory.StartNew(() => 
            {
                try
                {
                    var uri = new Uri(new Uri(interviewerSettings.GetSyncAddressPoint()), SyncGetlatestVersion);
                    updater.GetLatestVersion(uri, ApplicationFileName);
                    updater.StartUpdate(ApplicationFileName);
                }
                catch (Exception ex)
                {
                    this.logger.Error("Error on application update", ex);
                }
                finally
                {
                    this.RunOnUiThread(() =>
                    {
                        // hide the progress bar
                        if (this.progress != null)
                            this.progress.Dismiss();
                    });
                }
            });
        }
        
        protected override void OnPause()
        {
            base.OnPause();
            
            if (this.progress != null)
                this.progress.Dismiss();

            if(this.geoservice.IsListening)
                this.geoservice.StopListening();
        }

        private void textSyncPoint_Click(object sender, EventArgs e)
        {
            this.clickCount++;
            if (this.clickCount >= NUMBER_CLICK)
            {
                this.editSettingsSync.Enabled = true;
                this.buttonCollectMajor.Visibility = 
                    this.buttonCollect.Visibility = 
                    this.buttonChange.Visibility = 
                    this.textMem.Visibility =
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

            this.textMem.Text = this.GetResourceUsage();
        }

        private void buttonCollect_Click(object sender, EventArgs e)
        {
            GC.Collect(0);
            GC.Collect(0);

            this.textMem.Text = this.GetResourceUsage();
        }

        private void buttonChange_Click(object sender, EventArgs e)
        {
            var editSettingsSync = this.FindViewById<EditText>(Resource.Id.editSettingsSyncPoint);
            if (editSettingsSync != null)
            {
                try
                {
                    interviewerSettings.SetSyncAddressPoint(editSettingsSync.Text);
                    editSettingsSync.SetBackgroundColor(Color.LightGreen);
                }
                catch(ArgumentException)
                {
                    editSettingsSync.SetBackgroundColor(Color.Red);
                }
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            this.buttonChange.Click -= this.buttonChange_Click;
            this.buttonCollect.Click -= this.buttonCollect_Click;
            this.buttonCollectMajor.Click -= this.buttonCollectMajor_Click;
            this.textSyncPoint.Click -= this.textSyncPoint_Click;
            this.llContainer.Click -= this.llContainer_Click;
            this.btnVersion.Click -= this.btnVersion_Click;
            
            GC.Collect();
        }
    }
}
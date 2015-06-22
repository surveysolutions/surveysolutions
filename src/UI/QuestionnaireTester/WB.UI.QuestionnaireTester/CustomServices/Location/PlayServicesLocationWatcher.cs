using System;
using System.Threading;
using Android.App;
using Android.Content;
using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.Gms.Location;
using Android.OS;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Droid;
using Cirrious.CrossCore.Droid.Platform;
using Cirrious.CrossCore.Exceptions;
using Cirrious.CrossCore.Platform;
using Cirrious.MvvmCross.Plugins.Location;

namespace WB.UI.QuestionnaireTester.CustomServices.Location
{
    public class PlayServicesLocationWatcher
        : MvxLocationWatcher
        , IMvxLocationReceiver
        , IPlayConnectionCallbacksReceiver
        , IPlayConnectionFailedReceiver
    {
        private Context context;
        private IGoogleApiClient googleApiClient;
        private LocationRequest locationRequest;
        private readonly PlayConnectionCallbacksListener connectionCallbacksListener;
        private readonly PlayConnectionFailedListener connectionFailedListener;
        private readonly MvxLocationListener mvxLocationListener;

        private Context Context
        {
            get
            {
                return this.context ?? (this.context = Mvx.Resolve<IMvxAndroidGlobals>().ApplicationContext);
            }
        }

        public PlayServicesLocationWatcher()
        {
            this.EnsureStopped();

            this.connectionCallbacksListener = new PlayConnectionCallbacksListener(this);
            this.connectionFailedListener = new PlayConnectionFailedListener(this);
            this.mvxLocationListener = new MvxLocationListener(this);
        }

        public override MvxGeoLocation CurrentLocation
        {
            get
            {
                if (this.googleApiClient == null || this.locationRequest == null)
                    throw new MvxException("Location Client not started");

                //var androidLocation = this._locationClient.LastLocation;
                var androidLocation = LocationServices.FusedLocationApi.GetLastLocation(this.googleApiClient);
                return androidLocation == null ? null : CreateLocation(androidLocation);
            }
        }

        protected override void PlatformSpecificStart(MvxLocationOptions options)
        {
            if (this.googleApiClient != null)
                throw new MvxException("You cannot start MvxLocation service more than once");

            if (GooglePlayServicesUtil.IsGooglePlayServicesAvailable(this.Context) != ConnectionResult.Success)
                throw new MvxException("Google Play Services are not available");

            this.locationRequest = LocationRequest.Create();
            this.locationRequest.SetInterval((long)options.TimeBetweenUpdates.TotalMilliseconds);
            this.locationRequest.SetSmallestDisplacement(options.MovementThresholdInM);
            this.locationRequest.SetFastestInterval(1000);

            this.locationRequest.SetPriority(options.Accuracy == MvxLocationAccuracy.Fine
                ? LocationRequest.PriorityHighAccuracy
                : LocationRequest.PriorityBalancedPowerAccuracy);

            //this._locationClient = new LocationClient(this.Context, this._connectionCallBacks, this._connectionFailed);
            this.googleApiClient = new GoogleApiClientBuilder(Context)
                                    .AddApi(LocationServices.Api)
                                    .AddConnectionCallbacks(this.connectionCallbacksListener)
                                    .AddOnConnectionFailedListener(this.connectionFailedListener)
                                    .Build();
                
            this.googleApiClient.Connect();
        }

        protected override void PlatformSpecificStop()
        {
            this.EnsureStopped();
        }

        private void EnsureStopped()
        {
            if (this.googleApiClient == null) return;

            //this._locationClient.RemoveLocationUpdates(this._locationListener);
            LocationServices.FusedLocationApi.RemoveLocationUpdates(this.googleApiClient, this.mvxLocationListener);
            this.googleApiClient.Disconnect();
            this.googleApiClient = null;
            this.locationRequest = null;
        }

        private static MvxGeoLocation CreateLocation(Android.Locations.Location androidLocation)
        {
            var position = new MvxGeoLocation {Timestamp = androidLocation.Time.FromMillisecondsUnixTimeToUtc()};
            var coords = position.Coordinates;

            if (androidLocation.HasAltitude)
                coords.Altitude = androidLocation.Altitude;

            if (androidLocation.HasBearing)
                coords.Heading = androidLocation.Bearing;

            coords.Latitude = androidLocation.Latitude;
            coords.Longitude = androidLocation.Longitude;
            if (androidLocation.HasSpeed)
                coords.Speed = androidLocation.Speed;
            if (androidLocation.HasAccuracy)
            {
                coords.Accuracy = androidLocation.Accuracy;
            }

            return position;
        }

        public void OnLocationChanged(Android.Locations.Location androidLocation)
        {
            if (androidLocation == null)
            {
                MvxTrace.Trace("Android: Null location seen");
                return;
            }

            if (AlmostEqual(androidLocation.Latitude, double.MaxValue)
                || AlmostEqual(androidLocation.Longitude, double.MaxValue))
            {
                MvxTrace.Trace("Android: Invalid location seen");
                return;
            }

            MvxGeoLocation location;
            try
            {
                location = CreateLocation(androidLocation);
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                MvxTrace.Trace("Android: Exception seen in converting location " + ex.ToLongString());
                return;
            }

            this.SendLocation(location);
        }

        public static bool AlmostEqual(double a, double b)
        {
            return Math.Abs(a - b) < Math.Abs(a)*0.000001; //10 cm precision at equator
        }

        public void OnConnected(Bundle p0)
        {
            //this._locationClient.RequestLocationUpdates(this._locationRequest, this._locationListener);
            LocationServices.FusedLocationApi.RequestLocationUpdates(
                this.googleApiClient, 
                this.locationRequest,
                this.mvxLocationListener);
        }

        public void OnConnectionSuspended(int cause)
        {
            //TODO
        }

        public void OnConnectionFailed(ConnectionResult p0)
        {
            if (p0.HasResolution)
            {
                try
                {
                    Mvx.TaggedTrace("OnConnectionFailed()", "Launching Resolution for ConnectionResult with ErrorCode: {0}", p0.ErrorCode.ToString());
                    var intent = new Intent();
                    var receiver = new ConnectionFailedPendingIntentReceiver();
                    receiver.ResultOk += ok => {
                        if (ok)
                        { 
                            this.googleApiClient.Connect();
                        }
                    };
                    p0.Resolution.Send(this.Context, 0, intent, receiver, null);
                }
                catch (PendingIntent.CanceledException ex)
                {
                    Mvx.TaggedTrace("OnConnectionFailed()", "Resolution for ConnectionResult Cancelled! Exception: {0}", ex);
                }
            }
        }

        public class ConnectionFailedPendingIntentReceiver 
            : Java.Lang.Object
            , PendingIntent.IOnFinished
        {
            public Action<bool> ResultOk;

            public void OnSendFinished(PendingIntent pendingIntent, Intent intent, Result resultCode, string resultData,
                Bundle resultExtras)
            {
                var res = this.ResultOk;
                if (res != null)
                {
                    res(resultCode == Result.Ok);
                }
            }
        }
    }
}
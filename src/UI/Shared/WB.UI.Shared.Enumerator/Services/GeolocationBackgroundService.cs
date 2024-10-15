using Android.Content;
using Android.Content.PM;
using Android.Locations;
using Android.OS;
using MvvmCross;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;

namespace WB.UI.Shared.Enumerator.Services;


public class LocationReceivedEventArgs : EventArgs
{
    public LocationReceivedEventArgs(GpsLocation location)
    {
        Location = location;
    }

    public GpsLocation Location { get; }
}

[Service(ForegroundServiceType = global::Android.Content.PM.ForegroundService.TypeLocation)]
public class GeolocationBackgroundService : Service, ILocationListener
{
    private ServiceBinder<GeolocationBackgroundService> binder;

    public event EventHandler<EventArgs> LocationReceived;

    LocationManager locationManager;

    public override void OnCreate()
    {
        base.OnCreate();
        locationManager = (LocationManager)GetSystemService(LocationService);
    }
    
    public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            var channel = new NotificationChannel("location_channel_id", "Location Service", NotificationImportance.Default)
            {
                Description = "Channel for location tracking"
            };
            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }
        
        
        var notification = new Notification.Builder(this, "location_channel_id")
            .SetContentTitle("Location Service")
            .SetContentText("Tracking your location in background")
            .SetSmallIcon(Resource.Drawable.dashboard_sync_icon)
            .SetOngoing(true)
            .Build();
        
        
        if (Build.VERSION.SdkInt < Android.OS.BuildVersionCodes.UpsideDownCake) {
            StartForeground(1, notification);
        } else {
            StartForeground(1, notification, ForegroundService.TypeLocation);
        }
        
        var locationCriteria = new Criteria
        {
            Accuracy = Accuracy.Fine, 
            PowerRequirement = Power.Medium
        };

        var bestProvider = locationManager.GetBestProvider(locationCriteria, true);
        locationManager.RequestLocationUpdates(bestProvider, 5000, 1, this);

        return StartCommandResult.Sticky;
    }
    
    public override void OnDestroy()
    {
        base.OnDestroy();
        locationManager.RemoveUpdates(this);
    }
    
    public override IBinder OnBind(Intent intent)
    {
        this.binder = new ServiceBinder<GeolocationBackgroundService>(this);
        return this.binder;
    }

    public void OnLocationChanged(Location location)
    {
        if (this.LocationReceived == null)
            return;
        
        var dateTimeOffset = GetTimestamp(location).ToUniversalTime();
        var gpsLocation = new GpsLocation(location.Accuracy, location.Altitude, location.Latitude, location.Longitude,
            dateTimeOffset);
        this.LocationReceived?.Invoke(this, new LocationReceivedEventArgs(gpsLocation));
    }

    public void OnProviderDisabled(string provider)
    {
        //throw new NotImplementedException();
    }

    public void OnProviderEnabled(string provider)
    {
        //throw new NotImplementedException();
    }

    public void OnStatusChanged(string provider, Availability status, Bundle extras)
    {
        //throw new NotImplementedException();
    }
    
    
    static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private static DateTimeOffset GetTimestamp(Location location)
    {
        try
        {
            return new DateTimeOffset(epoch.AddMilliseconds(location.Time));
        }
        catch (Exception)
        {
            return new DateTimeOffset(epoch);
        }
    }
}

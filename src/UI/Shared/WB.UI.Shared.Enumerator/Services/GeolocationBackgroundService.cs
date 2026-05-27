using Android.Content;
using Android.Content.PM;
using Android.Locations;
using Android.OS;
using MvvmCross;
using MvvmCross.Base;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;

namespace WB.UI.Shared.Enumerator.Services;


public class LocationReceivedEventArgs : EventArgs
{
    public LocationReceivedEventArgs(GpsLocation location, bool isFromMockProvider = false)
    {
        Location = location;
        IsFromMockProvider = isFromMockProvider;
    }

    public GpsLocation Location { get; }

    /// <summary>
    /// True when the fix was injected by an external GPS sensor via Android's mock
    /// location API (Developer Options → "Select mock location app").
    /// Accuracy thresholds should not be applied to these fixes because external
    /// sensors may report a fixed or zero accuracy value regardless of actual quality.
    /// </summary>
    public bool IsFromMockProvider { get; }
}

public interface INotificationManager
{
    void Notify(string message);
}

[Service(ForegroundServiceType = global::Android.Content.PM.ForegroundService.TypeLocation)]
public class GeolocationBackgroundService : Service, ILocationListener, INotificationManager
{
    private ServiceBinder<GeolocationBackgroundService> binder;

    private const int NotificationId = 1;
    readonly string locationNotificationChannelId = "location_channel_id";
    private Notification.Builder notificationsBuilder = null;
    private NotificationManager notificationManager = null;
    
    public void Notify(string message)
    {
        notificationsBuilder.SetContentText(message);
        notificationManager.Notify(NotificationId, notificationsBuilder.Build());
    }
    
    public event EventHandler<LocationReceivedEventArgs> LocationReceived;

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
            var channel = new NotificationChannel(locationNotificationChannelId, "Location Service", NotificationImportance.Default)
            {
                Description = "Channel for location tracking"
            };
            notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }

        notificationsBuilder = new Notification.Builder(this, locationNotificationChannelId)
            .SetContentTitle("Location Service")
            .SetContentText("Tracking your location in background")
            .SetSmallIcon(Resource.Drawable.dashboard_sync_icon)
            .SetOngoing(true);
        var notification = notificationsBuilder.Build();
        
        
        if (Build.VERSION.SdkInt < Android.OS.BuildVersionCodes.UpsideDownCake) {
            StartForeground(NotificationId, notification);
        } else {
            StartForeground(NotificationId, notification, ForegroundService.TypeLocation);
        }
        
        /*var locationCriteria = new Criteria
        {
            Accuracy = Accuracy.Fine, 
            PowerRequirement = Power.Medium
        };*/
        //var bestProvider = locationManager.GetBestProvider(locationCriteria, true);
        //locationManager.RequestLocationUpdates(bestProvider, 5000, 1, this);

        long minTimeMs = 5000;
        float minDistanceM = 1;
        locationManager.RequestLocationUpdates(LocationManager.GpsProvider, minTimeMs, minDistanceM, this);

        return StartCommandResult.NotSticky;
    }

    public override void OnTaskRemoved(Intent rootIntent)
    {
        Mvx.IoCProvider.GetSingleton<IGeolocationBackgroundServiceManager>()?.DisposeIfDisposable();
        StopForeground(true);
        StopSelf();

        base.OnTaskRemoved(rootIntent);
    }

    public override void OnDestroy()
    {
        locationManager.RemoveUpdates(this);
        
        base.OnDestroy();
    }
    
    public override IBinder OnBind(Intent intent)
    {
        this.binder = new ServiceBinder<GeolocationBackgroundService>(this);
        return this.binder;
    }

    public virtual void OnLocationChanged(Location location)
    {

        var dateTimeOffset = GetTimestamp(location).ToUniversalTime();
        var gpsLocation = new GpsLocation(
            location.HasAccuracy ? location.Accuracy : null,
            location.HasAltitude ? location.Altitude : null,
            location.Latitude,
            location.Longitude,
            dateTimeOffset);

        OnGpsLocationChanged(gpsLocation, location.IsFromMockProvider);
    }

    protected virtual void OnGpsLocationChanged(GpsLocation gpsLocation, bool isFromMockProvider = false)
    {
        this.LocationReceived?.Invoke(this, new LocationReceivedEventArgs(gpsLocation, isFromMockProvider));
    }

    public virtual void OnProviderDisabled(string provider)
    {
        //throw new NotImplementedException();
    }

    public virtual void OnProviderEnabled(string provider)
    {
        //throw new NotImplementedException();
    }

    public virtual void OnStatusChanged(string provider, Availability status, Bundle extras)
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

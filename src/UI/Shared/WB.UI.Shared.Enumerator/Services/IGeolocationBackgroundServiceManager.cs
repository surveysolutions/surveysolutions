namespace WB.UI.Shared.Enumerator.Services;

public interface IGeolocationBackgroundServiceManager
{
    bool HasGpsProvider();
    IGeolocationListener GetListen(IGeolocationListener geolocationListener);
    Task<bool> StartListen(IGeolocationListener geolocationListener);
    void StopListen(IGeolocationListener geolocationListener);
    event EventHandler<LocationReceivedEventArgs> LocationReceived;

    bool StopAll();
}

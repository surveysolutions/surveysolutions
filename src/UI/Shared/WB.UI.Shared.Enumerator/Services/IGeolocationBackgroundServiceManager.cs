namespace WB.UI.Shared.Enumerator.Services;

public interface IGeolocationBackgroundServiceManager
{
    bool HasGpsProvider();
    Task<bool> StartListen(IGeolocationListener geolocationListener);
    void StopListen(IGeolocationListener geolocationListener);
    event EventHandler<LocationReceivedEventArgs> LocationReceived;
}

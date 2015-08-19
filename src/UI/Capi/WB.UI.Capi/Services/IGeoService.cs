using System;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Geolocation;

namespace WB.UI.Shared.Android.GeolocationServices
{
    public interface IGeoService
    {
        bool IsGeolocationEnabled { get; }
        bool IsGeolocationAvailable { get; }
        bool IsListening { get; }
        void StopListening();

        event EventHandler<PositionErrorEventArgs> PositionError;
        event EventHandler<PositionEventArgs> PositionChanged;

        Task<Position> GetPositionAsync(int timeout, CancellationToken cancelToken);
        void StartListening(int minTime, double minDistance);
    }

}
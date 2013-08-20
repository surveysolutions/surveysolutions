using System;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Xamarin.Geolocation;

namespace CAPI.Android.GeolocationServices
{
    public class GeoService : IGeoService
    {
        private readonly Geolocator geolocator;

        public bool IsListening
        {
            get { return geolocator.IsListening; }
        }
        
        public bool IsGeolocationAvailable
        {
            get { return geolocator.IsGeolocationAvailable; }
        }

        public bool IsGeolocationEnabled
        {
            get { return geolocator.IsGeolocationEnabled; }
        }

        //todo: on suspend stop listening and start again on resume 

        public event EventHandler<PositionErrorEventArgs> PositionError
        {
            add { geolocator.PositionError += value; }
            remove { geolocator.PositionError -= value; }
        }

        public event EventHandler<PositionEventArgs> PositionChanged
        {
            add { geolocator.PositionChanged += value; }
            remove { geolocator.PositionChanged -= value; }
        }

        public void StartListening(int minTime, double minDistance)
        {
            geolocator.StartListening(minTime, minDistance);
        }

        public void StopListening()
        {
            geolocator.StopListening();
        }

        public GeoService(Context context)
        {
            geolocator = new Geolocator(context) { DesiredAccuracy = 50 };
        }

        public Task<Position> GetPositionAsync(int timeout, CancellationToken cancelToken)
        {
            // we need caching to save a battery if we want to track all coordinates of the answers
            return geolocator.GetPositionAsync(timeout, cancelToken);
        }
    }
}
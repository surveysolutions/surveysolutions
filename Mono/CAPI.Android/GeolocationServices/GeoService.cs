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

        private const int defaultTimeout = 45000;

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
            
            return geolocator.GetPositionAsync(timeout, cancelToken);
        }

        public Task<Position> GetPositionAsync(CancellationToken cancelToken)
        {
            return GetPositionAsync(defaultTimeout, cancelToken);
        }
    }
}
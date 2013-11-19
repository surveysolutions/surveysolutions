using System;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Xamarin.Geolocation;

namespace WB.UI.Shared.Android.GeolocationServices
{
    public class GeoService : IGeoService
    {
        private readonly Geolocator geolocator;

        public bool IsListening
        {
            get { return this.geolocator.IsListening; }
        }
        
        public bool IsGeolocationAvailable
        {
            get { return this.geolocator.IsGeolocationAvailable; }
        }

        public bool IsGeolocationEnabled
        {
            get { return this.geolocator.IsGeolocationEnabled; }
        }

        //todo: on suspend stop listening and start again on resume 

        public event EventHandler<PositionErrorEventArgs> PositionError
        {
            add { this.geolocator.PositionError += value; }
            remove { this.geolocator.PositionError -= value; }
        }

        public event EventHandler<PositionEventArgs> PositionChanged
        {
            add { this.geolocator.PositionChanged += value; }
            remove { this.geolocator.PositionChanged -= value; }
        }

        public void StartListening(int minTime, double minDistance)
        {
            this.geolocator.StartListening(minTime, minDistance);
        }

        public void StopListening()
        {
            this.geolocator.StopListening();
        }

        public GeoService(Context context)
        {
            this.geolocator = new Geolocator(context) { DesiredAccuracy = 50 };
        }

        public Task<Position> GetPositionAsync(int timeout, CancellationToken cancelToken)
        {
            // we need caching to save a battery if we want to track all coordinates of the answers
            return this.geolocator.GetPositionAsync(timeout, cancelToken);
        }
    }
}
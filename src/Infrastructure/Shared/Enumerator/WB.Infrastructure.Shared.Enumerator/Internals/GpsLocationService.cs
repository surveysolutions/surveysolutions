using System;
using System.Threading;
using System.Threading.Tasks;
using MvvmCross.Plugins.Location;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;

namespace WB.Infrastructure.Shared.Enumerator.Internals
{
    internal class GpsLocationService : IGpsLocationService
    {
        private class GpsLocation
        {
            public MvxGeoLocation BestLocation { get; set; }
            public MvxGeoLocation CurrentLocation { get; set; }
            public MvxGeoLocation LastSeenLocation { get; set; }

            public MvxGeoLocation GetLocation()
            {
                if (this.BestLocation != null) 
                    return this.BestLocation;
                if (this.CurrentLocation != null)
                    return this.CurrentLocation;
                return this.LastSeenLocation;
            }
        }

        private GpsLocation location;

        private readonly IMvxLocationWatcher locationWatcher;
        private int requestersCount;
        private static object lockObject = new object();
        
        public GpsLocationService(IMvxLocationWatcher locationWatcher)
        {
            this.locationWatcher = locationWatcher;
        }

        public async Task<MvxGeoLocation> GetLocation(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                this.ThreadSafeStart();
                this.location.LastSeenLocation = this.locationWatcher.LastSeenLocation;

                while (this.location.BestLocation == null)
                {
                    this.location.CurrentLocation = this.locationWatcher.CurrentLocation;

                    if (cancellationToken.IsCancellationRequested)
                        break;

                    await Task.Delay(TimeSpan.FromSeconds(5));
                }

                return await Task.FromResult(this.location.GetLocation());
            }
            finally
            {
                this.ThreadSafeStop();
            }
        }

        private void ThreadSafeStop()
        {
            Interlocked.Decrement(ref this.requestersCount);
            lock (lockObject)
            {
                if (this.requestersCount == 0)
                {
                    this.locationWatcher.Stop();
                }
            }
        }

        private void ThreadSafeStart()
        {
            Interlocked.Increment(ref this.requestersCount);
            lock (lockObject)
            {
                if (!this.locationWatcher.Started)
                {
                    this.location = new GpsLocation();
                    var locationOptions = new MvxLocationOptions() { Accuracy = MvxLocationAccuracy.Fine };
                    this.locationWatcher.Start(locationOptions, this.OnSuccess, this.OnError);
                }
            }
        }

        private void OnSuccess(MvxGeoLocation obj)
        {
            this.location.BestLocation = obj;
        }


        private void OnError(MvxLocationError obj)
        {
            // I really don't know what to do on error here. I think its ok for us to ignore it.
        }
    }
}
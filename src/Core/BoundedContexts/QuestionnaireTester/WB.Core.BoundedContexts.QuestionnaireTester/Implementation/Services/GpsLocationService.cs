using System;
using System.Threading;
using System.Threading.Tasks;
using Cirrious.MvvmCross.Plugins.Location;
using WB.Core.BoundedContexts.QuestionnaireTester.Services;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Services
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
                if (BestLocation != null) 
                    return BestLocation;
                if (CurrentLocation != null)
                    return CurrentLocation;
                return LastSeenLocation;
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
                location.LastSeenLocation = this.locationWatcher.LastSeenLocation;

                while (location.BestLocation == null)
                {
                    location.CurrentLocation = this.locationWatcher.CurrentLocation;

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
                    location = new GpsLocation();
                    var locationOptions = new MvxLocationOptions();
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
using System;
using System.Threading;
using System.Threading.Tasks;
using Cirrious.MvvmCross.Plugins.Location;
using WB.Core.BoundedContexts.QuestionnaireTester.Services;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Services
{
    internal class GpsLocationService : IGpsLocationService
    {
        private readonly IMvxLocationWatcher locationWatcher;
        private int requestersCount;
        private static object LockObject = new object();
        
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
                while (this.Coordinates == null) 
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                } 

                return await Task.FromResult(this.Coordinates);
            }
            finally
            {
                this.ThreadSafeStop();
            }
        }

        private void ThreadSafeStop()
        {
            Interlocked.Decrement(ref this.requestersCount);
            lock (LockObject)
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
            lock (LockObject)
            {
                if (!this.locationWatcher.Started)
                {
                    this.locationWatcher.Start(new MvxLocationOptions(), this.OnSuccess, this.OnError);
                }
            }
        }

        private void OnSuccess(MvxGeoLocation obj)
        {
            this.Coordinates = obj;
        }

        public MvxGeoLocation Coordinates { get; set; }

        private void OnError(MvxLocationError obj)
        {
            // I really don't know what to do on error here. I think its ok for us to ignore it.
        }
    }
}
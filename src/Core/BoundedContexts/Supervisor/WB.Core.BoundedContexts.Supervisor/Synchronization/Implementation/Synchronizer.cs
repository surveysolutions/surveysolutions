using System;
using System.Net.Http;

namespace WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation
{
    internal class Synchronizer : ISynchronizer 
    {
        private readonly ILocalFeedStorage localFeedStorage;
        private readonly IUserChangedFeedReader feedReader;

        public Synchronizer(ILocalFeedStorage localFeedStorage,
            IUserChangedFeedReader feedReader)
        {
            if (localFeedStorage == null) throw new ArgumentNullException("localFeedStorage");
            if (feedReader == null) throw new ArgumentNullException("feedReader");
            this.localFeedStorage = localFeedStorage;
            this.feedReader = feedReader;
        }

        public void FillLocalCopyOfFeed()
        {
            var lastStoredFeedEntry = this.localFeedStorage.GetLastEntryAsync().Result;
            var newEvents = feedReader.ReadAfterAsync(lastStoredFeedEntry).Result;

            foreach (var userChangedEvent in newEvents)
            {
                this.localFeedStorage.Store(userChangedEvent);
            }
        }
    }
}
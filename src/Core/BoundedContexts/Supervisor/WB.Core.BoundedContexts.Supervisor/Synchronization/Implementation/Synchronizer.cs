using System;
using System.Net.Http;
using System.Threading.Tasks;

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

        public async Task FillLocalCopyOfFeed()
        {
            var lastStoredFeedEntry = this.localFeedStorage.GetLastEntry();
            var newEvents = await feedReader.ReadAfterAsync(lastStoredFeedEntry);

            this.localFeedStorage.Store(newEvents);
        }
    }
}
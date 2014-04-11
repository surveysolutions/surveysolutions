using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Users;

namespace WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation
{
    internal class Synchronizer : ISynchronizer 
    {
        private readonly ILocalFeedStorage localFeedStorage;
        private readonly IUserChangedFeedReader feedReader;
        private readonly ILocalUserFeedProcessor localUserFeedProcessor;

        public Synchronizer(ILocalFeedStorage localFeedStorage,
            IUserChangedFeedReader feedReader,
            ILocalUserFeedProcessor localUserFeedProcessor)
        {
            if (localFeedStorage == null) throw new ArgumentNullException("localFeedStorage");
            if (feedReader == null) throw new ArgumentNullException("feedReader");
            if (localUserFeedProcessor == null) throw new ArgumentNullException("localUserFeedProcessor");
            this.localFeedStorage = localFeedStorage;
            this.feedReader = feedReader;
            this.localUserFeedProcessor = localUserFeedProcessor;
        }

        public async Task FillLocalCopyOfFeed()
        {
            var lastStoredFeedEntry = this.localFeedStorage.GetLastEntry();
            List<LocalUserChangedFeedEntry> newEvents = await feedReader.ReadAfterAsync(lastStoredFeedEntry);

            this.localFeedStorage.Store(newEvents);

            await this.localUserFeedProcessor.Process();
        }
    }
}
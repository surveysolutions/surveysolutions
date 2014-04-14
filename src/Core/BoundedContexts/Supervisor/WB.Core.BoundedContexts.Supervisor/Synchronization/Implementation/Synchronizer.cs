using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation
{
    internal class Synchronizer : ISynchronizer 
    {
        private readonly ILocalFeedStorage localFeedStorage;
        private readonly IUserChangedFeedReader feedReader;
        private readonly ILocalUserFeedProcessor localUserFeedProcessor;
        private bool isSynchronizationRunning;
        private static readonly object RebuildAllViewsLockObject = new object();

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

        public void Synchronize()
        {
            new Task(this.SynchronizeImpl).Start();
        }

        private void SynchronizeImpl()
        {
            if (!this.isSynchronizationRunning)
            {
                lock (RebuildAllViewsLockObject)
                {
                    if (!this.isSynchronizationRunning)
                    {
                        try
                        {
                            this.isSynchronizationRunning = true;

                            var lastStoredFeedEntry = this.localFeedStorage.GetLastEntry();
                            List<LocalUserChangedFeedEntry> newEvents = this.feedReader.ReadAfterAsync(lastStoredFeedEntry).Result;

                            this.localFeedStorage.Store(newEvents);

                            this.localUserFeedProcessor.Process();

                            this.isSynchronizationRunning = false;
                        }
                        finally
                        {
                            this.isSynchronizationRunning = false;
                        }
                    }
                }
            }
        }
    }
}
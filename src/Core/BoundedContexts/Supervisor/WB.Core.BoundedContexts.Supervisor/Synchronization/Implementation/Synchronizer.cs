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
        private readonly IInterviewsSynchronizer interviewsSynchronizer;
        private bool isSynchronizationRunning;
        private static readonly object LockObject = new object();

        public Synchronizer(ILocalFeedStorage localFeedStorage,
            IUserChangedFeedReader feedReader,
            ILocalUserFeedProcessor localUserFeedProcessor,
            IInterviewsSynchronizer interviewsSynchronizer)
        {
            if (localFeedStorage == null) throw new ArgumentNullException("localFeedStorage");
            if (feedReader == null) throw new ArgumentNullException("feedReader");
            if (localUserFeedProcessor == null) throw new ArgumentNullException("localUserFeedProcessor");
            if (interviewsSynchronizer == null) throw new ArgumentNullException("interviewsSynchronizer");
            this.localFeedStorage = localFeedStorage;
            this.feedReader = feedReader;
            this.localUserFeedProcessor = localUserFeedProcessor;
            this.interviewsSynchronizer = interviewsSynchronizer;
        }

        public void Synchronize()
        {
            new Task(this.SynchronizeImpl).Start();
        }

        private void SynchronizeImpl()
        {
            if (!this.isSynchronizationRunning)
            {
                lock (LockObject)
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

                            this.interviewsSynchronizer.Synchronize();
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
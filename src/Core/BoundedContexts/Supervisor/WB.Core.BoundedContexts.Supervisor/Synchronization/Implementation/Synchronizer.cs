using System;
using System.Collections.Generic;
using Quartz;

namespace WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation
{
    internal class Synchronizer : ISynchronizer, IJob
    {
        private readonly ILocalFeedStorage localFeedStorage;
        private readonly IUserChangedFeedReader feedReader;
        private readonly ILocalUserFeedProcessor localUserFeedProcessor;
        private readonly IInterviewsSynchronizer interviewsSynchronizer;
        private readonly SynchronizationContext synchronizationContext;
        private bool isSynchronizationRunning;
        private static readonly object LockObject = new object();

        public Synchronizer(ILocalFeedStorage localFeedStorage,
            IUserChangedFeedReader feedReader,
            ILocalUserFeedProcessor localUserFeedProcessor,
            IInterviewsSynchronizer interviewsSynchronizer,
            SynchronizationContext synchronizationContext)
        {
            if (localFeedStorage == null) throw new ArgumentNullException("localFeedStorage");
            if (feedReader == null) throw new ArgumentNullException("feedReader");
            if (localUserFeedProcessor == null) throw new ArgumentNullException("localUserFeedProcessor");
            if (interviewsSynchronizer == null) throw new ArgumentNullException("interviewsSynchronizer");
            if (synchronizationContext == null) throw new ArgumentNullException("synchronizationContext");
            this.localFeedStorage = localFeedStorage;
            this.feedReader = feedReader;
            this.localUserFeedProcessor = localUserFeedProcessor;
            this.interviewsSynchronizer = interviewsSynchronizer;
            this.synchronizationContext = synchronizationContext;
        }

        public void Synchronize()
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
                            this.synchronizationContext.Start();

                            var lastStoredFeedEntry = this.localFeedStorage.GetLastEntry();

                            this.synchronizationContext.PushMessage(lastStoredFeedEntry != null
                                ? string.Format("Last synchronized userentry id {0}, date {1}", lastStoredFeedEntry.EntryId,
                                    lastStoredFeedEntry.Timestamp)
                                : string.Format("Nothing synchronized yet, loading full users event stream"));

                            List<LocalUserChangedFeedEntry> newEvents = this.feedReader.ReadAfterAsync(lastStoredFeedEntry).Result;

                            this.synchronizationContext.PushMessage(string.Format("Saving {0} new events to local storage", newEvents.Count));
                            this.localFeedStorage.Store(newEvents);

                            this.localUserFeedProcessor.Process();

                            this.interviewsSynchronizer.Synchronize();
                        }
                        finally
                        {
                            this.isSynchronizationRunning = false;
                            this.synchronizationContext.Stop();
                        }
                    }
                }
            }
        }

        public void Execute(IJobExecutionContext context)
        {
            this.Synchronize();
        }
    }
}
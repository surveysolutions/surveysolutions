using System;
using System.Collections.Generic;
using Quartz;

namespace WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation
{
    internal class Synchronizer : ISynchronizer, IJob
    {
        private readonly ILocalFeedStorage localFeedStorage;
        private readonly ILocalUserFeedProcessor localUserFeedProcessor;
        private readonly IUserChangedFeedReader feedReader;

        
        private readonly IInterviewsSynchronizer interviewsSynchronizer;
        private readonly IQuestionnaireSynchronizer questionnaireSynchronizer;
        
        private readonly HeadquartersPullContext headquartersPullContext;
        private readonly HeadquartersPushContext headquartersPushContext;
        private bool isSynchronizationRunning;
        private static readonly object LockObject = new object();

        public Synchronizer(
            ILocalFeedStorage localFeedStorage,
            IUserChangedFeedReader feedReader,
            ILocalUserFeedProcessor localUserFeedProcessor, 
            IInterviewsSynchronizer interviewsSynchronizer, IQuestionnaireSynchronizer questionnaireSynchronizer,
            HeadquartersPullContext headquartersPullContext,
            HeadquartersPushContext headquartersPushContext)
        {
            if (localFeedStorage == null) throw new ArgumentNullException("localFeedStorage");
            if (feedReader == null) throw new ArgumentNullException("feedReader");
            if (localUserFeedProcessor == null) throw new ArgumentNullException("localUserFeedProcessor");
            if (interviewsSynchronizer == null) throw new ArgumentNullException("interviewsSynchronizer");
            if (headquartersPullContext == null) throw new ArgumentNullException("headquartersPullContext");
            if (headquartersPushContext == null) throw new ArgumentNullException("headquartersPushContext");

            this.localFeedStorage = localFeedStorage;
            this.feedReader = feedReader;
            this.localUserFeedProcessor = localUserFeedProcessor;
            this.interviewsSynchronizer = interviewsSynchronizer;
            this.headquartersPullContext = headquartersPullContext;
            this.headquartersPushContext = headquartersPushContext;
            this.questionnaireSynchronizer = questionnaireSynchronizer;
        }

        public void Pull()
        {
            if (!this.isSynchronizationRunning)
            {
                lock (LockObject)
                {
                    if (!this.isSynchronizationRunning)
                    {
                        this.PullImpl();
                    }
                }
            }
        }

        private void PullImpl()
        {
            try
            {
                this.isSynchronizationRunning = true;
                this.headquartersPullContext.Start();

                var lastStoredFeedEntry = this.localFeedStorage.GetLastEntry();

                this.headquartersPullContext.PushMessage(lastStoredFeedEntry != null
                    ? string.Format("Last synchronized userentry id {0}, date {1}", lastStoredFeedEntry.EntryId,
                        lastStoredFeedEntry.Timestamp)
                    : string.Format("Nothing synchronized yet, loading full users event stream"));

                List<LocalUserChangedFeedEntry> newEvents = this.feedReader.ReadAfterAsync(lastStoredFeedEntry).Result;

                this.headquartersPullContext.PushMessage(string.Format("Saving {0} new events to local storage", newEvents.Count));
                this.localFeedStorage.Store(newEvents);

                this.localUserFeedProcessor.Process();

                this.questionnaireSynchronizer.Pull();
                this.interviewsSynchronizer.Pull();
            }
            finally
            {
                this.isSynchronizationRunning = false;
                this.headquartersPullContext.Stop();
            }
        }

        public void Push(Guid userId)
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
                            this.headquartersPushContext.Start();

                            this.interviewsSynchronizer.Push(userId);
                        }
                        finally
                        {
                            this.isSynchronizationRunning = false;
                            this.headquartersPushContext.Stop();
                        }
                    }
                }
            }
        }

        public void Execute(IJobExecutionContext context)
        {
            this.Pull();
        }
    }
}
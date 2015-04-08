using System;
using System.Collections.Generic;
using Nito.AsyncEx;
using Quartz;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;

namespace WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation
{
    internal class Synchronizer : ISynchronizer, IJob
    {
        private readonly ILocalFeedStorage localUsersFeedStorage;
        private readonly ILocalUserFeedProcessor localUserFeedProcessor;
        private readonly IUserChangedFeedReader feedReader;
        
        private readonly IInterviewsSynchronizer interviewsSynchronizer;
        private readonly IQuestionnaireSynchronizer questionnaireSynchronizer;
        private readonly IPlainTransactionManager plainTransactionManager;

        private readonly HeadquartersPullContext headquartersPullContext;
        private readonly HeadquartersPushContext headquartersPushContext;
        private bool isSynchronizationRunning;
        private static readonly object LockObject = new object();

        public Synchronizer(
            ILocalFeedStorage localUsersFeedStorage,
            IUserChangedFeedReader feedReader,
            ILocalUserFeedProcessor localUserFeedProcessor, 
            IInterviewsSynchronizer interviewsSynchronizer, 
            IQuestionnaireSynchronizer questionnaireSynchronizer,
            IPlainTransactionManager plainTransactionManager,
            HeadquartersPullContext headquartersPullContext,
            HeadquartersPushContext headquartersPushContext)
        {
            if (localUsersFeedStorage == null) throw new ArgumentNullException("localFeedStorage");
            if (feedReader == null) throw new ArgumentNullException("feedReader");
            if (localUserFeedProcessor == null) throw new ArgumentNullException("localUserFeedProcessor");
            if (interviewsSynchronizer == null) throw new ArgumentNullException("interviewsSynchronizer");
            if (plainTransactionManager == null) throw new ArgumentNullException("plainTransactionManager");
            if (headquartersPullContext == null) throw new ArgumentNullException("headquartersPullContext");
            if (headquartersPushContext == null) throw new ArgumentNullException("headquartersPushContext");

            this.localUsersFeedStorage = localUsersFeedStorage;
            this.feedReader = feedReader;
            this.localUserFeedProcessor = localUserFeedProcessor;
            this.interviewsSynchronizer = interviewsSynchronizer;
            this.headquartersPullContext = headquartersPullContext;
            this.headquartersPushContext = headquartersPushContext;
            this.questionnaireSynchronizer = questionnaireSynchronizer;
            this.plainTransactionManager = plainTransactionManager;
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
                
                this.plainTransactionManager.ExecuteInPlainTransaction(() =>
                {
                    var lastStoredFeedEntry = this.localUsersFeedStorage.GetLastEntry();

                    this.headquartersPullContext.PushMessage(lastStoredFeedEntry != null ? 
                        string.Format("Last synchronized userentry id {0}, date {1}", lastStoredFeedEntry.EntryId, lastStoredFeedEntry.Timestamp) : 
                        string.Format("Nothing synchronized yet, loading full users event stream"));

                    List<LocalUserChangedFeedEntry> newEvents = AsyncContext.Run(() => this.feedReader.ReadAfterAsync(lastStoredFeedEntry));

                    this.headquartersPullContext.PushMessageFormat("Saving {0} new events to local storage", newEvents.Count);
                    this.localUsersFeedStorage.Store(newEvents);
                });

                var supervisorIds = this.localUserFeedProcessor.PullUsersAndReturnListOfSynchronizedSupervisorsId();

                this.questionnaireSynchronizer.Pull();

                this.interviewsSynchronizer.PullInterviewsForSupervisors(supervisorIds);
            }
            catch (ApplicationException e)
            {
                this.headquartersPullContext.PushError(e.Message);
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
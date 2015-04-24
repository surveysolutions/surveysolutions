using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation
{
    internal class LocalFeedStorage : ILocalFeedStorage
    {
        private readonly IPlainStorageAccessor<LocalUserChangedFeedEntry> plainStorage;

        public LocalFeedStorage(IPlainStorageAccessor<LocalUserChangedFeedEntry> plainStorage)
        {
            if (plainStorage == null) throw new ArgumentNullException("plainStorage");

            this.plainStorage = plainStorage;
        }

        public LocalUserChangedFeedEntry GetLastEntry()
        {
            return this.plainStorage.Query(_ => _.OrderByDescending(x => x.Timestamp).FirstOrDefault());
        }

        public void Store(IEnumerable<LocalUserChangedFeedEntry> userChangedEvent)
        {
            this.plainStorage.Store(userChangedEvent.Select(@event => Tuple.Create(@event, (object) @event.EntryId)));
        }

        public IEnumerable<LocalUserChangedFeedEntry> GetNotProcessedSupervisorEvents(string supervisorId)
        {
            return this.plainStorage.Query(_ => _.Where(x => x.IsProcessed == false && (x.SupervisorId == supervisorId || x.SupervisorId == null))
                                                 .OrderBy(x => x.Timestamp)
                                                 .ToList());
        }
    }
}
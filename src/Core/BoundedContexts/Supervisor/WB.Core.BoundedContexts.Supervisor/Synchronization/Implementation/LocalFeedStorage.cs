using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Raven.Client;
using Raven.Client.Linq;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Raven.PlainStorage;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Users;

namespace WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation
{
    internal class LocalFeedStorage : ILocalFeedStorage
    {
        private readonly IQueryablePlainStorageAccessor<UserChangedFeedEntry> plainStorageProvider;

        public LocalFeedStorage(IQueryablePlainStorageAccessor<UserChangedFeedEntry> plainStorageProvider)
        {
            if (plainStorageProvider == null) throw new ArgumentNullException("plainStorageProvider");

            this.plainStorageProvider = plainStorageProvider;
        }

        public UserChangedFeedEntry GetLastEntry()
        {
            return plainStorageProvider.Query(_ => _.OrderByDescending(x => x.Timestamp).FirstOrDefault());
        }

        public void Store(IEnumerable<UserChangedFeedEntry> userChangedEvent)
        {
            plainStorageProvider.Store(userChangedEvent.Select(@event => Tuple.Create(@event, @event.EntryId)));
        }
    }
}
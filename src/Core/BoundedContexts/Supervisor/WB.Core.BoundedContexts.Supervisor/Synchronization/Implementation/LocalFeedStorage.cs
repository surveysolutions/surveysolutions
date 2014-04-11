using System;
using System.Linq;
using System.Threading.Tasks;
using Raven.Client;
using Raven.Client.Linq;
using WB.Core.Infrastructure.Raven.PlainStorage;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Users;

namespace WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation
{
    internal class LocalFeedStorage : ILocalFeedStorage
    {
        private readonly IRavenPlainStorageProvider plainStorageProvider;

        public LocalFeedStorage(IRavenPlainStorageProvider plainStorageProvider)
        {
            if (plainStorageProvider == null) throw new ArgumentNullException("plainStorageProvider");

            this.plainStorageProvider = plainStorageProvider;
        }

        public async Task<UserChangedFeedEntry> GetLastEntryAsync()
        {
            using (var session = plainStorageProvider.GetDocumentStore().OpenAsyncSession())
            {
                return await session.Query<UserChangedFeedEntry>().OrderByDescending(x => x.Timestamp).FirstOrDefaultAsync();
            }
        }

        public void Store(UserChangedFeedEntry userChangedEvent)
        {
            using (var session = plainStorageProvider.GetDocumentStore().OpenSession())
            {
                session.Store(userChangedEvent, userChangedEvent.EntryId);
            }
        }
    }
}
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Supervisor.Synchronization
{
    public class HeadquartersPushContext : HeadquartersSynchronizationContext
    {
        public HeadquartersPushContext(IPlainStorageAccessor<SynchronizationStatus> statusStorage)
            : base(statusStorage) {}

        protected override string StorageDocumentId
        {
            get { return "PushStatus"; }
        }
    }
}
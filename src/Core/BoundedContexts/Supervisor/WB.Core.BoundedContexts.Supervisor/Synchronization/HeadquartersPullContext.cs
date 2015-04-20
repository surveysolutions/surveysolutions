using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Supervisor.Synchronization
{
    public class HeadquartersPullContext : HeadquartersSynchronizationContext
    {
        public HeadquartersPullContext(IPlainKeyValueStorage<SynchronizationStatus> statusStorage)
            : base(statusStorage) {}

        protected override string StorageDocumentId
        {
            get { return "PullStatus"; }
        }
    }
}
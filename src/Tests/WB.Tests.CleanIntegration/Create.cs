using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Tests.CleanIntegration
{
    internal static class Create
    {
        public static ReadSideChunkWriter ReadSideChunkWriter(IReadSideRepositoryWriter<SynchronizationDeltaMetaInformation> metaWriter,
            IReadSideKeyValueStorage<SynchronizationDeltaContent> inMemoryReadSideRepositoryAccessor,
            IReadSideKeyValueStorage<SynchronizationDeltasCounter> storage)
        {
            return new ReadSideChunkWriter(metaWriter, inMemoryReadSideRepositoryAccessor, storage);
        }
    }
}
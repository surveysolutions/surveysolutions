using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Infrastructure.Native.Storage.Memory.Implementation;


namespace WB.Tests.Unit.Infrastructure.MemoryCachedReadSideStoreTests
{
    internal class when_disable_cache_is_called : MemoryCachedReadSideStoreTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            readSideStorageMock = new Mock<IReadSideStorage<ReadSideRepositoryEntity>>();
            memoryCachedReadSideStorage = CreateMemoryCachedReadSideStore(readSideStorageMock.Object, cacheSizeInEntities: MaxCountOfCachedEntities);
            memoryCachedReadSideStorage.EnableCache();

            for (int i = 0; i < MaxCountOfCachedEntities - 1; i++)
            {
                memoryCachedReadSideStorage.Store(new ReadSideRepositoryEntity(), id + i);
            }
            memoryCachedReadSideStorage.Store(null, null_id);
            BecauseOf();
        }
        public void BecauseOf() =>
            memoryCachedReadSideStorage.DisableCache();

        [NUnit.Framework.Test] public void should_call_BulkStore_of_IReadSideStorage_once () =>
            readSideStorageMock.Verify(x => x.BulkStore(Moq.It.IsAny<List<Tuple<ReadSideRepositoryEntity, string>>>()), Times.Once);

        [NUnit.Framework.Test] public void should_call_Remove_of_IReadSideStorage_once () =>
          readSideStorageMock.Verify(x => x.Remove(null_id), Times.Once);

        [NUnit.Framework.Test] public void should_return_readable_status () =>
            memoryCachedReadSideStorage.GetReadableStatus().Should().Be("  |  cache disabled  |  cached (memory): 0");

        private static MemoryCachedReadSideStorage<ReadSideRepositoryEntity> memoryCachedReadSideStorage;
        private static Mock<IReadSideStorage<ReadSideRepositoryEntity>> readSideStorageMock;
        private static string id = "id";
        private static string null_id = "null_id";

        private const int MaxCountOfCachedEntities = 256;
    }
}

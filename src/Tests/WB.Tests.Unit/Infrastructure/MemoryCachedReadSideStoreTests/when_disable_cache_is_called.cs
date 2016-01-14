using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Storage.Memory.Implementation;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Infrastructure.MemoryCachedReadSideStoreTests
{
    internal class when_disable_cache_is_called : MemoryCachedReadSideStoreTestContext
    {
        Establish context = () =>
        {
            readSideStorageMock = new Mock<IReadSideStorage<ReadSideRepositoryEntity>>();
            memoryCachedReadSideStorage = CreateMemoryCachedReadSideStore(readSideStorageMock.Object, cacheSizeInEntities: MaxCountOfCachedEntities);
            memoryCachedReadSideStorage.EnableCache();

            for (int i = 0; i < MaxCountOfCachedEntities - 1; i++)
            {
                memoryCachedReadSideStorage.Store(new ReadSideRepositoryEntity(), id + i);
            }
            memoryCachedReadSideStorage.Store(null, null_id);
        };
        Because of = () =>
            memoryCachedReadSideStorage.DisableCache();

        It should_call_BulkStore_of_IReadSideStorage_once = () =>
            readSideStorageMock.Verify(x => x.BulkStore(Moq.It.IsAny<List<Tuple<ReadSideRepositoryEntity, string>>>()), Times.Once);

        It should_call_Remove_of_IReadSideStorage_once = () =>
          readSideStorageMock.Verify(x => x.Remove(null_id), Times.Once);

        It should_return_readable_status = () =>
            memoryCachedReadSideStorage.GetReadableStatus().ShouldEqual("  |  cache disabled  |  cached (memory): 0");

        private static MemoryCachedReadSideStorage<ReadSideRepositoryEntity> memoryCachedReadSideStorage;
        private static Mock<IReadSideStorage<ReadSideRepositoryEntity>> readSideStorageMock;
        private static string id = "id";
        private static string null_id = "null_id";

        private const int MaxCountOfCachedEntities = 256;
    }
}

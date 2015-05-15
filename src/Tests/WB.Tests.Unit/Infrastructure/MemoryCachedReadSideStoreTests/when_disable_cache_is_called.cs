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
            memoryCachedReadSideStore = CreateMemoryCachedReadSideStore(readSideStorageMock.Object);
            memoryCachedReadSideStore.EnableCache();

            for (int i = 0; i < MaxCountOfCachedEntities - 1; i++)
            {
                memoryCachedReadSideStore.Store(new ReadSideRepositoryEntity(), id + i);
            }
            memoryCachedReadSideStore.Store(null, null_id);
        };
        Because of = () =>
            memoryCachedReadSideStore.DisableCache();

        It should_call_Store_of_IReadSideStorage_255_times = () =>
            readSideStorageMock.Verify(x => x.Store(Moq.It.IsAny<ReadSideRepositoryEntity>(), Moq.It.IsAny<string>()), Times.Exactly(MaxCountOfCachedEntities-1));

        It should_call_Remove_of_IReadSideStorage_once = () =>
          readSideStorageMock.Verify(x => x.Remove(null_id), Times.Once);

        It should_return_readable_status = () =>
            memoryCachedReadSideStore.GetReadableStatus().ShouldEqual("  |  cache disabled  |  cached 0");

        private static MemoryCachedReadSideStore<ReadSideRepositoryEntity> memoryCachedReadSideStore;
        private static Mock<IReadSideStorage<ReadSideRepositoryEntity>> readSideStorageMock;
        private static string id = "id";
        private static string null_id = "null_id";

        private const int MaxCountOfCachedEntities = 256;
    }
}

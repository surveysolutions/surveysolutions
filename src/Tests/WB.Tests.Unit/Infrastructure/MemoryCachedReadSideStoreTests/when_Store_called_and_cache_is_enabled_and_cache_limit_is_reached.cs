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
    internal class when_Store_called_and_cache_is_enabled_and_cache_limit_is_reached : MemoryCachedReadSideStoreTestContext
    {
        Establish context = () =>
        {
            readSideStorageMock = new Mock<IReadSideStorage<ReadSideRepositoryEntity>>();
            memoryCachedReadSideStore = CreateMemoryCachedReadSideStore(readSideStorageMock.Object);
            memoryCachedReadSideStore.EnableCache();

            for (int i = 0; i < MaxCountOfCachedEntities-1; i++)
            {
                memoryCachedReadSideStore.Store(new ReadSideRepositoryEntity(), id + i);
            }
        };
        Because of = () =>
            memoryCachedReadSideStore.Store(new ReadSideRepositoryEntity(), last_id);

        It should_call_Store_of_IReadSideStorage_16_times = () =>
            readSideStorageMock.Verify(x => x.Store(Moq.It.IsAny<ReadSideRepositoryEntity>(), Moq.It.IsAny<string>()), Times.Exactly(MaxCountOfEntitiesInOneStoreOperation));

        It should_return_readable_status = () =>
            memoryCachedReadSideStore.GetReadableStatus().ShouldEqual("IReadSideStorage cache is enabled;    cached 128;");

        It should_return_view_type_ReadSideRepositoryEntity = () =>
            memoryCachedReadSideStore.ViewType.ShouldEqual(typeof(ReadSideRepositoryEntity));

        private static MemoryCachedReadSideStore<ReadSideRepositoryEntity> memoryCachedReadSideStore;
        private static Mock<IReadSideStorage<ReadSideRepositoryEntity>> readSideStorageMock;
        private static string id = "id";
        private static string last_id = "last_id";

        private const int MaxCountOfCachedEntities = 256;
        private const int MaxCountOfEntitiesInOneStoreOperation = 128;
    }
}

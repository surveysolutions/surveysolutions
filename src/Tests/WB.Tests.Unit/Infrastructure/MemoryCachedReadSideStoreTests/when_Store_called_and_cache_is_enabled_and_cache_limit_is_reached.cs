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
            memoryCachedReadSideStorage = CreateMemoryCachedReadSideStore(readSideStorageMock.Object,
                cacheSizeInEntities: MaxCountOfCachedEntities, storeOperationBulkSize: MaxCountOfEntitiesInOneStoreOperation);
            memoryCachedReadSideStorage.EnableCache();

            for (int i = 0; i < MaxCountOfCachedEntities-1; i++)
            {
                memoryCachedReadSideStorage.Store(new ReadSideRepositoryEntity(), id + i);
            }
        };
        Because of = () =>
            memoryCachedReadSideStorage.Store(new ReadSideRepositoryEntity(), last_id);

        It should_call_BulkStore_of_IReadSideStorage_once = () =>
            readSideStorageMock.Verify(x => x.BulkStore(Moq.It.IsAny<List<Tuple<ReadSideRepositoryEntity, string>>>()), Times.Once);

        It should_return_readable_status = () =>
            memoryCachedReadSideStorage.GetReadableStatus().ShouldEqual("  |  cache enabled  |  cached 128");

        It should_return_view_type_ReadSideRepositoryEntity = () =>
            memoryCachedReadSideStorage.ViewType.ShouldEqual(typeof(ReadSideRepositoryEntity));

        private static MemoryCachedReadSideStorage<ReadSideRepositoryEntity> memoryCachedReadSideStorage;
        private static Mock<IReadSideStorage<ReadSideRepositoryEntity>> readSideStorageMock;
        private static string id = "id";
        private static string last_id = "last_id";

        private const int MaxCountOfCachedEntities = 256;
        private const int MaxCountOfEntitiesInOneStoreOperation = 128;
    }
}

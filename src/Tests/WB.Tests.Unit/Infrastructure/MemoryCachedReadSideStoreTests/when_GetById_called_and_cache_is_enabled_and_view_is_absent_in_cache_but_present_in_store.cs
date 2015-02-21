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
    internal class when_GetById_called_and_cache_is_enabled_and_view_is_absent_in_cache_but_present_in_store : MemoryCachedReadSideStoreTestContext
    {
        Establish context = () =>
        {
            readSideStorageMock = new Mock<IReadSideStorage<ReadSideRepositoryEntity>>();
            readSideStorageMock.Setup(x => x.GetById(id)).Returns(view);
            memoryCachedReadSideStore = CreateMemoryCachedReadSideStore(readSideStorageMock.Object);
            memoryCachedReadSideStore.EnableCache();
        };
        Because of = () =>
            result = memoryCachedReadSideStore.GetById(id);

        It should_once_call_GetById_of_IReadSideStorage = () =>
            readSideStorageMock.Verify(x => x.GetById(id), Times.Once);

        It should_return_cached_result = () =>
           result.ShouldEqual(view);

        private static MemoryCachedReadSideStore<ReadSideRepositoryEntity> memoryCachedReadSideStore;
        private static Mock<IReadSideStorage<ReadSideRepositoryEntity>> readSideStorageMock;
        private static string id = "id_view";
        private static ReadSideRepositoryEntity view = new ReadSideRepositoryEntity();
        private static ReadSideRepositoryEntity result;
    }
}

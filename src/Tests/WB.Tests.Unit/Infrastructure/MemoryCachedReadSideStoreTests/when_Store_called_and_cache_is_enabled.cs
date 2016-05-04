using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Infrastructure.Native.Storage.Memory.Implementation;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Infrastructure.MemoryCachedReadSideStoreTests
{
    internal class when_Store_called_and_cache_is_enabled : MemoryCachedReadSideStoreTestContext
    {
        Establish context = () =>
        {
            readSideStorageMock = new Mock<IReadSideStorage<ReadSideRepositoryEntity>>();
            memoryCachedReadSideStorage = CreateMemoryCachedReadSideStore(readSideStorageMock.Object);
            memoryCachedReadSideStorage.EnableCache();
        };
        Because of = () =>
            memoryCachedReadSideStorage.Store(view, id);

        It should_never_call_Store_of_IReadSideStorage = () =>
            readSideStorageMock.Verify(x => x.Store(view, id), Times.Never);

        It should_store_view_in_cache = () =>
           memoryCachedReadSideStorage.GetById(id).ShouldEqual(view);

        private static MemoryCachedReadSideStorage<ReadSideRepositoryEntity> memoryCachedReadSideStorage;
        private static Mock<IReadSideStorage<ReadSideRepositoryEntity>> readSideStorageMock;
        private static string id = "id_view";
        private static ReadSideRepositoryEntity view = new ReadSideRepositoryEntity();
    }
}

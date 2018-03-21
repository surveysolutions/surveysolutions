using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Infrastructure.Native.Storage.Memory.Implementation;


namespace WB.Tests.Unit.Infrastructure.MemoryCachedReadSideStoreTests
{
    internal class when_Remove_called_and_cache_is_enabled : MemoryCachedReadSideStoreTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            readSideStorageMock = new Mock<IReadSideStorage<ReadSideRepositoryEntity>>();
            memoryCachedReadSideStorage = CreateMemoryCachedReadSideStore(readSideStorageMock.Object);
            memoryCachedReadSideStorage.EnableCache();
            memoryCachedReadSideStorage.Store(view, id);
            BecauseOf();
        }
        public void BecauseOf() =>
             memoryCachedReadSideStorage.Remove(id);

        [NUnit.Framework.Test] public void should_once_call_Remove_of_IReadSideStorage () =>
            readSideStorageMock.Verify(x => x.Remove(id), Times.Once);

        [NUnit.Framework.Test] public void should_delete_view_from_cache () =>
           memoryCachedReadSideStorage.GetById(id).Should().BeNull();

        private static MemoryCachedReadSideStorage<ReadSideRepositoryEntity> memoryCachedReadSideStorage;
        private static Mock<IReadSideStorage<ReadSideRepositoryEntity>> readSideStorageMock;
        private static string id = "id_view";
        private static ReadSideRepositoryEntity view = new ReadSideRepositoryEntity();
    }
}

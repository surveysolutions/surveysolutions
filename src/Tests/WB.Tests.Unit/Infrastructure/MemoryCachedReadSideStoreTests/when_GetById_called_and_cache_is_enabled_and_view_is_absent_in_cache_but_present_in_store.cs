using FluentAssertions;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Infrastructure.Native.Storage.Memory.Implementation;


namespace WB.Tests.Unit.Infrastructure.MemoryCachedReadSideStoreTests
{
    internal class when_GetById_called_and_cache_is_enabled_and_view_is_absent_in_cache_but_present_in_store : MemoryCachedReadSideStoreTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            readSideStorageMock = new Mock<IReadSideStorage<ReadSideRepositoryEntity>>();
            readSideStorageMock.Setup(x => x.GetById(id)).Returns(view);
            memoryCachedReadSideStorage = CreateMemoryCachedReadSideStore(readSideStorageMock.Object);
            memoryCachedReadSideStorage.EnableCache();
            BecauseOf();
        }
        public void BecauseOf() =>
            result = memoryCachedReadSideStorage.GetById(id);

        [NUnit.Framework.Test] public void should_once_call_GetById_of_IReadSideStorage () =>
            readSideStorageMock.Verify(x => x.GetById(id), Times.Once);

        [NUnit.Framework.Test] public void should_return_cached_result () =>
           result.Should().Be(view);

        private static MemoryCachedReadSideStorage<ReadSideRepositoryEntity> memoryCachedReadSideStorage;
        private static Mock<IReadSideStorage<ReadSideRepositoryEntity>> readSideStorageMock;
        private static string id = "id_view";
        private static ReadSideRepositoryEntity view = new ReadSideRepositoryEntity();
        private static ReadSideRepositoryEntity result;
    }
}

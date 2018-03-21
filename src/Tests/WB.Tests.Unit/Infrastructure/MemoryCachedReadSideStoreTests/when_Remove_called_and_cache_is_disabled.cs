using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Infrastructure.Native.Storage.Memory.Implementation;


namespace WB.Tests.Unit.Infrastructure.MemoryCachedReadSideStoreTests
{
    internal class when_Remove_called_and_cache_is_disabled : MemoryCachedReadSideStoreTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            readSideStorageMock = new Mock<IReadSideStorage<ReadSideRepositoryEntity>>();
            memoryCachedReadSideStorage = CreateMemoryCachedReadSideStore(readSideStorageMock.Object);
            BecauseOf();
        }
        public void BecauseOf() =>
            memoryCachedReadSideStorage.Remove(id);

        [NUnit.Framework.Test] public void should_call_Remove_of_IReadSideStorage () =>
            readSideStorageMock.Verify(x => x.Remove(id), Times.Once);

        private static MemoryCachedReadSideStorage<ReadSideRepositoryEntity> memoryCachedReadSideStorage;
        private static Mock<IReadSideStorage<ReadSideRepositoryEntity>> readSideStorageMock;
        private static string id = "id_view";
    }
}

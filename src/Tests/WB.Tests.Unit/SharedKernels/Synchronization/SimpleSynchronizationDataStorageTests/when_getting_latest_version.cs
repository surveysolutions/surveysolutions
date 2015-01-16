using Machine.Specifications;
using Moq;
using WB.Core.Synchronization.SyncStorage;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Synchronization.SimpleSynchronizationDataStorageTests
{
    internal class when_getting_latest_version : SimpleSynchronizationDataStorageTestContext
    {
        Establish context = () =>
        {
            chunkStorageReader = new Mock<IChunkReader>();
            simpleSynchronizationDataStorage = GetSimpleSynchronizationDataStorage(chunkStorageReader: chunkStorageReader.Object);
        };

        private Because of = () => simpleSynchronizationDataStorage.GetLatestVersion(id);

        It should_store_chunck = () =>
            chunkStorageReader.Verify(x => x.ReadChunk(id), Times.Once);
        
        private static SimpleSynchronizationDataStorage simpleSynchronizationDataStorage;
        private static string id = "11";
        private static Mock<IChunkReader> chunkStorageReader;

    }
}

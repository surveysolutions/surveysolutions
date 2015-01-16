using System;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.Synchronization.SyncStorage;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Synchronization.SimpleSynchronizationDataStorageTests
{
    internal class when_deleting_questionnaire : SimpleSynchronizationDataStorageTestContext
    {
        Establish context = () =>
        {
            timestamp = DateTime.MinValue;
            chunkStorageWriter = CreateIChunkWriter();
            simpleSynchronizationDataStorage = GetSimpleSynchronizationDataStorage(chunkStorageWriter: chunkStorageWriter.Object);
        };

        private Because of = () => simpleSynchronizationDataStorage.DeleteQuestionnaire(questionnaireId, version, timestamp);

        It should_store_chunck = () =>
            chunkStorageWriter.Verify(
                x => x.StoreChunk(
                    Moq.It.Is<SyncItem>(s => s.ItemType == SyncItemType.DeleteTemplate && 
                                             s.IsCompressed == true), null, timestamp), Times.Once);


        private static SimpleSynchronizationDataStorage simpleSynchronizationDataStorage;
        private static Guid questionnaireId = Guid.Parse("1BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static DateTime timestamp;
        private static long version = 4;

        private static Mock<IChunkWriter> chunkStorageWriter;

    }
}

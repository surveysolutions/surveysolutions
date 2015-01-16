using System;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.Synchronization.SyncStorage;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Synchronization.SimpleSynchronizationDataStorageTests
{
    internal class when_marking_interview_for_deleting : SimpleSynchronizationDataStorageTestContext
    {
        Establish context = () =>
        {
            timestamp = DateTime.MinValue;
            chunkStorageWriter = CreateIChunkWriter();
            simpleSynchronizationDataStorage = GetSimpleSynchronizationDataStorage(chunkStorageWriter: chunkStorageWriter.Object);
        };

        private Because of = () => simpleSynchronizationDataStorage.MarkInterviewForClientDeleting(interviewId, responsibleId, timestamp);

        It should_store_chunck = () =>
            chunkStorageWriter.Verify(x => x.StoreChunk(
                Moq.It.Is<SyncItem>(s => s.ItemType == SyncItemType.DeleteQuestionnare && 
                    s.RootId == interviewId && 
                    s.IsCompressed == true &&
                    s.MetaInfo == string.Empty), 
                responsibleId, 
                timestamp), Times.Once);

        
        private static SimpleSynchronizationDataStorage simpleSynchronizationDataStorage;
        private static Guid responsibleId = Guid.Parse("1BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid interviewId = Guid.Parse("1BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBA");
        private static DateTime timestamp;

        private static Mock<IChunkWriter> chunkStorageWriter;

    }
}

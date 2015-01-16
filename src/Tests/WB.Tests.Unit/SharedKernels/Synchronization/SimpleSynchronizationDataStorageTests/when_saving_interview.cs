using System;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.Synchronization.SyncStorage;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Synchronization.SimpleSynchronizationDataStorageTests
{
    internal class when_saving_interview : SimpleSynchronizationDataStorageTestContext
    {
        Establish context = () =>
        {
            interview = new InterviewSynchronizationDto() {Id = interviewId};
            timestamp = DateTime.MinValue;
            chunkStorageWriter = CreateIChunkWriter();
            simpleSynchronizationDataStorage = GetSimpleSynchronizationDataStorage(chunkStorageWriter: chunkStorageWriter.Object);
        };

        private Because of = () => simpleSynchronizationDataStorage.SaveInterview(interview, responsibleId, timestamp);

        It should_store_chunck = () =>
            chunkStorageWriter.Verify(x => x.StoreChunk(
                Moq.It.Is<SyncItem>(s => s.ItemType == SyncItemType.Questionnare && 
                    s.RootId == interviewId && 
                    s.IsCompressed ), 
                responsibleId, 
                timestamp), Times.Once);

        
        private static SimpleSynchronizationDataStorage simpleSynchronizationDataStorage;
        private static InterviewSynchronizationDto interview;
        private static Guid responsibleId = Guid.Parse("1BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid interviewId = Guid.Parse("1BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBC");
        private static DateTime timestamp;

        private static Mock<IChunkWriter> chunkStorageWriter;

    }
}

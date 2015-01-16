using System;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.Synchronization.SyncStorage;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Synchronization.SimpleSynchronizationDataStorageTests
{
    internal class when_saving_questionnare_assembly : SimpleSynchronizationDataStorageTestContext
    {
        Establish context = () =>
        {
            timestamp = DateTime.MinValue;
            chunkStorageWriter = CreateIChunkWriter();
            simpleSynchronizationDataStorage = GetSimpleSynchronizationDataStorage(chunkStorageWriter: chunkStorageWriter.Object);
        };

        private Because of = () => simpleSynchronizationDataStorage.SaveQuestionnaireAssembly(qId, version, assembly, timestamp);

        It should_store_chunck = () =>
            chunkStorageWriter.Verify(x => x.StoreChunk(
                Moq.It.Is<SyncItem>(s => s.ItemType == SyncItemType.QuestionnaireAssembly && 
                    s.RootId == qId.Combine(SimpleSynchronizationDataStorage.AssemblySeed).Combine(version)), 
                null, 
                timestamp), Times.Once);

        
        private static SimpleSynchronizationDataStorage simpleSynchronizationDataStorage;

        private static Guid qId = Guid.Parse("1BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static string assembly = "dummy";
        private static long version = 3;
        private static DateTime timestamp;

        private static Mock<IChunkWriter> chunkStorageWriter;

    }
}

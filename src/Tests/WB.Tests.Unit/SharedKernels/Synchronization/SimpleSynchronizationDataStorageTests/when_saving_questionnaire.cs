using System;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.Synchronization.SyncStorage;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Synchronization.SimpleSynchronizationDataStorageTests
{
    internal class when_saving_questionnaire : SimpleSynchronizationDataStorageTestContext
    {
        Establish context = () =>
        {
            questionnaire = new QuestionnaireDocument(){PublicKey = qId};
            timestamp = DateTime.MinValue;
            chunkStorageWriter = CreateIChunkWriter();
            simpleSynchronizationDataStorage = GetSimpleSynchronizationDataStorage(chunkStorageWriter: chunkStorageWriter.Object);
        };

        private Because of = () => simpleSynchronizationDataStorage.SaveQuestionnaire(questionnaire, version, false, timestamp);

        It should_store_chunck = () =>
            chunkStorageWriter.Verify(x => x.StoreChunk(Moq.It.Is<SyncItem>(
                s => s.ItemType == SyncItemType.Template &&
                    s.IsCompressed && s.RootId == qId.Combine(version)), null, timestamp), Times.Once);

        
        private static SimpleSynchronizationDataStorage simpleSynchronizationDataStorage;
        private static QuestionnaireDocument questionnaire;
        private static long version = 3;
        private static Guid responsibleId = Guid.Parse("1BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid qId = Guid.Parse("1BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBC");
        private static DateTime timestamp;

        private static Mock<IChunkWriter> chunkStorageWriter;

    }
}

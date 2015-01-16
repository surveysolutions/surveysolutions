using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.Synchronization.SyncStorage;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Synchronization.SimpleSynchronizationDataStorageTests
{
    internal class when_saving_user : SimpleSynchronizationDataStorageTestContext
    {
        Establish context = () =>
        {
            user = new UserDocument() { Roles = new List<UserRoles>() { UserRoles.Operator }, PublicKey = userId };
            timestamp = DateTime.MinValue;
            chunkStorageWriter = CreateIChunkWriter();
            simpleSynchronizationDataStorage = GetSimpleSynchronizationDataStorage(chunkStorageWriter: chunkStorageWriter.Object);
        };

        private Because of = () => simpleSynchronizationDataStorage.SaveUser(user, timestamp);

        It should_store_chunck = () =>
            chunkStorageWriter.Verify(x => x.StoreChunk(Moq.It.Is<SyncItem>(
                s => s.RootId == userId && 
                    s.ItemType == SyncItemType.User && 
                    s.MetaInfo == string.Empty), userId, timestamp), Times.Once);
        
        private static SimpleSynchronizationDataStorage simpleSynchronizationDataStorage;
        private static UserDocument user;
        private static Guid userId = Guid.Parse("1BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static DateTime timestamp;

        private static Mock<IChunkWriter> chunkStorageWriter;

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.Synchronization.SyncStorage;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Synchronization.SimpleSynchronizationDataStorageTests
{
    internal class when_getting_chunk_pairs_created_after : SimpleSynchronizationDataStorageTestContext
    {
        Establish context = () =>
        {
            chunkStorageReader = new Mock<IChunkReader>();
            userStorage = new Mock<IQueryableReadSideRepositoryReader<UserDocument>>();
            userStorage.Setup(x => x.Query(Moq.It.IsAny<Func<IQueryable<UserDocument>, IQueryable<UserDocument>>>()))
                .Returns(new[] { new UserDocument() 
                { Roles = new List<UserRoles>() { UserRoles.Operator }, 
                  PublicKey = userId, 
                  Supervisor = new UserLight(superId, "bob") } }.AsQueryable());

            userStorage.Setup(x => x.Query(Moq.It.IsAny<Func<IQueryable<UserDocument>, IQueryable<Guid>>>()))
                .Returns(new[] { userId }.AsQueryable());

            simpleSynchronizationDataStorage = GetSimpleSynchronizationDataStorage(userStorage: userStorage.Object, chunkStorageReader: chunkStorageReader.Object);
        };

        private Because of = () => simpleSynchronizationDataStorage.GetChunkPairsCreatedAfter(id, userId);

        It should_get_chunk_meta_data_created_after = () =>
            chunkStorageReader.Verify(x => x.GetChunkMetaDataCreatedAfter(id, Moq.It.IsAny<IEnumerable<Guid>>()), Times.Once);

        private static SimpleSynchronizationDataStorage simpleSynchronizationDataStorage;
        private static Guid userId = Guid.Parse("1BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid superId = Guid.Parse("1BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBA");

        private static string id = "11";
        private static Mock<IChunkReader> chunkStorageReader;

        private static Mock<IQueryableReadSideRepositoryReader<UserDocument>> userStorage;


    }
}

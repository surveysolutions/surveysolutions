using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.Synchronization.Tests
{
    [TestFixture]
    public class SimpleSynchronizationDataStorageTests
    {
        [Test]
        public void SaveQuestionnarie_When_questionnarie_is_valied_Then_questionnarie_returned()
        {
            // arrange
            var questionnarieId = Guid.Parse("23333333-3333-3333-3333-333333333333");

            var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var supervisorId = Guid.Parse("22222222-2222-2222-2222-222222222222");
           

            var documentStorageMock = new Mock<IQueryableReadSideRepositoryReader<CompleteQuestionnaireStoreDocument>>();
            documentStorageMock.Setup(x => x.GetById(questionnarieId)).Returns(new CompleteQuestionnaireStoreDocument() { Responsible = new UserLight(userId,"test") });
            SimpleSynchronizationDataStorage target = CreateSimpleSynchronizationDataStorageWithOneSupervisor(supervisorId, documentStorageMock.Object);

            target.SaveUser(new UserDocument()
            {
                PublicKey = userId,
                Roles = new List<UserRoles>() { UserRoles.Operator },
                Supervisor = new UserLight(supervisorId, "")
            });

            // act
            target.SaveQuestionnarie(questionnarieId, userId);

            // assert
            var result = target.GetLatestVersion(questionnarieId, userId);
            Assert.That(result.ItemType, Is.EqualTo(SyncItemType.Questionnare));
            Assert.That(result.Id, Is.EqualTo(questionnarieId));
            Assert.That(result.IsCompressed, Is.EqualTo(true));
        }

        [Test]
        public void SaveUser_When_quesr_is_valid_Then_user_is_returned()
        {
            // arrange
            var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var supervisorId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var userName = "testUser";
            var testpassword = "testPassword";

            SimpleSynchronizationDataStorage target = CreateSimpleSynchronizationDataStorageWithOneSupervisor(supervisorId);

            // act
            target.SaveUser(new UserDocument()
                {
                    PublicKey = userId,
                    UserName = userName,
                    Password = testpassword,
                    Roles = new List<UserRoles>() {UserRoles.Operator},
                    Supervisor = new UserLight(supervisorId, "")
                });

            // assert
            var result = target.GetLatestVersion(userId, userId);
            Assert.That(result.ItemType, Is.EqualTo(SyncItemType.User));
            Assert.That(result.Id, Is.EqualTo(userId));
            Assert.That(result.IsCompressed, Is.EqualTo(true));
        }

        private SimpleSynchronizationDataStorage CreateSimpleSynchronizationDataStorageWithOneSupervisor(Guid supervisorId, IQueryableReadSideRepositoryReader<CompleteQuestionnaireStoreDocument> documentStorage)
        {
            var inmemoryChunkStorage = new InMemoryChunkStorage();
            var chunkFactoryMock = new Mock<IChunkStorageFactory>();
            chunkFactoryMock.Setup(x => x.GetStorage(It.IsAny<Guid>())).Returns(inmemoryChunkStorage);
            var retval=
                new SimpleSynchronizationDataStorage(documentStorage,
                    chunkFactoryMock.Object);

            var supervisorName = "supe";

            retval.SaveUser(new UserDocument()
            {
                PublicKey = supervisorId,
                UserName = supervisorName,
                Roles = new List<UserRoles>() { UserRoles.Supervisor }
            });
            return retval;
        }
        private SimpleSynchronizationDataStorage CreateSimpleSynchronizationDataStorageWithOneSupervisor(Guid supervisorId)
        {
            return
                CreateSimpleSynchronizationDataStorageWithOneSupervisor(supervisorId,
                    new Mock<IQueryableReadSideRepositoryReader<CompleteQuestionnaireStoreDocument>>().Object);

        }
    }
}

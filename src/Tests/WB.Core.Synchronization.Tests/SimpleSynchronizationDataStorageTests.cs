using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Moq;
using NUnit.Framework;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
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
            var questionnarieId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var documentStorageMock = new Mock<IQueryableReadSideRepositoryReader<CompleteQuestionnaireStoreDocument>>();
            documentStorageMock.Setup(x => x.GetById(questionnarieId)).Returns(new CompleteQuestionnaireStoreDocument());
            SimpleSynchronizationDataStorage target = CreateSimpleSynchronizationDataStorage(documentStorageMock.Object);

            // act
            target.SaveQuestionnarie(questionnarieId);

            // assert
            var result= target.GetLatestVersion(questionnarieId);
            Assert.That(result.ItemType, Is.EqualTo(SyncItemType.Questionnare));
            Assert.That(result.Id, Is.EqualTo(questionnarieId));
            Assert.That(result.IsCompressed, Is.EqualTo(true));
        }

        [Test]
        public void SaveUser_When_quesr_is_valid_Then_user_is_returned()
        {
            // arrange
            var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var userName = "testUser";
            var testpassword = "testPassword";
            
            SimpleSynchronizationDataStorage target = CreateSimpleSynchronizationDataStorage();

            // act
            target.SaveUser(new UserDocument(){PublicKey = userId,UserName = userName, Password = testpassword});

            // assert
            var result = target.GetLatestVersion(userId);
            Assert.That(result.ItemType, Is.EqualTo(SyncItemType.User));
            Assert.That(result.Id, Is.EqualTo(userId));
            Assert.That(result.IsCompressed, Is.EqualTo(true));
        }

        private SimpleSynchronizationDataStorage CreateSimpleSynchronizationDataStorage(IQueryableReadSideRepositoryReader<CompleteQuestionnaireStoreDocument> documentStorage)
        {
            return
                new SimpleSynchronizationDataStorage(documentStorage,
                    new InMemoryChunkStorage());
        }
        private SimpleSynchronizationDataStorage CreateSimpleSynchronizationDataStorage()
        {
            return
                CreateSimpleSynchronizationDataStorage(
                    new Mock<IQueryableReadSideRepositoryReader<CompleteQuestionnaireStoreDocument>>().Object);

        }
    }
}

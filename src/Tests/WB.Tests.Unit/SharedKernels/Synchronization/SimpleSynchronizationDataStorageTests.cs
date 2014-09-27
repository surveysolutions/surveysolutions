using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Utility;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.Synchronization.MetaInfo;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Tests.Unit.SharedKernels.Synchronization
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

            SimpleSynchronizationDataStorage target = CreateSimpleSynchronizationDataStorageWithOneSupervisorAndOneUser(supervisorId, userId);

            // act
            target.SaveInterview(
                new InterviewSynchronizationDto(questionnarieId, InterviewStatus.Created, null, userId, Guid.NewGuid(),1, null, null, null,null,
                                                null, null, null, false), userId, DateTime.Now);

            // assert
            var result = target.GetLatestVersion(questionnarieId);
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
                }, DateTime.Now);

            // assert
            var result = target.GetLatestVersion(userId);
            Assert.That(result.ItemType, Is.EqualTo(SyncItemType.User));
            Assert.That(result.Id, Is.EqualTo(userId));
            Assert.That(result.IsCompressed, Is.EqualTo(true));
        }

        [Test]
        public void DeleteQuestionnarie_When_questionnarie_is_valid_Then_last_stored_chunk_by_questionnarie_is_command_For_delete()
        {
            // arrange
            var questionnarieId = Guid.Parse("23333333-3333-3333-3333-333333333333");
            var supervisorId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            SimpleSynchronizationDataStorage target = CreateSimpleSynchronizationDataStorageWithOneSupervisorAndOneUser(supervisorId, userId);
            
            // act
            target.MarkInterviewForClientDeleting(questionnarieId, userId, DateTime.Now);

            // assert
            var result = target.GetLatestVersion(questionnarieId);
            Assert.That(result.ItemType, Is.EqualTo(SyncItemType.DeleteQuestionnare));
            Assert.That(result.Id, Is.EqualTo(questionnarieId));
            Assert.That(result.Content, Is.EqualTo(questionnarieId.ToString()));
        }

        [Test]
        public void SaveQuestionnaire_When_questionnarie_is_census_mode_Then_last_stored_chunk_by_questionnarie_is_command_For_create_questionnaire()
        {
            // arrange
            var serviceLocatorMock = new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock };
            ServiceLocator.SetLocatorProvider(() => serviceLocatorMock.Object);

            var questionnarieId = Guid.Parse("23333333-3333-3333-3333-333333333333");
            var supervisorId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");


            SimpleSynchronizationDataStorage target = CreateSimpleSynchronizationDataStorageWithOneSupervisorAndOneUser(supervisorId, userId);

            // act
            target.SaveQuestionnaire(new QuestionnaireDocument() { PublicKey = questionnarieId }, 1, true, DateTime.Now);

            // assert
            var packageId = questionnarieId.Combine(1);
            var result = target.GetLatestVersion(packageId);
            var metaInformation = JsonConvert.DeserializeObject<QuestionnaireMetadata>(result.MetaInfo);
            Assert.That(result.ItemType, Is.EqualTo(SyncItemType.Template));
            Assert.That(result.Id, Is.EqualTo(packageId));
            Assert.That(result.IsCompressed, Is.EqualTo(true));
            Assert.That(metaInformation.AllowCensusMode, Is.EqualTo(true));
        }

        [Test]
        public void SaveQuestionnaire_When_questionnarie_is_deleted_Then_last_stored_chunk_by_questionnarie_is_command_For_create_questionnaire_but_not_marked_as_deleted()
        {
            // arrange
            var serviceLocatorMock = new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock };
            ServiceLocator.SetLocatorProvider(() => serviceLocatorMock.Object);

            var questionnarieId = Guid.Parse("23333333-3333-3333-3333-333333333333");
            var supervisorId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");


            SimpleSynchronizationDataStorage target = CreateSimpleSynchronizationDataStorageWithOneSupervisorAndOneUser(supervisorId, userId);

            // act
            target.SaveQuestionnaire(new QuestionnaireDocument() { PublicKey = questionnarieId, IsDeleted = true}, 1, true, DateTime.Now);

            // assert
            var packageId = questionnarieId.Combine(1);
            var result = target.GetLatestVersion(packageId);
            var metaInformation = JsonConvert.DeserializeObject<QuestionnaireMetadata>(result.MetaInfo);
            var questionnaire = JsonConvert.DeserializeObject<QuestionnaireDocument>(result.Content);
            Assert.That(result.ItemType, Is.EqualTo(SyncItemType.Template));
            Assert.That(result.Id, Is.EqualTo(packageId));
            Assert.That(result.IsCompressed, Is.EqualTo(true));
            Assert.That(questionnaire.IsDeleted,Is.EqualTo(false));
            Assert.That(metaInformation.AllowCensusMode, Is.EqualTo(true));
        }

        [Test]
        public void DeleteQuestionnaire_When_questionnarie_is_census_mode_Then_last_stored_chunk_by_questionnarie_is_command_For_delete_questionnaire()
        {
            // arrange
            var serviceLocatorMock = new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock };
            ServiceLocator.SetLocatorProvider(() => serviceLocatorMock.Object);

            var questionnarieId = Guid.Parse("23333333-3333-3333-3333-333333333333");
            var supervisorId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");


            SimpleSynchronizationDataStorage target = CreateSimpleSynchronizationDataStorageWithOneSupervisorAndOneUser(supervisorId, userId);

            // act
            target.DeleteQuestionnaire(questionnarieId, 1, DateTime.Now);

            // assert
            var packageId = questionnarieId.Combine(1);
            var result = target.GetLatestVersion(packageId);
            Assert.That(result.ItemType, Is.EqualTo(SyncItemType.DeleteTemplate));
            Assert.That(result.Id, Is.EqualTo(packageId));
            Assert.That(result.IsCompressed, Is.EqualTo(true));
        }

        [Test]
        public void SaveQuestionnaire_When_questionnarie_is_not_in_census_mode_Then_last_stored_chunk_by_questionnarie_is_command_For_create_questionnaire()
        {
            // arrange
            var serviceLocatorMock = new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock };
            ServiceLocator.SetLocatorProvider(() => serviceLocatorMock.Object);

            var questionnarieId = Guid.Parse("23333333-3333-3333-3333-333333333333");
            var supervisorId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");


            SimpleSynchronizationDataStorage target = CreateSimpleSynchronizationDataStorageWithOneSupervisorAndOneUser(supervisorId, userId);

            // act
            target.SaveQuestionnaire(new QuestionnaireDocument() { PublicKey = questionnarieId }, 1, false, DateTime.Now);

            // assert
            var packageId = questionnarieId.Combine(1);
            var result = target.GetLatestVersion(packageId);
            var metaInformation = JsonConvert.DeserializeObject<QuestionnaireMetadata>(result.MetaInfo);
            Assert.That(result.ItemType, Is.EqualTo(SyncItemType.Template));
            Assert.That(result.Id, Is.EqualTo(packageId));
            Assert.That(result.IsCompressed, Is.EqualTo(true));
            Assert.That(metaInformation.AllowCensusMode, Is.EqualTo(false));
        }


        [Test]
        public void SaveTemplateAssembly_()
        {
            // arrange
            var serviceLocatorMock = new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock };
            ServiceLocator.SetLocatorProvider(() => serviceLocatorMock.Object);

            var questionnarieId = Guid.Parse("23333333-3333-3333-3333-333333333333");
            var supervisorId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var version = 3;
            var assemblyAsBase64String = "test_assembly";


            SimpleSynchronizationDataStorage target = CreateSimpleSynchronizationDataStorageWithOneSupervisorAndOneUser(supervisorId, userId);

            // act
            target.SaveTemplateAssembly(questionnarieId, version, assemblyAsBase64String, DateTime.Now);

            // assert
            var packageId = questionnarieId.Combine(version);
            var result = target.GetLatestVersion(packageId);
            var metaInformation = JsonConvert.DeserializeObject<QuestionnaireAssemblyMetadata>(result.MetaInfo);

            Assert.That(result.ItemType, Is.EqualTo(SyncItemType.QuestionnaireAssembly));
            Assert.That(result.Id, Is.EqualTo(packageId));
            Assert.That(result.IsCompressed, Is.EqualTo(false));
            Assert.That(result.Content, Is.EqualTo(assemblyAsBase64String));
            Assert.That(metaInformation.Version, Is.EqualTo(version));
        }

        private SimpleSynchronizationDataStorage CreateSimpleSynchronizationDataStorageWithOneSupervisor(Guid supervisorId)
        {
            return CreateSimpleSynchronizationDataStorageWithOneSupervisorAndOneUser(supervisorId, Guid.NewGuid());
        }

        private SimpleSynchronizationDataStorage CreateSimpleSynchronizationDataStorageWithOneSupervisorAndOneUser(Guid supervisorId, Guid userId, IMetaInfoBuilder metaInfoBuilder=null)
        {
            var inmemoryChunkStorage = new InMemoryChunkStorage();
            var userStorageMock = new Mock<IQueryableReadSideRepositoryWriter<UserDocument>>();

            var retval =
                new SimpleSynchronizationDataStorage(userStorageMock.Object, inmemoryChunkStorage, inmemoryChunkStorage,
                                                     metaInfoBuilder ?? Mock.Of<IMetaInfoBuilder>());

            retval.SaveUser(new UserDocument()
            {
                PublicKey = userId,
                Roles = new List<UserRoles>() { UserRoles.Operator },
                Supervisor = new UserLight(supervisorId, "")
            }, DateTime.Now);
            retval.SaveUser(new UserDocument()
            {
                PublicKey = supervisorId,
                Roles = new List<UserRoles>() { UserRoles.Supervisor }
            }, DateTime.Now);
            return retval;
        }
        
        
    }
}

using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Implementation.Services.QuestionnairePostProcessors;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Implementation;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer
{
    [TestOf(typeof(ListViewPostProcessor))]
    [TestFixture]
    public class QuestionnaireListViewPostProcessorTests
    {
        [Test]
        public void When_CloneQuestionnaire_command()
        {
            // arrange
            AssemblyContext.SetupServiceLocator();
            var listViewItemsStorage = new TestPlainStorage<QuestionnaireListViewItem>();
            Setup.InstanceToMockedServiceLocator<IPlainStorageAccessor<QuestionnaireListViewItem>>(listViewItemsStorage);


            Guid questionnaireId = Guid.NewGuid();
            Guid creatorId = Guid.NewGuid();
            Guid? clonnerId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            string userName = "testname";

            var accountStorage = new TestInMemoryWriter<AccountDocument>();
            accountStorage.Store(new AccountDocument() {UserName = userName}, clonnerId.FormatGuid());
            Setup.InstanceToMockedServiceLocator<IReadSideRepositoryWriter<AccountDocument>>(accountStorage);

            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(questionnaireId);
            questionnaire.CreatedBy = creatorId;

            var command = new CloneQuestionnaire(questionnaireId, "title", clonnerId.Value, true,
                questionnaire);
            
            var listViewPostProcessor = CreateListViewPostProcessor();
            // act
            listViewPostProcessor.Process(null, command);

            // assert
            var questionnaireListViewItem = listViewItemsStorage.GetById(questionnaireId.FormatGuid());
            Assert.That(questionnaireListViewItem, Is.Not.Null);
            Assert.That(questionnaireListViewItem.Title, Is.EqualTo(command.Title));
            Assert.That(questionnaireListViewItem.IsPublic, Is.EqualTo(command.IsPublic));
            Assert.That(questionnaireListViewItem.CreatedBy, Is.EqualTo(command.ResponsibleId));
            Assert.That(questionnaireListViewItem.CreatorName, Is.EqualTo(userName));
        }

        [Test]
        public void When_CreateQuestionnaire_command()
        {
            // arrange
            AssemblyContext.SetupServiceLocator();
            var listViewItemsStorage = new TestPlainStorage<QuestionnaireListViewItem>();
            Setup.InstanceToMockedServiceLocator<IPlainStorageAccessor<QuestionnaireListViewItem>>(listViewItemsStorage);

            Guid questionnaireId = Guid.NewGuid();
            var command = new CreateQuestionnaire(questionnaireId, "title", Guid.NewGuid(), true);

            var listViewPostProcessor = CreateListViewPostProcessor();
            // act
            listViewPostProcessor.Process(null, command);

            // assert
            var questionnaireListViewItem = listViewItemsStorage.GetById(questionnaireId.FormatGuid());
            Assert.That(questionnaireListViewItem, Is.Not.Null);
            Assert.That(questionnaireListViewItem.Title, Is.EqualTo(command.Title));
            Assert.That(questionnaireListViewItem.IsPublic, Is.EqualTo(command.IsPublic));
            Assert.That(questionnaireListViewItem.CreatedBy, Is.EqualTo(command.ResponsibleId));
        }

        [Test]
        public void When_UpdateQuestionnaire_command()
        {
            // arrange
            Guid questionnaireId = Guid.NewGuid();
            var command = new UpdateQuestionnaire(questionnaireId, "title", true, Guid.NewGuid(), false);

            AssemblyContext.SetupServiceLocator();
            var listViewItemsStorage = new TestPlainStorage<QuestionnaireListViewItem>();
            listViewItemsStorage.Store(new QuestionnaireListViewItem { QuestionnaireId = questionnaireId.FormatGuid() }, questionnaireId.FormatGuid());
            Setup.InstanceToMockedServiceLocator<IPlainStorageAccessor<QuestionnaireListViewItem>>(listViewItemsStorage);

            var listViewPostProcessor = CreateListViewPostProcessor();
            // act
            listViewPostProcessor.Process(null, command);

            // assert
            var questionnaireListViewItem = listViewItemsStorage.GetById(questionnaireId.FormatGuid());
            Assert.That(questionnaireListViewItem, Is.Not.Null);
            Assert.That(questionnaireListViewItem.Title, Is.EqualTo(command.Title));
            Assert.That(questionnaireListViewItem.IsPublic, Is.EqualTo(command.IsPublic));
        }

        [Test]
        public void When_DeleteQuestionnaire_command()
        {
            // arrange
            AssemblyContext.SetupServiceLocator();

            Guid questionnaireId = Guid.NewGuid();
            var command = new DeleteQuestionnaire(questionnaireId, Guid.NewGuid());

            var listViewItemsStorage = new TestPlainStorage<QuestionnaireListViewItem>();
            listViewItemsStorage.Store(new QuestionnaireListViewItem {QuestionnaireId = questionnaireId.FormatGuid()}, questionnaireId.FormatGuid());
            Setup.InstanceToMockedServiceLocator<IPlainStorageAccessor<QuestionnaireListViewItem>>(listViewItemsStorage);
            
            var listViewPostProcessor = CreateListViewPostProcessor();
            // act
            listViewPostProcessor.Process(null, command);

            // assert
            var questionnaireListViewItem = listViewItemsStorage.GetById(questionnaireId.FormatGuid());
            Assert.That(questionnaireListViewItem.IsDeleted, Is.EqualTo(true));
        }

        [Test]
        public void When_AddSharedPerson_command()
        {
            // arrange
            AssemblyContext.SetupServiceLocator();

            Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
            Guid responsibleId = Guid.Parse("22222222222222222222222222222222");
            Guid questionnaireOwnerId = Guid.Parse("44444444444444444444444444444444");
            Guid sharedWithUserId = Guid.Parse("33333333333333333333333333333333");
            var questionnaireOwner = new AccountDocument { UserName = "questionnaire owner name", Email = "questionnaire owner email" };
            var questionnaireIdFormatted = questionnaireId.FormatGuid();
            var command = new AddSharedPersonToQuestionnaire(questionnaireId, sharedWithUserId, "test@test.com", ShareType.View, responsibleId);

            var listViewItemsStorage = new TestPlainStorage<QuestionnaireListViewItem>();
            
            var listViewItem = new QuestionnaireListViewItem { QuestionnaireId = questionnaireIdFormatted, Title = "title", CreatedBy = questionnaireOwnerId};
            listViewItemsStorage.Store(listViewItem, questionnaireIdFormatted);
            Setup.InstanceToMockedServiceLocator<IPlainStorageAccessor<QuestionnaireListViewItem>>(listViewItemsStorage);

            var sharedPersonsStorage = new InMemoryKeyValueStorage<QuestionnaireSharedPersons>();
            Setup.InstanceToMockedServiceLocator<IPlainKeyValueStorage<QuestionnaireSharedPersons>>(sharedPersonsStorage);

            var accountStorage = new TestInMemoryWriter<AccountDocument>();
            Setup.InstanceToMockedServiceLocator<IReadSideRepositoryWriter<AccountDocument>>(accountStorage);
            
            accountStorage.Store(questionnaireOwner, questionnaireOwnerId);

            var mockOfEmailNotifier = new Mock<IRecipientNotifier>();
            Setup.InstanceToMockedServiceLocator(mockOfEmailNotifier.Object);

            var listViewPostProcessor = CreateListViewPostProcessor();
            // act
            listViewPostProcessor.Process(null, command);

            // assert
            var questionnaireListViewItem = listViewItemsStorage.GetById(questionnaireIdFormatted);
            Assert.That(questionnaireListViewItem.SharedPersons, Is.Not.Null);
            Assert.That(questionnaireListViewItem.SharedPersons.First(), Is.EqualTo(sharedWithUserId));

            var sharedPersonsByQuestionnaire = sharedPersonsStorage.GetById(questionnaireIdFormatted);
            Assert.That(sharedPersonsByQuestionnaire, Is.Not.Null);
            Assert.That(sharedPersonsByQuestionnaire.SharedPersons, Is.Not.Null);
            Assert.That(sharedPersonsByQuestionnaire.SharedPersons[0].Id, Is.EqualTo(command.PersonId));
            Assert.That(sharedPersonsByQuestionnaire.SharedPersons[0].Email, Is.EqualTo(command.Email));
            Assert.That(sharedPersonsByQuestionnaire.SharedPersons[0].ShareType, Is.EqualTo(command.ShareType));

            mockOfEmailNotifier.Verify(
                x => x.NotifyTargetPersonAboutShareChange(ShareChangeType.Share, command.Email, It.IsAny<string>(),
                    questionnaireIdFormatted, listViewItem.Title, command.ShareType, It.IsAny<string>()), Times.Once);

            mockOfEmailNotifier.Verify(
                x => x.NotifyOwnerAboutShareChange(ShareChangeType.Share, questionnaireOwner.Email, questionnaireOwner.UserName,
                    questionnaireIdFormatted, listViewItem.Title, command.ShareType, It.IsAny<string>(), command.Email), Times.Once);
        }

        [Test]
        public void When_RemoveSharedPerson_command()
        {
            // arrange
            AssemblyContext.SetupServiceLocator();

            Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
            Guid responsibleId = Guid.Parse("22222222222222222222222222222222");
            Guid questionnaireOwnerId = Guid.Parse("44444444444444444444444444444444");
            Guid sharedWithUserId = Guid.Parse("33333333333333333333333333333333");
            var questionnaireOwner = new AccountDocument { UserName = "questionnaire owner name", Email = "questionnaire owner email" };
            var questionnaireIdFormatted = questionnaireId.FormatGuid();
            var command = new RemoveSharedPersonFromQuestionnaire(questionnaireId, sharedWithUserId, "test@test.com", responsibleId);

            var listViewItemsStorage = new TestPlainStorage<QuestionnaireListViewItem>();

            var listViewItem = new QuestionnaireListViewItem { QuestionnaireId = questionnaireIdFormatted, Title = "title", CreatedBy = questionnaireOwnerId };
            listViewItemsStorage.Store(listViewItem, questionnaireIdFormatted);
            Setup.InstanceToMockedServiceLocator<IPlainStorageAccessor<QuestionnaireListViewItem>>(listViewItemsStorage);

            var sharedPersonsStorage = new InMemoryKeyValueStorage<QuestionnaireSharedPersons>();
            sharedPersonsStorage.Store(new QuestionnaireSharedPersons(questionnaireId) {SharedPersons = { new SharedPerson {Id = sharedWithUserId}}}, questionnaireIdFormatted);
            Setup.InstanceToMockedServiceLocator<IPlainKeyValueStorage<QuestionnaireSharedPersons>>(sharedPersonsStorage);

            var accountStorage = new TestInMemoryWriter<AccountDocument>();
            Setup.InstanceToMockedServiceLocator<IReadSideRepositoryWriter<AccountDocument>>(accountStorage);

            accountStorage.Store(questionnaireOwner, questionnaireOwnerId);

            var mockOfEmailNotifier = new Mock<IRecipientNotifier>();
            Setup.InstanceToMockedServiceLocator(mockOfEmailNotifier.Object);

            var listViewPostProcessor = CreateListViewPostProcessor();
            // act
            listViewPostProcessor.Process(null, command);

            // assert
            var questionnaireListViewItem = listViewItemsStorage.GetById(questionnaireIdFormatted);
            Assert.That(questionnaireListViewItem.SharedPersons, Is.Empty);

            var sharedPersonsByQuestionnaire = sharedPersonsStorage.GetById(questionnaireIdFormatted);
            Assert.That(sharedPersonsByQuestionnaire.SharedPersons, Is.Empty);

            mockOfEmailNotifier.Verify(
                x => x.NotifyTargetPersonAboutShareChange(ShareChangeType.StopShare, command.Email, It.IsAny<string>(),
                    questionnaireIdFormatted, listViewItem.Title, ShareType.Edit, It.IsAny<string>()), Times.Once);

            mockOfEmailNotifier.Verify(
                x => x.NotifyOwnerAboutShareChange(ShareChangeType.StopShare, questionnaireOwner.Email, questionnaireOwner.UserName,
                    questionnaireIdFormatted, listViewItem.Title, ShareType.Edit, It.IsAny<string>(), command.Email), Times.Once);
        }

        private static ListViewPostProcessor CreateListViewPostProcessor() => new ListViewPostProcessor();
    }
}
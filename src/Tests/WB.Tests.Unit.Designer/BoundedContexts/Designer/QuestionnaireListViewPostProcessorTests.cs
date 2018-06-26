using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Implementation.Services.QuestionnairePostProcessors;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;

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

            var accountStorage = new TestPlainStorage<User>();
            accountStorage.Store(new User {UserName = userName}, clonnerId.FormatGuid());
            Setup.InstanceToMockedServiceLocator<IPlainStorageAccessor<User>>(accountStorage);

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
            var command = Create.Command.CreateQuestionnaire(questionnaireId, "title", Guid.NewGuid(), true);

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
            var command = new UpdateQuestionnaire(questionnaireId, "title", "questionnaire", true, Guid.NewGuid(), false);

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
            var questionnaireOwner = new User { UserName = "questionnaire owner name", Email = "questionnaire owner email" };
            var questionnaireIdFormatted = questionnaireId.FormatGuid();
            var sharedWithEmail = "test@test.com";
            var shareType = ShareType.View;
            var command = new AddSharedPersonToQuestionnaire(questionnaireId, sharedWithUserId, sharedWithEmail, shareType, responsibleId);

            var listViewItemsStorage = new TestPlainStorage<QuestionnaireListViewItem>();
            
            var listViewItem = new QuestionnaireListViewItem { QuestionnaireId = questionnaireIdFormatted, Title = "title", CreatedBy = questionnaireOwnerId};
            listViewItemsStorage.Store(listViewItem, questionnaireIdFormatted);
            Setup.InstanceToMockedServiceLocator<IPlainStorageAccessor<QuestionnaireListViewItem>>(listViewItemsStorage);

            var accountStorage = new TestPlainStorage<User>();
            Setup.InstanceToMockedServiceLocator<IPlainStorageAccessor<User>>(accountStorage);
            
            accountStorage.Store(questionnaireOwner, questionnaireOwnerId.FormatGuid());

            var mockOfEmailNotifier = new Mock<IRecipientNotifier>();
            Setup.InstanceToMockedServiceLocator(mockOfEmailNotifier.Object);

            var listViewPostProcessor = CreateListViewPostProcessor();
            // act
            listViewPostProcessor.Process(null, command);

            // assert
            var questionnaireListViewItem = listViewItemsStorage.GetById(questionnaireIdFormatted);
            Assert.That(questionnaireListViewItem.SharedPersons, Is.Not.Null);
            Assert.That(questionnaireListViewItem.SharedPersons.First().UserId, Is.EqualTo(sharedWithUserId));
            Assert.That(questionnaireListViewItem.SharedPersons.First().Email, Is.EqualTo(sharedWithEmail));
            Assert.That(questionnaireListViewItem.SharedPersons.First().IsOwner, Is.False);
            Assert.That(questionnaireListViewItem.SharedPersons.First().ShareType, Is.EqualTo(shareType));

            mockOfEmailNotifier.Verify(
                x => x.NotifyTargetPersonAboutShareChange(ShareChangeType.Share, command.EmailOrLogin, It.IsAny<string>(),
                    questionnaireIdFormatted, listViewItem.Title, command.ShareType, It.IsAny<string>()), Times.Once);

            mockOfEmailNotifier.Verify(
                x => x.NotifyOwnerAboutShareChange(ShareChangeType.Share, questionnaireOwner.Email, questionnaireOwner.UserName,
                    questionnaireIdFormatted, listViewItem.Title, command.ShareType, It.IsAny<string>(), command.EmailOrLogin), Times.Once);
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
            var questionnaireOwner = new User { UserName = "questionnaire owner name", Email = "questionnaire owner email" };
            var questionnaireIdFormatted = questionnaireId.FormatGuid();
            var command = new RemoveSharedPersonFromQuestionnaire(questionnaireId, sharedWithUserId, "test@test.com", responsibleId);

            var listViewItemsStorage = new TestPlainStorage<QuestionnaireListViewItem>();

            var listViewItem = new QuestionnaireListViewItem { QuestionnaireId = questionnaireIdFormatted, Title = "title", CreatedBy = questionnaireOwnerId };
            listViewItemsStorage.Store(listViewItem, questionnaireIdFormatted);
            Setup.InstanceToMockedServiceLocator<IPlainStorageAccessor<QuestionnaireListViewItem>>(listViewItemsStorage);

            var accountStorage = new TestPlainStorage<User>();
            Setup.InstanceToMockedServiceLocator<IPlainStorageAccessor<User>>(accountStorage);

            accountStorage.Store(questionnaireOwner, questionnaireOwnerId.FormatGuid());

            var mockOfEmailNotifier = new Mock<IRecipientNotifier>();
            Setup.InstanceToMockedServiceLocator(mockOfEmailNotifier.Object);

            var listViewPostProcessor = CreateListViewPostProcessor();
            // act
            listViewPostProcessor.Process(null, command);

            // assert
            var questionnaireListViewItem = listViewItemsStorage.GetById(questionnaireIdFormatted);
            Assert.That(questionnaireListViewItem.SharedPersons, Is.Empty);

            mockOfEmailNotifier.Verify(
                x => x.NotifyTargetPersonAboutShareChange(ShareChangeType.StopShare, command.Email, It.IsAny<string>(),
                    questionnaireIdFormatted, listViewItem.Title, ShareType.Edit, It.IsAny<string>()), Times.Once);

            mockOfEmailNotifier.Verify(
                x => x.NotifyOwnerAboutShareChange(ShareChangeType.StopShare, questionnaireOwner.Email, questionnaireOwner.UserName,
                    questionnaireIdFormatted, listViewItem.Title, ShareType.Edit, It.IsAny<string>(), command.Email), Times.Once);
        }

        [Test]
        public void When_RevertVersionQuestionnaire_command()
        {
            // arrange
            Guid questionnaireId    = Guid.Parse("11111111111111111111111111111111");
            Guid historyReferanceId = Guid.Parse("22222222222222222222222222222222");
            Guid responsibleId      = Guid.Parse("33333333333333333333333333333333");
            var command = Create.Command.RevertVersionQuestionnaire(questionnaireId, historyReferanceId, responsibleId);

            AssemblyContext.SetupServiceLocator();
            var listViewItemsStorage = new TestPlainStorage<QuestionnaireListViewItem>();
            listViewItemsStorage.Store(new QuestionnaireListViewItem
            {
                QuestionnaireId = questionnaireId.FormatGuid(),
                Title = "old title",
                IsPublic = true
            }, questionnaireId.FormatGuid());
            Setup.InstanceToMockedServiceLocator<IPlainStorageAccessor<QuestionnaireListViewItem>>(listViewItemsStorage);

            var questionnaireDocument = Create.QuestionnaireDocument(questionnaireId, "reverted title");
            questionnaireDocument.IsPublic = false;
            var questionnaire = Create.Questionnaire(responsibleId, questionnaireDocument);

            var listViewPostProcessor = CreateListViewPostProcessor();
            // act
            listViewPostProcessor.Process(questionnaire, command);

            // assert
            var questionnaireListViewItem = listViewItemsStorage.GetById(questionnaireId.FormatGuid());
            Assert.That(questionnaireListViewItem, Is.Not.Null);
            Assert.That(questionnaireListViewItem.Title, Is.EqualTo("reverted title"));
            Assert.That(questionnaireListViewItem.IsPublic, Is.False);
        }

        [Test]
        public void When_ImportQuestionnaire_command_for_existed_questionnaire()
        {
            // arrange
            AssemblyContext.SetupServiceLocator();
            var listViewItemsStorage = new TestPlainStorage<QuestionnaireListViewItem>();
            Setup.InstanceToMockedServiceLocator<IPlainStorageAccessor<QuestionnaireListViewItem>>(listViewItemsStorage);


            Guid questionnaireId = Guid.NewGuid();
            Guid? userId1 = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            Guid? userId2 = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            string userName1 = "first_owner";
            string userName2 = "second_importer";

            var accountStorage = new TestPlainStorage<User>();
            accountStorage.Store(new User { UserName = userName1 }, userId1.FormatGuid());
            accountStorage.Store(new User { UserName = userName2 }, userId2.FormatGuid());
            Setup.InstanceToMockedServiceLocator<IPlainStorageAccessor<User>>(accountStorage);

            var listViewPostProcessor = CreateListViewPostProcessor();
            var listViewItem = Create.QuestionnaireListViewItem(id: questionnaireId, sharedPersons: new[]
            {
                new SharedPerson() {UserId = userId1.Value, IsOwner = true, ShareType = ShareType.Edit},
                new SharedPerson() {UserId = userId2.Value, IsOwner = false, ShareType = ShareType.View},
            });
            listViewItemsStorage.Store(listViewItem, questionnaireId.FormatGuid());
            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(questionnaireId: questionnaireId);
            questionnaire.CreatedBy = userId2;

            var command = new ImportQuestionnaire(userId2.Value, questionnaire);

            // act
            listViewPostProcessor.Process(null, command);

            // assert
            var questionnaireListViewItem = listViewItemsStorage.GetById(questionnaireId.FormatGuid());
            Assert.That(questionnaireListViewItem, Is.Not.Null);
            Assert.That(questionnaireListViewItem.CreatedBy, Is.EqualTo(command.ResponsibleId));
            Assert.That(questionnaireListViewItem.CreatorName, Is.EqualTo(userName2));
            Assert.That(questionnaireListViewItem.SharedPersons.Count(p => p.IsOwner), Is.EqualTo(1));
            Assert.That(questionnaireListViewItem.SharedPersons.Single(p => p.IsOwner).UserId, Is.EqualTo(userId2.Value));
            Assert.That(questionnaireListViewItem.SharedPersons.Single(p => p.IsOwner).ShareType, Is.EqualTo(ShareType.Edit));
        }

        private static ListViewPostProcessor CreateListViewPostProcessor() => new ListViewPostProcessor();
    }
}

using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Implementation.Services.QuestionnairePostProcessors;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer
{
    [TestOf(typeof(ListViewPostProcessor))]
    public class QuestionnaireListViewPostProcessorTests
    {
        [Test]
        public void When_CloneQuestionnaire_command()
        {
            // arrange
            Guid questionnaireId = Guid.NewGuid();
            Guid creatorId = Guid.NewGuid();
            Guid clonnerId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            string userName = "testname";

            var dbContext = Create.InMemoryDbContext();
            dbContext.Users.Add(new DesignerIdentityUser { Id = clonnerId, UserName = userName});
            dbContext.SaveChanges();

            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(questionnaireId);
            questionnaire.CreatedBy = creatorId;

            var command = new CloneQuestionnaire(questionnaireId, "title", clonnerId, true,
                questionnaire);
            
            var listViewPostProcessor = Create.ListViewPostProcessor(dbContext);
            // act
            listViewPostProcessor.Process(null, command);
            dbContext.SaveChanges();

            // assert
            var questionnaireListViewItem = dbContext.Questionnaires.Find(questionnaireId.FormatGuid());
            Assert.That(questionnaireListViewItem, Is.Not.Null);
            Assert.That(questionnaireListViewItem.Title, Is.EqualTo(command.Title));
            Assert.That(questionnaireListViewItem.IsPublic, Is.EqualTo(command.IsPublic));
            Assert.That(questionnaireListViewItem.OwnerId, Is.EqualTo(command.ResponsibleId));
            Assert.That(questionnaireListViewItem.CreatorName, Is.EqualTo(userName));
        }

        [Test]
        public void When_CreateQuestionnaire_command()
        {
            // arrange
            var listViewItemsStorage = Create.InMemoryDbContext();

            Guid questionnaireId = Guid.NewGuid();
            var command = Create.Command.CreateQuestionnaire(questionnaireId, "title", Guid.NewGuid(), true);

            var listViewPostProcessor = Create.ListViewPostProcessor(listViewItemsStorage);
            // act
            listViewPostProcessor.Process(null, command);
            listViewItemsStorage.SaveChanges();

            // assert
            var questionnaireListViewItem = listViewItemsStorage.Questionnaires.Find(questionnaireId.FormatGuid());
            Assert.That(questionnaireListViewItem, Is.Not.Null);
            Assert.That(questionnaireListViewItem.Title, Is.EqualTo(command.Title));
            Assert.That(questionnaireListViewItem.IsPublic, Is.EqualTo(command.IsPublic));
            Assert.That(questionnaireListViewItem.OwnerId, Is.EqualTo(command.ResponsibleId));
        }

        [Test]
        public void When_UpdateQuestionnaire_command()
        {
            // arrange
            Guid questionnaireId = Guid.NewGuid();
            var command = new UpdateQuestionnaire(questionnaireId, "title", "questionnaire", false, true, null, Guid.NewGuid(), false);

            var listViewItemsStorage = Create.InMemoryDbContext();
            listViewItemsStorage.Add(Create.QuestionnaireListViewItem(questionnaireId));
            listViewItemsStorage.SaveChanges();

            var listViewPostProcessor = Create.ListViewPostProcessor(listViewItemsStorage);
            // act
            listViewPostProcessor.Process(null, command);

            // assert
            var questionnaireListViewItem = listViewItemsStorage.Questionnaires.Find(questionnaireId.FormatGuid());
            Assert.That(questionnaireListViewItem, Is.Not.Null);
            Assert.That(questionnaireListViewItem.Title, Is.EqualTo(command.Title));
            Assert.That(questionnaireListViewItem.IsPublic, Is.EqualTo(command.IsPublic));
        }

        [Test]
        public void When_DeleteQuestionnaire_command()
        {
            // arrange

            Guid questionnaireId = Guid.NewGuid();
            var command = new DeleteQuestionnaire(questionnaireId, Guid.NewGuid());

            var listViewItemsStorage = Create.InMemoryDbContext();
            listViewItemsStorage.Add(Create.QuestionnaireListViewItem(id: questionnaireId));
            
            var listViewPostProcessor = Create.ListViewPostProcessor(listViewItemsStorage);
            // act
            listViewPostProcessor.Process(null, command);

            // assert
            var questionnaireListViewItem = listViewItemsStorage.Questionnaires.Find(questionnaireId.FormatGuid());
            Assert.That(questionnaireListViewItem.IsDeleted, Is.EqualTo(true));
        }

        [Test]
        public void When_AddSharedPerson_command()
        {
            // arrange

            Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
            Guid responsibleId = Guid.Parse("22222222222222222222222222222222");
            Guid questionnaireOwnerId = Guid.Parse("44444444444444444444444444444444");
            Guid sharedWithUserId = Guid.Parse("33333333333333333333333333333333");
            var responsible = new DesignerIdentityUser
            {
                Id = responsibleId,
                UserName = "responsible",
                Email = "resp@te.com"
            };

            var questionnaireOwner = new DesignerIdentityUser
            {
                Id = questionnaireOwnerId,
                UserName = "questionnaire owner name",
                Email = "questionnaire owner email"
            };
        
            var questionnaireIdFormatted = questionnaireId.FormatGuid();
            var sharedWithEmail = "test@test.com";
            var shareType = ShareType.View;

            var sharedWith = new DesignerIdentityUser
            {
                Id = sharedWithUserId,
                UserName = "share with",
                Email = sharedWithEmail,
                NormalizedEmail = sharedWithEmail.ToUpper()
            };

            var command = new AddSharedPersonToQuestionnaire(questionnaireId, sharedWithUserId, sharedWithEmail, shareType, responsibleId);

            var listViewItemsStorage = Create.InMemoryDbContext();

            var listViewItem =
                Create.QuestionnaireListViewItem(id: questionnaireId, title: "title", createdBy: questionnaireOwnerId);
            
            listViewItemsStorage.Questionnaires.Add(listViewItem);
            listViewItemsStorage.Users.Add(questionnaireOwner);
            listViewItemsStorage.Users.Add(sharedWith);
            listViewItemsStorage.Users.Add(responsible);
            listViewItemsStorage.SaveChanges();

            var mockOfEmailNotifier = new Mock<IRecipientNotifier>();

            var listViewPostProcessor = Create.ListViewPostProcessor(listViewItemsStorage, 
                emailNotifier: mockOfEmailNotifier.Object);
            // act
            listViewPostProcessor.Process(null, command);

            // assert
            var questionnaireListViewItem = listViewItemsStorage.Questionnaires.Find(questionnaireIdFormatted);
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
        public void When_PassOwnership_Command()
        {
            // arrange

            Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
            Guid responsibleId = Guid.Parse("22222222222222222222222222222222");
            Guid questionnaireOwnerId = Guid.Parse("44444444444444444444444444444444");
            Guid sharedWithUserId = Guid.Parse("33333333333333333333333333333333");
            var sharedWithEmail = "test@test.com";

            var responsible = new DesignerIdentityUser
            {
                Id = responsibleId,
                UserName = "responsible",
                Email = "resp@te.com"
            };
            var questionnaireOwner = new DesignerIdentityUser
            {
                Id = questionnaireOwnerId,
                UserName = "questionnaire owner name",
                Email = "questionnaire owner email"
            };
            var sharedWith = new DesignerIdentityUser
            {
                Id = sharedWithUserId,
                UserName = "share with",
                Email = sharedWithEmail,
                NormalizedEmail = sharedWithEmail.ToUpper()
            };
            var questionnaireIdFormatted = questionnaireId.FormatGuid();
            var command = new PassOwnershipFromQuestionnaire(questionnaireId, questionnaireOwner.Id, 
                sharedWithUserId, questionnaireOwner.Email, sharedWith.Email);
            
            var dbContext = Create.InMemoryDbContext();

            var listViewItem =
                Create.QuestionnaireListViewItem(id: questionnaireId, title: "title", createdBy: questionnaireOwnerId);
            dbContext.Questionnaires.Add(listViewItem);
            dbContext.Users.Add(questionnaireOwner);
            dbContext.Users.Add(responsible);
            dbContext.Users.Add(sharedWith);
            dbContext.SaveChanges();

            var mockOfEmailNotifier = new Mock<IRecipientNotifier>();

            var listViewPostProcessor = Create.ListViewPostProcessor(dbContext, emailNotifier: mockOfEmailNotifier.Object);
            // act
            listViewPostProcessor.Process(null, command);

            // assert
            var questionnaireListViewItem = dbContext.Questionnaires.Find(questionnaireIdFormatted);

            Assert.That(questionnaireListViewItem.OwnerId, Is.EqualTo(sharedWith.Id));

            Assert.That(questionnaireListViewItem.SharedPersons.Single(), Has.Property(nameof(SharedPerson.UserId)).EqualTo(questionnaireOwner.Id));
            Assert.That(questionnaireListViewItem.SharedPersons.Single(), Has.Property(nameof(SharedPerson.ShareType)).EqualTo(ShareType.Edit));

            mockOfEmailNotifier.Verify(
                x => x.NotifyTargetPersonAboutShareChange(ShareChangeType.TransferOwnership, command.NewOwnerEmail, It.IsAny<string>(),
                    questionnaireIdFormatted, listViewItem.Title, ShareType.Edit, It.IsAny<string>()), Times.Once);

            mockOfEmailNotifier.Verify(
                x => x.NotifyTargetPersonAboutShareChange(ShareChangeType.Share, questionnaireOwner.Email, It.IsAny<string>(),
                    questionnaireIdFormatted, listViewItem.Title, ShareType.Edit, It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void When_RemoveSharedPerson_command()
        {
            // arrange

            Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
            Guid responsibleId = Guid.Parse("22222222222222222222222222222222");
            Guid questionnaireOwnerId = Guid.Parse("44444444444444444444444444444444");
            Guid sharedWithUserId = Guid.Parse("33333333333333333333333333333333");
            var sharedWithEmail = "test@test.com";

            var responsible = new DesignerIdentityUser
            {
                Id = responsibleId,
                UserName = "responsible",
                Email = "resp@te.com"
            };
            var questionnaireOwner = new DesignerIdentityUser
            {
                Id = questionnaireOwnerId,
                UserName = "questionnaire owner name",
                Email = "questionnaire owner email"
            };
            var sharedWith = new DesignerIdentityUser
            {
                Id = sharedWithUserId,
                UserName = "share with",
                Email = sharedWithEmail,
                NormalizedEmail = sharedWithEmail.ToUpper()
            };
            var questionnaireIdFormatted = questionnaireId.FormatGuid();
            var command = new RemoveSharedPersonFromQuestionnaire(questionnaireId, sharedWithUserId, sharedWithEmail, responsibleId);

            var dbContext = Create.InMemoryDbContext();

            var listViewItem =
                Create.QuestionnaireListViewItem(id: questionnaireId, title: "title", createdBy: questionnaireOwnerId);
            dbContext.Questionnaires.Add(listViewItem);
            dbContext.Users.Add(questionnaireOwner);
            dbContext.Users.Add(responsible);
            dbContext.Users.Add(sharedWith);
            dbContext.SaveChanges();

            var mockOfEmailNotifier = new Mock<IRecipientNotifier>();

            var listViewPostProcessor = Create.ListViewPostProcessor(dbContext, emailNotifier: mockOfEmailNotifier.Object);
            // act
            listViewPostProcessor.Process(null, command);

            // assert
            var questionnaireListViewItem = dbContext.Questionnaires.Find(questionnaireIdFormatted);
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

            var listViewItemsStorage = Create.InMemoryDbContext();
            listViewItemsStorage.Questionnaires.Add(new QuestionnaireListViewItem
            {
                PublicId = questionnaireId,
                QuestionnaireId = questionnaireId.FormatGuid(),
                Title = "old title",
                IsPublic = true
            });

            var questionnaireDocument = Create.QuestionnaireDocument(questionnaireId, "reverted title");
            questionnaireDocument.IsPublic = false;
            var questionnaire = Create.Questionnaire(responsibleId, questionnaireDocument);

            var listViewPostProcessor = Create.ListViewPostProcessor(listViewItemsStorage);
            // act
            listViewPostProcessor.Process(questionnaire, command);

            // assert
            var questionnaireListViewItem = listViewItemsStorage.Questionnaires.Find(questionnaireId.FormatGuid());
            Assert.That(questionnaireListViewItem, Is.Not.Null);
            Assert.That(questionnaireListViewItem.Title, Is.EqualTo("reverted title"));
            Assert.That(questionnaireListViewItem.IsPublic, Is.False);
        }

        [Test]
        public void When_ImportQuestionnaire_command_for_existed_questionnaire()
        {
            // arrange
            Guid questionnaireId = Guid.NewGuid();
            Guid userId1 = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            Guid userId2 = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            string userName1 = "first_owner";
            string userName2 = "second_importer";

            var dbContext = Create.InMemoryDbContext();
            dbContext.Users.Add(new DesignerIdentityUser { Id = userId1, UserName = userName1 });
            dbContext.Users.Add(new DesignerIdentityUser { Id = userId2, UserName = userName2 });

            var listViewItem = Create.QuestionnaireListViewItem(id: questionnaireId, sharedPersons: new[]
            {
                new SharedPerson() {UserId = userId1, IsOwner = true, ShareType = ShareType.Edit},
                new SharedPerson() {UserId = userId2, IsOwner = false, ShareType = ShareType.View},
            });
            dbContext.Questionnaires.Add(listViewItem);
            dbContext.SaveChanges();

            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(questionnaireId: questionnaireId);
            questionnaire.CreatedBy = userId2;

            var command = new ImportQuestionnaire(userId2, questionnaire);
            var listViewPostProcessor = Create.ListViewPostProcessor(dbContext);

            // act
            listViewPostProcessor.Process(null, command);

            // assert
            var questionnaireListViewItem = dbContext.Questionnaires.Find(questionnaireId.FormatGuid());
            Assert.That(questionnaireListViewItem, Is.Not.Null);
            Assert.That(questionnaireListViewItem.OwnerId, Is.EqualTo(command.ResponsibleId));
            Assert.That(questionnaireListViewItem.CreatorName, Is.EqualTo(userName2));
            Assert.That(questionnaireListViewItem.SharedPersons.Count(p => p.IsOwner), Is.EqualTo(1));
            Assert.That(questionnaireListViewItem.SharedPersons.Single(p => p.IsOwner).UserId, Is.EqualTo(userId2));
            Assert.That(questionnaireListViewItem.SharedPersons.Single(p => p.IsOwner).ShareType, Is.EqualTo(ShareType.Edit));
        }

        private static ListViewPostProcessor CreateListViewPostProcessor() => Create.ListViewPostProcessor();
    }
}

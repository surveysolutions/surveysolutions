using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Attachments;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Translations;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.Implementation.Services.QuestionnairePostProcessors;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Implementation;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Infrastructure.Native.Storage;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer
{
    [TestOf(typeof(HistoryPostProcessor))]
    [TestFixture]
    public class QuestionnaireHistoryPostProcessorTests
    {
        [Test]
        public void When_CloneQuestionnaire_Then_new_history_item_should_be_added_with_specified_parameters()
        {
            // arrange
            Guid questionnaireId = Id.g1;
            Guid responsibleId = Id.g2;
            Guid questionnaireOwner = Id.g3;
            string ownerName = "owner";

            DesignerDbContext dbContext = Create.InMemoryDbContext();
            dbContext.Users.Add(new DesignerIdentityUser { Id = questionnaireOwner, UserName = ownerName});
            dbContext.SaveChanges();

            var questionnaireDocument = Create.QuestionnaireDocument(questionnaireId, userId: questionnaireOwner);
            var questionnaire = Create.Questionnaire();
            questionnaire.Initialize(questionnaireId, questionnaireDocument, Enumerable.Empty<SharedPerson>());
            var command = new CloneQuestionnaire(questionnaireId, "title", responsibleId, true,
                questionnaireDocument);
            
            var historyPostProcessor = Create.HistoryPostProcessor(dbContext, Create.QuestionnireHistoryVersionsService(dbContext));

            // act
            historyPostProcessor.Process(questionnaire, command);
            dbContext.SaveChanges();

            // assert
            var questionnaireHistoryItem = dbContext.QuestionnaireChangeRecords.First(historyItem =>
                    historyItem.QuestionnaireId == questionnaireId.FormatGuid());

            Assert.That(questionnaireHistoryItem, Is.Not.Null);
            Assert.That(questionnaireHistoryItem.QuestionnaireId, Is.EqualTo(command.QuestionnaireId.FormatGuid()));
            Assert.That(questionnaireHistoryItem.ActionType, Is.EqualTo(QuestionnaireActionType.Clone));
            Assert.That(questionnaireHistoryItem.UserId, Is.EqualTo(questionnaireOwner));
            Assert.That(questionnaireHistoryItem.UserName, Is.EqualTo(ownerName));
            Assert.That(questionnaireHistoryItem.Sequence, Is.EqualTo(0));
            Assert.That(questionnaireHistoryItem.TargetItemType, Is.EqualTo(QuestionnaireItemType.Questionnaire));
            Assert.That(questionnaireHistoryItem.TargetItemId, Is.EqualTo(questionnaireId));
            Assert.That(questionnaireHistoryItem.TargetItemTitle, Is.EqualTo(command.Title));
            Assert.That(questionnaireHistoryItem.ResultingQuestionnaireDocument, Is.Null);
        }

        [Test]
        public void When_CreateQuestionnaire_Then_new_history_item_should_be_added_with_specified_parameters()
        {
            // arrange
            Guid questionnaireId = Id.g1;
            Guid chapterId = Guid.Parse("A1111111111111111111111111111111");
            Guid responsibleId = Id.g2;
            string responsibleName = "owner";

            var dbContext = Create.InMemoryDbContext();
            dbContext.Users.Add(new DesignerIdentityUser { Id = responsibleId, UserName = responsibleName });
            dbContext.SaveChanges();

            var questionnaireStateTackerStorage = new InMemoryKeyValueStorage<QuestionnaireStateTracker>();

            var questionnaireDocument = new QuestionnaireDocument()
            {
                Children = new List<IComposite>()
                {
                    Create.Group(chapterId)
                }.ToReadOnlyCollection()
            };

            var questionnaire = Create.Questionnaire();
            questionnaire.Initialize(questionnaireId, questionnaireDocument, Enumerable.Empty<SharedPerson>());

            var command = new CreateQuestionnaire(questionnaireId, "title", responsibleId, true, null);

            var historyPostProcessor = Create.HistoryPostProcessor(dbContext, 
                Create.QuestionnireHistoryVersionsService(dbContext),
                questionnaireStateTrackerStorage: questionnaireStateTackerStorage);
            // act
            historyPostProcessor.Process(questionnaire, command);
            dbContext.SaveChanges();

            // assert
            var questionnaireHistoryItem = dbContext.QuestionnaireChangeRecords.First(historyItem =>
                    historyItem.QuestionnaireId == questionnaireId.FormatGuid());

            var stateTracker = questionnaireStateTackerStorage.GetById(questionnaireId.FormatGuid());

            Assert.That(questionnaireHistoryItem, Is.Not.Null);
            Assert.That(questionnaireHistoryItem.QuestionnaireId, Is.EqualTo(command.QuestionnaireId.FormatGuid()));
            Assert.That(questionnaireHistoryItem.ActionType, Is.EqualTo(QuestionnaireActionType.Add));
            Assert.That(questionnaireHistoryItem.UserId, Is.EqualTo(responsibleId));
            Assert.That(questionnaireHistoryItem.UserName, Is.EqualTo(responsibleName));
            Assert.That(questionnaireHistoryItem.Sequence, Is.EqualTo(0));
            Assert.That(questionnaireHistoryItem.TargetItemType, Is.EqualTo(QuestionnaireItemType.Questionnaire));
            Assert.That(questionnaireHistoryItem.TargetItemId, Is.EqualTo(questionnaireId));
            Assert.That(questionnaireHistoryItem.TargetItemTitle, Is.EqualTo(command.Title));

            Assert.That(stateTracker.GroupsState.Keys.Count, Is.EqualTo(2));
            Assert.That(questionnaireHistoryItem.ResultingQuestionnaireDocument, Is.Not.Null);
        }

        //[Test]
        //public void When_ImportQuestionnaire_Then_new_history_item_should_be_added_with_specified_parameters()
        //{
        //    Guid questionnaireId = Id.g1;
        //    Guid questionnaireOwner = Id.g3;
        //    string ownerName = "owner";
        //    string questionnnaireTitle = "name of questionnaire";
        //    DesignerDbContext dbContext;
        //    HistoryPostProcessor historyPostProcessor;
        //    QuestionnaireDocument questionnaireDocument;

        //    Given().ServiceLocator().
        //        And.QuestionnaireChangeRecordStorage(out dbContext).
        //        And.QuestionnireHistoryVersionsService(Create.QuestionnireHistoryVersionsService(dbContext)).
        //        And.AccountDocument(questionnaireOwner, ownerName).
        //        And.QuestionnaireStateTrackerStorage().
        //        And.QuestionnaireDocument(out questionnaireDocument, id: questionnaireId, title: questionnnaireTitle, userId: questionnaireOwner).
        //        And.HistoryPostProcessor(out historyPostProcessor);

        //    var questionnaire = Create.Questionnaire();
        //    questionnaire.Initialize(questionnaireId, questionnaireDocument, Enumerable.Empty<SharedPerson>());

        //    // when
        //    var command = Create.Command.ImportQuestionnaire(questionnaireDocument: questionnaireDocument);
        //    historyPostProcessor.Process(questionnaire, command);

        //    // then
        //    var questionnaireHistoryItem = dbContext.QuestionnaireChangeRecords
        //        .First(historyItem => historyItem.QuestionnaireId == questionnaireId.FormatGuid());

        //    questionnaireHistoryItem.Should().NotBeNull();
        //    questionnaireHistoryItem.QuestionnaireId.Should().Be(command.QuestionnaireId.FormatGuid());
        //    questionnaireHistoryItem.ActionType.Should().Be(QuestionnaireActionType.Import);
        //    questionnaireHistoryItem.UserId.Should().Be(questionnaireOwner);
        //    questionnaireHistoryItem.UserName.Should().Be(ownerName);
        //    questionnaireHistoryItem.Sequence.Should().Be(0);
        //    questionnaireHistoryItem.TargetItemType.Should().Be(QuestionnaireItemType.Questionnaire);
        //    questionnaireHistoryItem.TargetItemId.Should().Be(questionnaireId);
        //    questionnaireHistoryItem.TargetItemTitle.Should().Be(questionnnaireTitle);
        //    questionnaireHistoryItem.ResultingQuestionnaireDocument.Should().NotBeNull();
        //}

        [Test]
        public void When_DeleteQuestionnaire_Then_new_history_item_should_be_added_with_specified_parameters()
        {
            // arrange
            Guid questionnaireId = Id.g1;
            Guid responsibleId = Id.g2;
            Guid questionnaireOwnerId = Id.g3;
            string ownerName = "owner";
            string questionnaireTitle = "title of questionnaire";

            var dbContext =  Create.InMemoryDbContext();
            dbContext.Users.Add(new DesignerIdentityUser { Id = questionnaireOwnerId, UserName = ownerName });
            dbContext.SaveChanges();

            var questionnaireStateTackerStorage = new InMemoryKeyValueStorage<QuestionnaireStateTracker>();
            questionnaireStateTackerStorage.Store(
                new QuestionnaireStateTracker
                {
                    CreatedBy = questionnaireOwnerId,
                    GroupsState = new Dictionary<Guid, string> {
                    {
                        questionnaireId, questionnaireTitle
                    }}
                },
                questionnaireId.FormatGuid());

            var questionnaire = Create.Questionnaire();
            questionnaire.Initialize(questionnaireId, Create.QuestionnaireDocumentWithOneChapter(), Enumerable.Empty<SharedPerson>());

            var command = new DeleteQuestionnaire(questionnaireId, responsibleId);

            var historyPostProcessor = Create.HistoryPostProcessor(dbContext, 
                questionnaireHistoryVersionsService: Create.QuestionnireHistoryVersionsService(dbContext),
                questionnaireStateTrackerStorage: questionnaireStateTackerStorage);
            // act
            historyPostProcessor.Process(questionnaire, command);
            dbContext.SaveChanges();

            // assert
            var questionnaireHistoryItem = dbContext.QuestionnaireChangeRecords.First(historyItem =>
                    historyItem.QuestionnaireId == questionnaireId.FormatGuid());

            Assert.That(questionnaireHistoryItem, Is.Not.Null);
            Assert.That(questionnaireHistoryItem.QuestionnaireId, Is.EqualTo(command.QuestionnaireId.FormatGuid()));
            Assert.That(questionnaireHistoryItem.ActionType, Is.EqualTo(QuestionnaireActionType.Delete));
            Assert.That(questionnaireHistoryItem.UserId, Is.EqualTo(questionnaireOwnerId));
            Assert.That(questionnaireHistoryItem.UserName, Is.EqualTo(ownerName));
            Assert.That(questionnaireHistoryItem.Sequence, Is.EqualTo(0));
            Assert.That(questionnaireHistoryItem.TargetItemType, Is.EqualTo(QuestionnaireItemType.Questionnaire));
            Assert.That(questionnaireHistoryItem.TargetItemId, Is.EqualTo(questionnaireId));
            Assert.That(questionnaireHistoryItem.TargetItemTitle, Is.EqualTo(questionnaireTitle));
            Assert.That(questionnaireHistoryItem.Patch, Is.Null);
        }

        [Test]
        public void When_DeleteAttachment_Then_new_history_item_should_be_added_with_specified_parameters()
        {
            // arrange
            Guid questionnaireId = Id.g1;
            Guid responsibleId = Id.g2;
            Guid attachmentId = Id.g4;
            string ownerName = "owner";
            string attachmentName = "attachment";

            var dbContext = Create.InMemoryDbContext();
            dbContext.Add(new DesignerIdentityUser { Id = responsibleId, UserName = ownerName });

            var questionnaireStateTackerStorage = new InMemoryKeyValueStorage<QuestionnaireStateTracker>();
            questionnaireStateTackerStorage.Store(
                new QuestionnaireStateTracker
                {
                    CreatedBy = responsibleId,
                    AttachmentState = new Dictionary<Guid, string>() { { attachmentId, attachmentName } }
                },
                questionnaireId.FormatGuid());

            var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter();
            var questionnaire = Create.Questionnaire(responsibleId, questionnaireDocument);
            questionnaire.Initialize(questionnaireId, questionnaireDocument, Enumerable.Empty<SharedPerson>());

            var command = new DeleteAttachment(questionnaireId, attachmentId, responsibleId);

            var historyPostProcessor = Create.HistoryPostProcessor(dbContext,
                questionnaireHistoryVersionsService: Create.QuestionnireHistoryVersionsService(dbContext),
                questionnaireStateTrackerStorage: questionnaireStateTackerStorage);

            // act
            historyPostProcessor.Process(questionnaire, command);
            dbContext.SaveChanges();

            // assert
            var questionnaireHistoryItem = dbContext.QuestionnaireChangeRecords
                .First(historyItem => historyItem.QuestionnaireId == questionnaireId.FormatGuid());

            Assert.That(questionnaireHistoryItem, Is.Not.Null);
            Assert.That(questionnaireHistoryItem.QuestionnaireId, Is.EqualTo(command.QuestionnaireId.FormatGuid()));
            Assert.That(questionnaireHistoryItem.ActionType, Is.EqualTo(QuestionnaireActionType.Delete));
            Assert.That(questionnaireHistoryItem.UserId, Is.EqualTo(responsibleId));
            Assert.That(questionnaireHistoryItem.UserName, Is.EqualTo(ownerName));
            Assert.That(questionnaireHistoryItem.Sequence, Is.EqualTo(0));
            Assert.That(questionnaireHistoryItem.TargetItemType, Is.EqualTo(QuestionnaireItemType.Attachment));
            Assert.That(questionnaireHistoryItem.TargetItemId, Is.EqualTo(attachmentId));
            Assert.That(questionnaireHistoryItem.TargetItemTitle, Is.EqualTo(attachmentName));
            Assert.That(questionnaireHistoryItem.ResultingQuestionnaireDocument, Is.Not.Null);
        }

        [Test]
        public void When_UpdateQuestionnaire_Then_new_history_item_should_be_added_with_specified_parameters()
        {
            // arrange
            Guid questionnaireId = Id.g1;
            Guid responsibleId = Id.g2;
            string responsibleName = "owner";

            var dbContext = Create.InMemoryDbContext();

            dbContext.Users.Add(new DesignerIdentityUser { Id = responsibleId, UserName = responsibleName });

            var questionnaireStateTackerStorage = new InMemoryKeyValueStorage<QuestionnaireStateTracker>();
            questionnaireStateTackerStorage.Store(
                new QuestionnaireStateTracker
                {
                    CreatedBy = responsibleId,
                    GroupsState = new Dictionary<Guid, string> { { questionnaireId, "title" } },
                },
                questionnaireId.FormatGuid());

            var questionnaire = Create.Questionnaire();
            var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter();
            questionnaire.Initialize(questionnaireId, questionnaireDocument, Enumerable.Empty<SharedPerson>());

            var command = new UpdateQuestionnaire(questionnaireId, "title", "questionnaire", false, true, null, responsibleId, false);

            var historyPostProcessor = Create.HistoryPostProcessor(dbContext,
                questionnaireHistoryVersionsService: Create.QuestionnireHistoryVersionsService(dbContext),
                questionnaireStateTrackerStorage: questionnaireStateTackerStorage);
            // act
            historyPostProcessor.Process(questionnaire, command);
            dbContext.SaveChanges();

            // assert
            var questionnaireHistoryItem = dbContext.QuestionnaireChangeRecords.First(historyItem =>
                    historyItem.QuestionnaireId == questionnaireId.FormatGuid());

            Assert.That(questionnaireHistoryItem, Is.Not.Null);
            Assert.That(questionnaireHistoryItem.QuestionnaireId, Is.EqualTo(command.QuestionnaireId.FormatGuid()));
            Assert.That(questionnaireHistoryItem.ActionType, Is.EqualTo(QuestionnaireActionType.Update));
            Assert.That(questionnaireHistoryItem.UserId, Is.EqualTo(responsibleId));
            Assert.That(questionnaireHistoryItem.UserName, Is.EqualTo(responsibleName));
            Assert.That(questionnaireHistoryItem.Sequence, Is.EqualTo(0));
            Assert.That(questionnaireHistoryItem.TargetItemType, Is.EqualTo(QuestionnaireItemType.Questionnaire));
            Assert.That(questionnaireHistoryItem.TargetItemId, Is.EqualTo(questionnaireId));
            Assert.That(questionnaireHistoryItem.TargetItemTitle, Is.EqualTo(command.Title));
            Assert.That(questionnaireHistoryItem.ResultingQuestionnaireDocument, Is.Not.Null);
        }

        [Test]
        public void When_AddSharedPersonToQuestionnaire_Then_new_history_item_should_be_added_with_specified_parameters()
        {
            // arrange
            Guid questionnaireId = Id.g1;
            Guid responsibleId = Id.g2;
            Guid sharedWithId = Id.g3;
            string responsibleUserName = "responsible";
            string sharedWithUserName = "shared with";

            var dbContext = Create.InMemoryDbContext();
            dbContext.Users.Add(new DesignerIdentityUser { Id = responsibleId, UserName = responsibleUserName, Email = responsibleUserName + "email" });
            dbContext.Users.Add(new DesignerIdentityUser { Id = sharedWithId, UserName = sharedWithUserName, Email = sharedWithUserName + "email" });
            dbContext.SaveChanges();

            var questionnaire = Create.Questionnaire();
            var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter();
            questionnaire.Initialize(questionnaireId, questionnaireDocument, Enumerable.Empty<SharedPerson>());

            var command = new AddSharedPersonToQuestionnaire(questionnaireId, sharedWithId, "", ShareType.Edit,
                responsibleId);

            var historyPostProcessor = Create.HistoryPostProcessor(dbContext,
                questionnaireHistoryVersionsService: Create.QuestionnireHistoryVersionsService(dbContext));
            // act
            historyPostProcessor.Process(questionnaire, command);
            dbContext.SaveChanges();

            // assert
            var questionnaireHistoryItem = dbContext.QuestionnaireChangeRecords.First(historyItem =>
                    historyItem.QuestionnaireId == questionnaireId.FormatGuid());

            Assert.That(questionnaireHistoryItem, Is.Not.Null);
            Assert.That(questionnaireHistoryItem.QuestionnaireId, Is.EqualTo(command.QuestionnaireId.FormatGuid()));
            Assert.That(questionnaireHistoryItem.ActionType, Is.EqualTo(QuestionnaireActionType.Add));
            Assert.That(questionnaireHistoryItem.UserId, Is.EqualTo(responsibleId));
            Assert.That(questionnaireHistoryItem.UserName, Is.EqualTo(responsibleUserName));
            Assert.That(questionnaireHistoryItem.Sequence, Is.EqualTo(0));
            Assert.That(questionnaireHistoryItem.TargetItemType, Is.EqualTo(QuestionnaireItemType.Person));
            Assert.That(questionnaireHistoryItem.TargetItemId, Is.EqualTo(sharedWithId));
            Assert.That(questionnaireHistoryItem.TargetItemTitle, Is.EqualTo(sharedWithUserName + "email"));
            Assert.That(questionnaireHistoryItem.Patch, Is.Null);
        }

        [Test]
        public void When_RemoveSharedPersonFromQuestionnaire_Then_new_history_item_should_be_added_with_specified_parameters()
        {
            // arrange
            Guid questionnaireId = Id.g1;
            Guid responsibleId = Id.g2;
            Guid sharedWithId = Id.g3;
            string responsibleUserName = "responsible";
            string sharedWithUserName = "shared with";

            var dbContext = Create.InMemoryDbContext();

            dbContext.Users.Add(new DesignerIdentityUser { Id = responsibleId, UserName = responsibleUserName, Email = sharedWithUserName + "email" });
            dbContext.Users.Add(new DesignerIdentityUser { Id = sharedWithId, UserName = sharedWithUserName, Email = sharedWithUserName + "email" });
            dbContext.SaveChanges();

            var questionnaire = Create.Questionnaire();
            var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter();
            questionnaire.Initialize(questionnaireId, questionnaireDocument, Enumerable.Empty<SharedPerson>());

            var command = new RemoveSharedPersonFromQuestionnaire(questionnaireId, sharedWithId, "", responsibleId);

            var historyPostProcessor = Create.HistoryPostProcessor(dbContext, Create.QuestionnireHistoryVersionsService(dbContext));
            // act
            historyPostProcessor.Process(questionnaire, command);
            dbContext.SaveChanges();

            // assert
            var questionnaireHistoryItem = dbContext.QuestionnaireChangeRecords.First(historyItem =>
                    historyItem.QuestionnaireId == questionnaireId.FormatGuid());

            Assert.That(questionnaireHistoryItem, Is.Not.Null);
            Assert.That(questionnaireHistoryItem.QuestionnaireId, Is.EqualTo(command.QuestionnaireId.FormatGuid()));
            Assert.That(questionnaireHistoryItem.ActionType, Is.EqualTo(QuestionnaireActionType.Delete));
            Assert.That(questionnaireHistoryItem.UserId, Is.EqualTo(responsibleId));
            Assert.That(questionnaireHistoryItem.UserName, Is.EqualTo(responsibleUserName));
            Assert.That(questionnaireHistoryItem.Sequence, Is.EqualTo(0));
            Assert.That(questionnaireHistoryItem.TargetItemType, Is.EqualTo(QuestionnaireItemType.Person));
            Assert.That(questionnaireHistoryItem.TargetItemId, Is.EqualTo(sharedWithId));
            Assert.That(questionnaireHistoryItem.TargetItemTitle, Is.EqualTo(sharedWithUserName + "email"));
            Assert.That(questionnaireHistoryItem.Patch, Is.Null);
        }

        [Test]
        public void When_UpdateAttachment_Then_new_history_item_should_be_added_with_specified_parameters()
        {
            // arrange
            Guid questionnaireId = Id.g1;
            Guid responsibleId = Id.g2;
            Guid attachmentId = Id.g3;
            string responsibleUserName = "responsible";
            string attachmentName = "shared with";

            var dbContext = Create.InMemoryDbContext();
            dbContext.Users.Add(new DesignerIdentityUser { Id = responsibleId, UserName = responsibleUserName });
            dbContext.SaveChanges();

            var command = new AddOrUpdateAttachment(questionnaireId, attachmentId, responsibleId, attachmentName, "", null);

            var questionnaire = Create.Questionnaire();
            var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter();
            questionnaire.Initialize(questionnaireId, questionnaireDocument, Enumerable.Empty<SharedPerson>());

            var historyPostProcessor = Create.HistoryPostProcessor(dbContext,
                Create.QuestionnireHistoryVersionsService(dbContext),
                Create.MockedKeyValueStorage<QuestionnaireStateTracker>()
            );

            // act
            historyPostProcessor.Process(questionnaire, command);
            dbContext.SaveChanges();

            // assert
            var questionnaireHistoryItem = dbContext.QuestionnaireChangeRecords.First(historyItem =>
                    historyItem.QuestionnaireId == questionnaireId.FormatGuid());

            Assert.That(questionnaireHistoryItem, Is.Not.Null);
            Assert.That(questionnaireHistoryItem.QuestionnaireId, Is.EqualTo(command.QuestionnaireId.FormatGuid()));
            Assert.That(questionnaireHistoryItem.ActionType, Is.EqualTo(QuestionnaireActionType.Update));
            Assert.That(questionnaireHistoryItem.UserId, Is.EqualTo(responsibleId));
            Assert.That(questionnaireHistoryItem.UserName, Is.EqualTo(responsibleUserName));
            Assert.That(questionnaireHistoryItem.Sequence, Is.EqualTo(0));
            Assert.That(questionnaireHistoryItem.TargetItemType, Is.EqualTo(QuestionnaireItemType.Attachment));
            Assert.That(questionnaireHistoryItem.TargetItemId, Is.EqualTo(attachmentId));
            Assert.That(questionnaireHistoryItem.TargetItemTitle, Is.EqualTo(attachmentName));
            Assert.That(questionnaireHistoryItem.ResultingQuestionnaireDocument, Is.Not.Null);
        }

        [Test]
        public void When_UpdateStaticText_Then_new_history_item_should_be_added_with_specified_parameters()
        {
            // arrange
            Guid questionnaireId = Id.g1;
            Guid responsibleId = Id.g2;
            Guid staticTextId = Id.g3;
            string responsibleUserName = "responsible";
            string staticText = "static text";

            var dbContext =  Create.InMemoryDbContext();

            dbContext.Users.Add(new DesignerIdentityUser {Id = responsibleId, UserName = responsibleUserName});

            var questionnaire = Create.Questionnaire();
            var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter();
            questionnaire.Initialize(questionnaireId, questionnaireDocument, Enumerable.Empty<SharedPerson>());

            var command = Create.Command.UpdateStaticText(questionnaireId, staticTextId, staticText, "", responsibleId, "");

            var historyPostProcessor = Create.HistoryPostProcessor(dbContext, 
                Create.QuestionnireHistoryVersionsService(dbContext),
                Create.MockedKeyValueStorage<QuestionnaireStateTracker>()
                );
            // act
            historyPostProcessor.Process(questionnaire, command);
            dbContext.SaveChanges();

            // assert
            var questionnaireHistoryItem = dbContext.QuestionnaireChangeRecords.First(historyItem =>
                    historyItem.QuestionnaireId == questionnaireId.FormatGuid());

            Assert.That(questionnaireHistoryItem, Is.Not.Null);
            Assert.That(questionnaireHistoryItem.QuestionnaireId, Is.EqualTo(command.QuestionnaireId.FormatGuid()));
            Assert.That(questionnaireHistoryItem.ActionType, Is.EqualTo(QuestionnaireActionType.Update));
            Assert.That(questionnaireHistoryItem.UserId, Is.EqualTo(responsibleId));
            Assert.That(questionnaireHistoryItem.UserName, Is.EqualTo(responsibleUserName));
            Assert.That(questionnaireHistoryItem.Sequence, Is.EqualTo(0));
            Assert.That(questionnaireHistoryItem.TargetItemType, Is.EqualTo(QuestionnaireItemType.StaticText));
            Assert.That(questionnaireHistoryItem.TargetItemId, Is.EqualTo(staticTextId));
            Assert.That(questionnaireHistoryItem.TargetItemTitle, Is.EqualTo(staticText));
            Assert.That(questionnaireHistoryItem.ResultingQuestionnaireDocument, Is.Not.Null);
        }


        [Test]
        public void When_UpdateVariable_Then_new_history_item_should_be_added_with_specified_parameters()
        {
            // arrange
            Guid questionnaireId = Id.g1;
            Guid responsibleId = Id.g2;
            Guid variableId = Id.g3;
            string responsibleUserName = "responsible";
            string variableName = "variable";

            var dbContext = Create.InMemoryDbContext();
            dbContext.Users.Add(new DesignerIdentityUser { Id = responsibleId, UserName = responsibleUserName });

            var questionnaire = Create.Questionnaire();
            var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter();
            questionnaire.Initialize(questionnaireId, questionnaireDocument, Enumerable.Empty<SharedPerson>());

            var command = Create.Command.UpdateVariable(questionnaireId, variableId, VariableType.Boolean, variableName, "", null, responsibleId);

            var historyPostProcessor = Create.HistoryPostProcessor(dbContext,
                Create.QuestionnireHistoryVersionsService(dbContext),
                Create.MockedKeyValueStorage<QuestionnaireStateTracker>()
            );
            // act
            historyPostProcessor.Process(questionnaire, command);
            dbContext.SaveChanges();

            // assert
            var questionnaireHistoryItem = dbContext.QuestionnaireChangeRecords.First(historyItem =>
                    historyItem.QuestionnaireId == questionnaireId.FormatGuid());

            Assert.That(questionnaireHistoryItem, Is.Not.Null);
            Assert.That(questionnaireHistoryItem.QuestionnaireId, Is.EqualTo(command.QuestionnaireId.FormatGuid()));
            Assert.That(questionnaireHistoryItem.ActionType, Is.EqualTo(QuestionnaireActionType.Update));
            Assert.That(questionnaireHistoryItem.UserId, Is.EqualTo(responsibleId));
            Assert.That(questionnaireHistoryItem.UserName, Is.EqualTo(responsibleUserName));
            Assert.That(questionnaireHistoryItem.Sequence, Is.EqualTo(0));
            Assert.That(questionnaireHistoryItem.TargetItemType, Is.EqualTo(QuestionnaireItemType.Variable));
            Assert.That(questionnaireHistoryItem.TargetItemId, Is.EqualTo(variableId));
            Assert.That(questionnaireHistoryItem.TargetItemTitle, Is.EqualTo(variableName));
            Assert.That(questionnaireHistoryItem.ResultingQuestionnaireDocument, Is.Not.Null);
        }

        [Test]
        public void When_UpdateGroup_Then_new_history_item_should_be_added_with_specified_parameters()
        {
            // arrange
            Guid questionnaireId = Id.g1;
            Guid responsibleId = Id.g2;
            Guid groupId = Id.g3;
            string responsibleUserName = "responsible";
            string variable = "variable";

            var dbContext = Create.InMemoryDbContext();

            dbContext.Users.Add(new DesignerIdentityUser { Id = responsibleId, UserName = responsibleUserName });

            var questionnaireStateTackerStorage = new InMemoryKeyValueStorage<QuestionnaireStateTracker>();
            questionnaireStateTackerStorage.Store(
                new QuestionnaireStateTracker
                {
                    CreatedBy = responsibleId,
                    GroupsState = new Dictionary<Guid, string>() { { groupId, variable } }
                },
                questionnaireId.FormatGuid());

            var questionnaire = Create.Questionnaire();
            var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter();
            questionnaire.Initialize(questionnaireId, questionnaireDocument, Enumerable.Empty<SharedPerson>());

            var command = Create.Command.UpdateGroup(questionnaireId, groupId, responsibleId, variableName: variable);

            var historyPostProcessor = Create.HistoryPostProcessor(dbContext,
                Create.QuestionnireHistoryVersionsService(dbContext),
                questionnaireStateTackerStorage
            );
            // act
            historyPostProcessor.Process(questionnaire, command);
            dbContext.SaveChanges();

            // assert
            var questionnaireHistoryItem = dbContext.QuestionnaireChangeRecords.First(historyItem =>
                    historyItem.QuestionnaireId == questionnaireId.FormatGuid());

            Assert.That(questionnaireHistoryItem, Is.Not.Null);
            Assert.That(questionnaireHistoryItem.QuestionnaireId, Is.EqualTo(command.QuestionnaireId.FormatGuid()));
            Assert.That(questionnaireHistoryItem.ActionType, Is.EqualTo(QuestionnaireActionType.Update));
            Assert.That(questionnaireHistoryItem.UserId, Is.EqualTo(responsibleId));
            Assert.That(questionnaireHistoryItem.UserName, Is.EqualTo(responsibleUserName));
            Assert.That(questionnaireHistoryItem.Sequence, Is.EqualTo(0));
            Assert.That(questionnaireHistoryItem.TargetItemType, Is.EqualTo(QuestionnaireItemType.Section));
            Assert.That(questionnaireHistoryItem.TargetItemId, Is.EqualTo(groupId));
            Assert.That(questionnaireHistoryItem.TargetItemTitle, Is.EqualTo(variable));
            Assert.That(questionnaireHistoryItem.ResultingQuestionnaireDocument, Is.Not.Null);
        }

        [Test]
        public void When_UpdateGroup_and_group_became_a_roster_Then_new_history_item_should_be_added_with_specified_parameters()
        {
            // arrange
            Guid questionnaireId = Id.g1;
            Guid responsibleId = Id.g2;
            Guid rosterId = Id.g3;
            string responsibleUserName = "responsible";
            string variable = "variable";

            var dbContext = Create.InMemoryDbContext();

            dbContext.Users.Add(new DesignerIdentityUser { Id = responsibleId, UserName = responsibleUserName });

            var questionnaireStateTackerStorage = new InMemoryKeyValueStorage<QuestionnaireStateTracker>();
            questionnaireStateTackerStorage.Store(
                new QuestionnaireStateTracker
                {
                    CreatedBy = responsibleId,
                    GroupsState = new Dictionary<Guid, string>() { { rosterId, variable } }
                },
                questionnaireId.FormatGuid());

            var questionnaire = Create.Questionnaire();
            var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter();
            questionnaire.Initialize(questionnaireId, questionnaireDocument, Enumerable.Empty<SharedPerson>());

            var command = Create.Command.UpdateGroup(questionnaireId, rosterId, responsibleId, variableName: variable, isRoster: true);

            var historyPostProcessor = Create.HistoryPostProcessor(dbContext,
                Create.QuestionnireHistoryVersionsService(dbContext),
                Create.MockedKeyValueStorage<QuestionnaireStateTracker>()
            );
            // act
            historyPostProcessor.Process(questionnaire, command);
            dbContext.SaveChanges();

            // assert

            var allHistoryItems = dbContext.QuestionnaireChangeRecords.OrderBy(x => x.Sequence).ToList();

            var updateGroupHistoryItem = allHistoryItems[0];

            Assert.That(updateGroupHistoryItem, Is.Not.Null);
            Assert.That(updateGroupHistoryItem.QuestionnaireId, Is.EqualTo(command.QuestionnaireId.FormatGuid()));
            Assert.That(updateGroupHistoryItem.ActionType, Is.EqualTo(QuestionnaireActionType.Update));
            Assert.That(updateGroupHistoryItem.UserId, Is.EqualTo(responsibleId));
            Assert.That(updateGroupHistoryItem.UserName, Is.EqualTo(responsibleUserName));
            Assert.That(updateGroupHistoryItem.Sequence, Is.EqualTo(0));
            Assert.That(updateGroupHistoryItem.TargetItemType, Is.EqualTo(QuestionnaireItemType.Section));
            Assert.That(updateGroupHistoryItem.TargetItemId, Is.EqualTo(rosterId));
            Assert.That(updateGroupHistoryItem.TargetItemTitle, Is.EqualTo(variable));
            Assert.That(updateGroupHistoryItem.Patch, Is.Null);

            var groupBecameARosterHistoryItem = allHistoryItems[1];

            Assert.That(groupBecameARosterHistoryItem, Is.Not.Null);
            Assert.That(groupBecameARosterHistoryItem.QuestionnaireId, Is.EqualTo(command.QuestionnaireId.FormatGuid()));
            Assert.That(groupBecameARosterHistoryItem.ActionType, Is.EqualTo(QuestionnaireActionType.GroupBecameARoster));
            Assert.That(groupBecameARosterHistoryItem.UserId, Is.EqualTo(responsibleId));
            Assert.That(groupBecameARosterHistoryItem.UserName, Is.EqualTo(responsibleUserName));
            Assert.That(groupBecameARosterHistoryItem.Sequence, Is.EqualTo(1));
            Assert.That(groupBecameARosterHistoryItem.TargetItemType, Is.EqualTo(QuestionnaireItemType.Section));
            Assert.That(groupBecameARosterHistoryItem.TargetItemId, Is.EqualTo(rosterId));
            Assert.That(groupBecameARosterHistoryItem.TargetItemTitle, Is.EqualTo(variable));
            Assert.That(groupBecameARosterHistoryItem.Patch, Is.Null);

            var updateRosterHistoryItem = allHistoryItems[2];

            Assert.That(updateRosterHistoryItem, Is.Not.Null);
            Assert.That(updateRosterHistoryItem.QuestionnaireId, Is.EqualTo(command.QuestionnaireId.FormatGuid()));
            Assert.That(updateRosterHistoryItem.ActionType, Is.EqualTo(QuestionnaireActionType.Update));
            Assert.That(updateRosterHistoryItem.UserId, Is.EqualTo(responsibleId));
            Assert.That(updateRosterHistoryItem.UserName, Is.EqualTo(responsibleUserName));
            Assert.That(updateRosterHistoryItem.Sequence, Is.EqualTo(2));
            Assert.That(updateRosterHistoryItem.TargetItemType, Is.EqualTo(QuestionnaireItemType.Roster));
            Assert.That(updateRosterHistoryItem.TargetItemId, Is.EqualTo(rosterId));
            Assert.That(updateRosterHistoryItem.TargetItemTitle, Is.EqualTo(variable));
            Assert.That(updateRosterHistoryItem.ResultingQuestionnaireDocument, Is.Not.Null);
        }

        [Test]
        public void When_UpdateRoster_and_roster_became_a_group_Then_new_history_item_should_be_added_with_specified_parameters()
        {
            // arrange
            Guid questionnaireId = Id.g1;
            Guid responsibleId = Id.g2;
            Guid groupId = Id.g3;
            string responsibleUserName = "responsible";
            string variable = "variable";

            var dbContext = Create.InMemoryDbContext();

            dbContext.Users.Add(new DesignerIdentityUser { Id = responsibleId, UserName = responsibleUserName });

            var questionnaireStateTackerStorage = new InMemoryKeyValueStorage<QuestionnaireStateTracker>();
            questionnaireStateTackerStorage.Store(
                new QuestionnaireStateTracker
                {
                    CreatedBy = responsibleId,
                    RosterState = new Dictionary<Guid, string>() { { groupId, variable } }
                },
                questionnaireId.FormatGuid());

            var questionnaire = Create.Questionnaire();
            var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter();
            questionnaire.Initialize(questionnaireId, questionnaireDocument, Enumerable.Empty<SharedPerson>());

            var command = Create.Command.UpdateGroup(questionnaireId, groupId, responsibleId, variableName: variable);

            var historyPostProcessor = Create.HistoryPostProcessor(dbContext,
                Create.QuestionnireHistoryVersionsService(dbContext),
                questionnaireStateTackerStorage);
            // act
            historyPostProcessor.Process(questionnaire, command);
            dbContext.SaveChanges();

            // assert
            var updateRosterHistoryItem = dbContext.QuestionnaireChangeRecords.First(historyItem =>
                    historyItem.QuestionnaireId == questionnaireId.FormatGuid());

            Assert.That(updateRosterHistoryItem, Is.Not.Null);
            Assert.That(updateRosterHistoryItem.QuestionnaireId, Is.EqualTo(command.QuestionnaireId.FormatGuid()));
            Assert.That(updateRosterHistoryItem.ActionType, Is.EqualTo(QuestionnaireActionType.Update));
            Assert.That(updateRosterHistoryItem.UserId, Is.EqualTo(responsibleId));
            Assert.That(updateRosterHistoryItem.UserName, Is.EqualTo(responsibleUserName));
            Assert.That(updateRosterHistoryItem.Sequence, Is.EqualTo(0));
            Assert.That(updateRosterHistoryItem.TargetItemType, Is.EqualTo(QuestionnaireItemType.Roster));
            Assert.That(updateRosterHistoryItem.TargetItemId, Is.EqualTo(groupId));
            Assert.That(updateRosterHistoryItem.TargetItemTitle, Is.EqualTo(variable));
            Assert.That(updateRosterHistoryItem.Patch, Is.Null);

            var rosterBecameAGroupHistoryItem = dbContext.QuestionnaireChangeRecords.Last(historyItem =>
                    historyItem.QuestionnaireId == questionnaireId.FormatGuid());

            Assert.That(rosterBecameAGroupHistoryItem, Is.Not.Null);
            Assert.That(rosterBecameAGroupHistoryItem.QuestionnaireId, Is.EqualTo(command.QuestionnaireId.FormatGuid()));
            Assert.That(rosterBecameAGroupHistoryItem.ActionType, Is.EqualTo(QuestionnaireActionType.RosterBecameAGroup));
            Assert.That(rosterBecameAGroupHistoryItem.UserId, Is.EqualTo(responsibleId));
            Assert.That(rosterBecameAGroupHistoryItem.UserName, Is.EqualTo(responsibleUserName));
            Assert.That(rosterBecameAGroupHistoryItem.Sequence, Is.EqualTo(1));
            Assert.That(rosterBecameAGroupHistoryItem.TargetItemType, Is.EqualTo(QuestionnaireItemType.Roster));
            Assert.That(rosterBecameAGroupHistoryItem.TargetItemId, Is.EqualTo(groupId));
            Assert.That(rosterBecameAGroupHistoryItem.TargetItemTitle, Is.EqualTo(variable));
            Assert.That(rosterBecameAGroupHistoryItem.ResultingQuestionnaireDocument, Is.Not.Null);
        }

        [Test]
        public void When_move_roster_and_group_and_static_text_and_text_qeustion_Then_4_new_history_items_should_be_added()
        {
            // arrange
            Guid questionnaireId = Id.g1;
            Guid responsibleId = Id.g2;
            Guid groupId = Id.g3;
            Guid staticTextId = Id.g4;
            Guid variableId = Id.g5;
            Guid questionId = Id.g6;
            Guid targetGroupId = Id.g7;

            var dbContext = Create.InMemoryDbContext();

            var questionnaireStateTackerStorage = new InMemoryKeyValueStorage<QuestionnaireStateTracker>();
            questionnaireStateTackerStorage.Store(
                new QuestionnaireStateTracker
                {
                    CreatedBy = responsibleId,
                    GroupsState = new Dictionary<Guid, string>() { { groupId, "" }, { targetGroupId, "" } },
                    VariableState = new Dictionary<Guid, string>() { { variableId, "" } },
                    StaticTextState = new Dictionary<Guid, string>() { { staticTextId, "" } },
                    QuestionsState = new Dictionary<Guid, string>() { { questionId, "" } }
                },
                questionnaireId.FormatGuid());

            var questionnaire = Create.Questionnaire();
            questionnaire.Initialize(questionnaireId, Create.QuestionnaireDocumentWithOneChapter(), Enumerable.Empty<SharedPerson>());

            var moveGroupCommand = Create.Command.MoveGroup(questionnaireId, groupId, responsibleId, targetGroupId);
            var moveStaticTextCommand = Create.Command.MoveStaticText(questionnaireId, staticTextId, responsibleId, targetGroupId);
            var moveVariableCommand = Create.Command.MoveVariable(questionnaireId, variableId, responsibleId, targetGroupId);
            var moveQuestionCommand = Create.Command.MoveQuestion(questionnaireId, questionId, responsibleId, targetGroupId);

            var historyPostProcessor = Create.HistoryPostProcessor(dbContext,
                Create.QuestionnireHistoryVersionsService(dbContext),
                questionnaireStateTackerStorage);
            // act
            historyPostProcessor.Process(questionnaire, moveGroupCommand);
            historyPostProcessor.Process(questionnaire, moveStaticTextCommand);
            historyPostProcessor.Process(questionnaire, moveVariableCommand);
            historyPostProcessor.Process(questionnaire, moveQuestionCommand);
            dbContext.SaveChanges();

            // assert
            var newHistoryItems = dbContext.QuestionnaireChangeRecords.ToArray();

            Assert.That(newHistoryItems.Length, Is.EqualTo(4));
            Assert.That(newHistoryItems[0].TargetItemType, Is.EqualTo(QuestionnaireItemType.Section));
            Assert.That(newHistoryItems[1].TargetItemType, Is.EqualTo(QuestionnaireItemType.StaticText));
            Assert.That(newHistoryItems[2].TargetItemType, Is.EqualTo(QuestionnaireItemType.Variable));
            Assert.That(newHistoryItems[3].TargetItemType, Is.EqualTo(QuestionnaireItemType.Question));
        }

        [Test]
        public void When_pasting_after_group_Then_children_should_be_added_to_history()
        {
            // arrange
            Guid questionnaireId = Id.g1;
            Guid responsibleId = Id.g2;
            Guid groupId = Id.g3;
            Guid staticTextId = Id.g4;
            Guid variableId = Id.g5;
            Guid questionId = Id.g6;
            Guid targetGroupId = Id.g7;

            var dbContext = Create.InMemoryDbContext();
            
            var questionnaireStateTackerStorage = new InMemoryKeyValueStorage<QuestionnaireStateTracker>();
            questionnaireStateTackerStorage.Store(
                new QuestionnaireStateTracker
                {
                    CreatedBy = responsibleId,
                    GroupsState = new Dictionary<Guid, string>() { { groupId, "" } },
                    VariableState = new Dictionary<Guid, string>() { },
                    StaticTextState = new Dictionary<Guid, string>() { },
                    QuestionsState = new Dictionary<Guid, string>() { }
                },
                questionnaireId.FormatGuid());
            
            var questionnaireDocument = new QuestionnaireDocument()
            {
                Children = new List<IComposite>()
                {
                    Create.Group(groupId),
                    Create.Group(targetGroupId, children: new List<IComposite>()
                    {
                        Create.StaticText(staticTextId),
                        Create.Variable(variableId),
                        Create.TextQuestion(questionId: questionId)
                    })
                }.ToReadOnlyCollection()
            };

            var questionnaire = Create.Questionnaire();

            questionnaire.Initialize(questionnaireId, questionnaireDocument, Enumerable.Empty<SharedPerson>());
            var pastAfterCommand = Create.Command.PasteAfter(questionnaireId, targetGroupId, groupId, questionnaireId, groupId, responsibleId);
            var historyPostProcessor = Create.HistoryPostProcessor(dbContext,
                Create.QuestionnireHistoryVersionsService(dbContext),
                questionnaireStateTackerStorage);

            // act
            historyPostProcessor.Process(questionnaire, pastAfterCommand);

            // assert
            var newHistoryItems = dbContext.QuestionnaireChangeRecords.ToArray();
            var state = questionnaireStateTackerStorage.GetById(questionnaireId.FormatGuid());

            Assert.That(newHistoryItems.Length, Is.EqualTo(1));
            Assert.That(newHistoryItems[0].TargetItemType, Is.EqualTo(QuestionnaireItemType.Section));
            Assert.That(newHistoryItems[0].ResultingQuestionnaireDocument, Is.Not.Null);

            Assert.That(state.VariableState.ContainsKey(variableId));
            Assert.That(state.QuestionsState.ContainsKey(questionId));
            Assert.That(state.StaticTextState.ContainsKey(staticTextId));
            Assert.That(state.GroupsState.ContainsKey(targetGroupId));
        }

        [Test]
        public void When_deleting_questionnaire_after_rename()
        {
            // arrange
            Guid questionnaireId = Id.g1;
            Guid responsibleId = Id.g2;

            var dbContext = Create.InMemoryDbContext();

            var questionnaireStateTackerStorage = new InMemoryKeyValueStorage<QuestionnaireStateTracker>();

            questionnaireStateTackerStorage.Store(
                new QuestionnaireStateTracker
                {
                    CreatedBy = responsibleId,
                    GroupsState = new Dictionary<Guid, string>() { { questionnaireId, "first title" } },
                    VariableState = new Dictionary<Guid, string>() { },
                    StaticTextState = new Dictionary<Guid, string>() { },
                    QuestionsState = new Dictionary<Guid, string>() { }
                },
                questionnaireId.FormatGuid());

            var questionnaireDocument = Create.QuestionnaireDocument(questionnaireId);

            var questionnaire = Create.Questionnaire();

            questionnaire.Initialize(questionnaireId, questionnaireDocument, Enumerable.Empty<SharedPerson>());
            var updateTitleCommand = Create.Command.UpdateQuestionnaire(questionnaireId, responsibleId, "new title");

            var historyPostProcessor = Create.HistoryPostProcessor(dbContext,
                Create.QuestionnireHistoryVersionsService(dbContext),
                questionnaireStateTackerStorage);

            historyPostProcessor.Process(questionnaire, updateTitleCommand);

            var deleteCommand = Create.Command.DeleteQuestionnaire(questionnaireId, responsibleId);

            // act
            historyPostProcessor.Process(questionnaire, deleteCommand);
            dbContext.SaveChanges();

            // assert
            var newHistoryItems = dbContext.QuestionnaireChangeRecords.ToArray();
            var state = questionnaireStateTackerStorage.GetById(questionnaireId.FormatGuid());

            Assert.That(newHistoryItems.Length, Is.EqualTo(2));
            Assert.That(newHistoryItems[0].ActionType, Is.EqualTo(QuestionnaireActionType.Update));
            Assert.That(newHistoryItems[0].TargetItemType, Is.EqualTo(QuestionnaireItemType.Questionnaire));
            Assert.That(newHistoryItems[0].TargetItemTitle, Is.EqualTo("new title"));
            Assert.That(newHistoryItems[0].ResultingQuestionnaireDocument, Is.Not.Null);

            Assert.That(newHistoryItems[1].ActionType, Is.EqualTo(QuestionnaireActionType.Delete));
            Assert.That(newHistoryItems[1].TargetItemType, Is.EqualTo(QuestionnaireItemType.Questionnaire));
            Assert.That(newHistoryItems[1].TargetItemTitle, Is.EqualTo("new title"));
            Assert.That(newHistoryItems[1].Patch, Is.Null);
        }

        [Test]
        public void When_move_question_to_another_group_Then_parents_should_be_updated()
        {
            // arrange
            Guid questionnaireId = Id.g1;
            Guid responsibleId = Id.g2;
            Guid groupAId = Id.g3;
            Guid groupBId = Id.g4;
            Guid questionId = Id.g6;

            var dbContext = Create.InMemoryDbContext();

            var tracker = new QuestionnaireStateTracker
            {
                CreatedBy = responsibleId,
                GroupsState = new Dictionary<Guid, string>() { { groupAId, "" }, { groupBId, "" } },
                QuestionsState = new Dictionary<Guid, string>() { { questionId, "" } }
            };

            var questionnaireStateTackerStorage = new InMemoryKeyValueStorage<QuestionnaireStateTracker>();
            questionnaireStateTackerStorage.Store(
                tracker,
                questionnaireId.FormatGuid());
            
            var questionnaire = Create.Questionnaire();
            questionnaire.Initialize(questionnaireId, Create.QuestionnaireDocumentWithOneChapter(), Enumerable.Empty<SharedPerson>());

            var moveQuestionCommand = Create.Command.MoveQuestion(questionnaireId, questionId, responsibleId, groupBId);

            var historyPostProcessor = Create.HistoryPostProcessor(dbContext,
                Create.QuestionnireHistoryVersionsService(dbContext),
                questionnaireStateTackerStorage);

            // act
            historyPostProcessor.Process(questionnaire, moveQuestionCommand);
            dbContext.SaveChanges();

            // assert
            var newHistoryItems = dbContext.QuestionnaireChangeRecords.ToArray();
            var questionParent = tracker.Parents[questionId];

            Assert.That(newHistoryItems.Length, Is.EqualTo(1));
            Assert.That(questionParent, Is.EqualTo(groupBId));
        }

        [Test]
        public void When_amount_of_records_exceed_the_limit_Then_questionnaire_should_be_set_to_null_for_older_records()
        {
            // arrange
            Guid questionnaireId = Id.g1;
            Guid responsibleId = Id.g2;
            Guid groupAId = Id.g3;
            Guid groupBId = Id.g4;
            Guid questionId = Id.g6;

            var dbContext = Create.InMemoryDbContext();

            var oldJson = new NewtonJsonSerializer()
                .Serialize(Create.QuestionnaireDocumentWithOneChapter());

            var newJson = new NewtonJsonSerializer()
                .Serialize(Create.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
                {
                    Create.TextQuestion()
                }));


            dbContext.QuestionnaireChangeRecords.Add(
                Create.QuestionnaireChangeRecord(
                    questionnaireChangeRecordId: Id.gF.FormatGuid(),
                    questionnaireId: questionnaireId.FormatGuid(),
                    targetId: questionId,
                    targetType: QuestionnaireItemType.Question,
                    action: QuestionnaireActionType.ImportToHq,
                    resultingQuestionnaireDocument: null,
                    diffWithPreviousVersion: "patch",
                    sequence: 0,
                    reference: new[] { Create.QuestionnaireChangeReference() }));

            dbContext.QuestionnaireChangeRecords.Add(
                Create.QuestionnaireChangeRecord(
                    questionnaireChangeRecordId: Id.gA.FormatGuid(),
                    questionnaireId: questionnaireId.FormatGuid(),
                    targetId: questionId,
                    targetType: QuestionnaireItemType.Question,
                    action: QuestionnaireActionType.Clone,
                    resultingQuestionnaireDocument: oldJson,
                    sequence: 1,
                    reference: new[] { Create.QuestionnaireChangeReference() }));

            dbContext.QuestionnaireChangeRecords.Add(
                Create.QuestionnaireChangeRecord(
                    questionnaireChangeRecordId: Id.gB.FormatGuid(),
                    questionnaireId: questionnaireId.FormatGuid(),
                    targetId: questionId,
                    targetType: QuestionnaireItemType.Question,
                    action: QuestionnaireActionType.Clone,
                    resultingQuestionnaireDocument: newJson,
                    sequence: 2,
                    reference: new[] { Create.QuestionnaireChangeReference() }));

            dbContext.QuestionnaireChangeRecords.Add(
                Create.QuestionnaireChangeRecord(
                    questionnaireChangeRecordId: Id.gC.FormatGuid(),
                    questionnaireId: questionnaireId.FormatGuid(),
                    targetType: QuestionnaireItemType.Question,
                    action: QuestionnaireActionType.Update,
                    resultingQuestionnaireDocument: null,
                    diffWithPreviousVersion: "patch",
                    sequence: 3,
                    targetId: questionId));

            var tracker = new QuestionnaireStateTracker
            {
                CreatedBy = responsibleId,
                GroupsState = new Dictionary<Guid, string>() { { groupAId, "" }, { groupBId, "" } },
                QuestionsState = new Dictionary<Guid, string>() { { questionId, "" } }
            };

            var questionnaireStateTackerStorage = new InMemoryKeyValueStorage<QuestionnaireStateTracker>();
            questionnaireStateTackerStorage.Store(tracker, questionnaireId.FormatGuid());

            var questionnaire = Create.Questionnaire();
            questionnaire.Initialize(questionnaireId, Create.QuestionnaireDocumentWithOneChapter(),
                Enumerable.Empty<SharedPerson>());

            var moveQuestionCommand = Create.Command.MoveQuestion(questionnaireId, questionId, responsibleId, groupBId);

            var historyPostProcessor = Create.HistoryPostProcessor(dbContext,
                Create.QuestionnireHistoryVersionsService(dbContext,
                    questionnaireHistorySettings:
                    Mock.Of<IOptions<QuestionnaireHistorySettings>>(x =>
                        x.Value == new QuestionnaireHistorySettings { QuestionnaireChangeHistoryLimit = 2 })),
                questionnaireStateTackerStorage);

            dbContext.SaveChanges();

            // act
            historyPostProcessor.Process(questionnaire, moveQuestionCommand);
            dbContext.SaveChanges();

            // assert
            var newHistoryItems = dbContext.QuestionnaireChangeRecords.ToArray();

            Assert.That(newHistoryItems.Length, Is.EqualTo(5));
            Assert.That(newHistoryItems[0].ResultingQuestionnaireDocument, Is.Null);
            Assert.That(newHistoryItems[0].Patch, Is.Not.Null); // should preserve import to hq
            Assert.That(newHistoryItems[0].ActionType, Is.EqualTo(QuestionnaireActionType.ImportToHq)); // should preserve import to hq

            Assert.That(newHistoryItems[1].ResultingQuestionnaireDocument, Is.Null);
            Assert.That(newHistoryItems[1].Patch, Is.Null);

            Assert.That(newHistoryItems[2].ResultingQuestionnaireDocument, Is.Null);
            Assert.That(newHistoryItems[2].Patch, Is.Null);

            Assert.That(newHistoryItems[3].ResultingQuestionnaireDocument, Is.Null);
            Assert.That(newHistoryItems[3].Patch, Is.Not.Null);

            Assert.That(newHistoryItems[4].ResultingQuestionnaireDocument, Is.Not.Null);
            Assert.That(newHistoryItems[4].Patch, Is.Null);
        }

        [Test]
        public void when_setting_translation_as_default()
        {
            // arrange
            Guid questionnaireId = Id.g1;
            Guid responsibleId = Id.g2;
            string responsibleName = "owner";

            var dbContext = Create.InMemoryDbContext();

            dbContext.Users.Add(new DesignerIdentityUser { Id = responsibleId, UserName = responsibleName });

            var questionnaireStateTackerStorage = new InMemoryKeyValueStorage<QuestionnaireStateTracker>();
            questionnaireStateTackerStorage.Store(
                new QuestionnaireStateTracker
                {
                    CreatedBy = responsibleId,
                    GroupsState = new Dictionary<Guid, string>() { { questionnaireId, "title" } },
                },
                questionnaireId.FormatGuid());

            var questionnaire = Create.Questionnaire();
            var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter();
            questionnaireDocument.Translations.Add(new Core.SharedKernels.SurveySolutions.Documents.Translation
            {
                Id = Id.gF,
                Name = "Mova"
            });

            questionnaire.Initialize(questionnaireId, questionnaireDocument, Enumerable.Empty<SharedPerson>());

            var command = new SetDefaultTranslation(questionnaireId, responsibleId, Id.gF);

            var historyPostProcessor = Create.HistoryPostProcessor(dbContext,
                Create.QuestionnireHistoryVersionsService(dbContext),
                questionnaireStateTackerStorage);
            // act
            historyPostProcessor.Process(questionnaire, command);
            dbContext.SaveChanges();

            // assert
            var questionnaireHistoryItem = dbContext.QuestionnaireChangeRecords.First(historyItem =>
                    historyItem.QuestionnaireId == questionnaireId.FormatGuid());

            Assert.That(questionnaireHistoryItem, Is.Not.Null);
            Assert.That(questionnaireHistoryItem.QuestionnaireId, Is.EqualTo(command.QuestionnaireId.FormatGuid()));
            Assert.That(questionnaireHistoryItem.ActionType, Is.EqualTo(QuestionnaireActionType.Mark));
            Assert.That(questionnaireHistoryItem.UserId, Is.EqualTo(responsibleId));
            Assert.That(questionnaireHistoryItem.UserName, Is.EqualTo(responsibleName));
            Assert.That(questionnaireHistoryItem.Sequence, Is.EqualTo(0));
            Assert.That(questionnaireHistoryItem.TargetItemType, Is.EqualTo(QuestionnaireItemType.Translation));
            Assert.That(questionnaireHistoryItem.TargetItemId, Is.EqualTo(questionnaireId));
            Assert.That(questionnaireHistoryItem.TargetItemTitle, Is.EqualTo("Mova"));
            Assert.That(questionnaireHistoryItem.ResultingQuestionnaireDocument, Is.Not.Null);
        }

        [Test]
        public void when_writing_share_action_Should_not_clear_resulting_questionnaire_from_previous_record()
        {
            // arrange
            Guid questionnaireId = Id.g1;
            Guid responsibleId = Id.g2;
            Guid sharedWithId = Id.g3;
            string responsibleUserName = "responsible";
            string sharedWithUserName = "shared with";
            var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter();

            var dbContext = Create.InMemoryDbContext();
            dbContext.QuestionnaireChangeRecords.Add(
                Create.QuestionnaireChangeRecord(questionnaireChangeRecordId: Id.gA.FormatGuid(),
                resultingQuestionnaireDocument: JsonConvert.SerializeObject(questionnaireDocument),
                questionnaireId: questionnaireId.FormatGuid(),
                sequence: 1));

            dbContext.Users.Add(new DesignerIdentityUser { Id = responsibleId, UserName = responsibleUserName, Email = responsibleUserName + "email" });
            dbContext.Users.Add(new DesignerIdentityUser { Id = sharedWithId, UserName = sharedWithUserName, Email = sharedWithUserName + "email" });

            var questionnaire = Create.Questionnaire();
            questionnaire.Initialize(questionnaireId, questionnaireDocument, Enumerable.Empty<SharedPerson>());

            var command = new AddSharedPersonToQuestionnaire(questionnaireId, sharedWithId, "", ShareType.Edit, responsibleId);

            var historyPostProcessor = Create.HistoryPostProcessor(dbContext, Create.QuestionnireHistoryVersionsService(dbContext: dbContext));

            // act
            historyPostProcessor.Process(questionnaire, command);
            dbContext.SaveChanges();

            // assert
            var questionnaireHistoryItem = dbContext.QuestionnaireChangeRecords.First(historyItem =>
                    historyItem.QuestionnaireId == questionnaireId.FormatGuid());

            questionnaireHistoryItem.ResultingQuestionnaireDocument.Should().NotBeNullOrEmpty();
        }
        
        [Test]
        public void When_pasting_into_cover_section_Then_children_should_be_added_to_history()
        {
            // arrange
            Guid questionnaireId = Id.g1;
            Guid responsibleId = Id.g2;
            Guid coverId = Id.g3;
            Guid staticTextId = Id.g4;
            Guid questionId = Id.g6;
            Guid sourceGroupId = Id.g7;

            var dbContext = Create.InMemoryDbContext();
            
            var questionnaireStateTackerStorage = new InMemoryKeyValueStorage<QuestionnaireStateTracker>();
            questionnaireStateTackerStorage.Store(
                new QuestionnaireStateTracker
                {
                    CreatedBy = responsibleId,
                    GroupsState = new Dictionary<Guid, string>() { { coverId, "" } },
                    VariableState = new Dictionary<Guid, string>() { },
                    StaticTextState = new Dictionary<Guid, string>() { },
                    QuestionsState = new Dictionary<Guid, string>() { }
                },
                questionnaireId.FormatGuid());
            
            var questionnaireDocument = new QuestionnaireDocument()
            {
                Children = new List<IComposite>()
                {
                    Create.Group(coverId, children: new List<IComposite>()
                    {
                        Create.StaticText(staticTextId),
                        Create.TextQuestion(questionId: questionId),
                        Create.Variable()
                    }),
                    Create.Group(sourceGroupId),
                }.ToReadOnlyCollection()
            };
            questionnaireDocument.CoverPageSectionId = coverId;

            var questionnaire = Create.Questionnaire();
            questionnaire.Initialize(questionnaireId, questionnaireDocument, Enumerable.Empty<SharedPerson>());

            var pastIntoCommand = Create.Command.PasteInto(questionnaireId, coverId, questionnaireId, sourceGroupId, coverId, responsibleId);
            var historyPostProcessor = Create.HistoryPostProcessor(dbContext,
                Create.QuestionnireHistoryVersionsService(dbContext),
                questionnaireStateTackerStorage);

            // act
            historyPostProcessor.Process(questionnaire, pastIntoCommand);

            // assert
            var newHistoryItems = dbContext.QuestionnaireChangeRecords.ToArray();
            var state = questionnaireStateTackerStorage.GetById(questionnaireId.FormatGuid());

            Assert.That(newHistoryItems.Length, Is.EqualTo(1));
            Assert.That(newHistoryItems[0].TargetItemType, Is.EqualTo(QuestionnaireItemType.Section));
            Assert.That(newHistoryItems[0].ResultingQuestionnaireDocument, Is.Not.Null);

            Assert.That(state.QuestionsState.ContainsKey(questionId), Is.True);
            Assert.That(state.StaticTextState.ContainsKey(staticTextId), Is.True);
            Assert.That(state.GroupsState.ContainsKey(sourceGroupId), Is.False);
        }

        
        [Test]
        public void When_question_pasting_into_cover_section_Then_question_paste_should_be_added_to_history()
        {
            // arrange
            Guid questionnaireId = Id.g1;
            Guid responsibleId = Id.g2;
            Guid coverId = Id.g3;
            Guid questionId = Id.g6;
            Guid newQuestionId = Id.g5;
            Guid sourceGroupId = Id.g7;

            var dbContext = Create.InMemoryDbContext();
            
            var questionnaireStateTackerStorage = new InMemoryKeyValueStorage<QuestionnaireStateTracker>();
            questionnaireStateTackerStorage.Store(
                new QuestionnaireStateTracker
                {
                    CreatedBy = responsibleId,
                    GroupsState = new Dictionary<Guid, string>() { { coverId, "" } },
                    VariableState = new Dictionary<Guid, string>() { },
                    StaticTextState = new Dictionary<Guid, string>() { },
                    QuestionsState = new Dictionary<Guid, string>() { { questionId, ""} }
                },
                questionnaireId.FormatGuid());
            
            var questionnaireDocument = new QuestionnaireDocument()
            {
                Children = new List<IComposite>()
                {
                    Create.Group(coverId, children: new List<IComposite>()
                    {
                        Create.TextQuestion(questionId: questionId),
                        Create.TextQuestion(questionId: newQuestionId),
                        Create.Variable()
                    }),
                    Create.Group(sourceGroupId),
                }.ToReadOnlyCollection()
            };
            questionnaireDocument.CoverPageSectionId = coverId;

            var questionnaire = Create.Questionnaire();
            questionnaire.Initialize(questionnaireId, questionnaireDocument, Enumerable.Empty<SharedPerson>());

            var pastIntoCommand = Create.Command.PasteInto(questionnaireId, newQuestionId, questionnaireId, questionId, coverId, responsibleId);
            var historyPostProcessor = Create.HistoryPostProcessor(dbContext,
                Create.QuestionnireHistoryVersionsService(dbContext),
                questionnaireStateTackerStorage);

            // act
            historyPostProcessor.Process(questionnaire, pastIntoCommand);

            // assert
            var newHistoryItems = dbContext.QuestionnaireChangeRecords.ToArray();
            var state = questionnaireStateTackerStorage.GetById(questionnaireId.FormatGuid());

            Assert.That(newHistoryItems.Length, Is.EqualTo(1));
            Assert.That(newHistoryItems[0].TargetItemType, Is.EqualTo(QuestionnaireItemType.Question));
            Assert.That(newHistoryItems[0].ResultingQuestionnaireDocument, Is.Not.Null);

            Assert.That(state.QuestionsState.ContainsKey(questionId), Is.True);
            Assert.That(state.QuestionsState.ContainsKey(newQuestionId), Is.True);
            Assert.That(state.GroupsState.ContainsKey(sourceGroupId), Is.False);
        }

    }
}

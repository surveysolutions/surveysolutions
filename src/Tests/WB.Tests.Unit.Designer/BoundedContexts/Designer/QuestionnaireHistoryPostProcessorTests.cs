using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using FluentAssertions;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Attachments;
using WB.Core.BoundedContexts.Designer.Implementation.Services.QuestionnairePostProcessors;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.Implementation;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer
{
    [TestOf(typeof(ListViewPostProcessor))]
    [TestFixture]
    public class QuestionnaireHistoryPostProcessorTests
    {
        #region Gherkin

        private static GherkinGiven Given() => new GherkinGiven();

        private static void Then(params Action[] shoulds)
        {
            var exceptions = new List<Exception>();

            foreach (Action should in shoulds)
            {
                try
                {
                    should.Invoke();
                }
                catch (Exception exception)
                {
                    exceptions.Add(exception);
                }
            }

            if (exceptions.Count > 0)
            {
                throw new AggregateException(
                    "Expectation(s) failed:" + Environment.NewLine +
                    string.Join(Environment.NewLine, exceptions.Select(exception => exception.Message)));
            }
        }

        private class GherkinGiven
        {
            public class FluentSyntax
            {
                public FluentSyntax(GherkinGiven given)
                {
                    this.Given = given;
                }

                private GherkinGiven Given { get; }

                public GherkinGiven And => this.Given;
                public dynamic Context => this.Given.Context;
            }

            private dynamic Context { get; } = new ExpandoObject();
            private FluentSyntax Fluent => new FluentSyntax(this);

            private delegate FluentSyntax GivenOut<T>(out T output);

            private static FluentSyntax IgnoreOut<T>(GivenOut<T> given)
            {
                T _;
                return given(out _);
            }

            public FluentSyntax ServiceLocator()
            {
                AssemblyContext.SetupServiceLocator();

                return this.Fluent;
            }

            public FluentSyntax QuestionnaireChangeRecordStorage()
                => IgnoreOut<TestPlainStorage<QuestionnaireChangeRecord>>(this.QuestionnaireChangeRecordStorage);

            public FluentSyntax QuestionnaireChangeRecordStorage(
                out TestPlainStorage<QuestionnaireChangeRecord> questionnaireChangeRecordStorage)
            {
                questionnaireChangeRecordStorage = new TestPlainStorage<QuestionnaireChangeRecord>();
                this.Context.QuestionnaireChangeRecordStorage = questionnaireChangeRecordStorage;
                Setup.InstanceToMockedServiceLocator<IPlainStorageAccessor<QuestionnaireChangeRecord>>(questionnaireChangeRecordStorage);

                return this.Fluent;
            }

            public FluentSyntax AccountDocumentStorage()
            {
                var accountDocumentStorage = new TestInMemoryWriter<AccountDocument>();
                this.Context.AccountDocumentStorage = accountDocumentStorage;
                Setup.InstanceToMockedServiceLocator<IReadSideRepositoryWriter<AccountDocument>>(accountDocumentStorage);

                return this.Fluent;
            }

            public FluentSyntax AccountDocument(Guid userId, string userName)
            {
                TestInMemoryWriter<AccountDocument> accountDocumentStorage = this.Context.AccountDocumentStorage;

                var accountDocument = Create.AccountDocument(userId: userId, userName: userName);
                this.Context.AccountDocument = accountDocument;

                accountDocumentStorage.Store(accountDocument, userId.FormatGuid());

                return this.Fluent;
            }

            public FluentSyntax QuestionnaireStateTrackerStorage()
                => IgnoreOut<InMemoryKeyValueStorage<QuestionnaireStateTracker>>(this.QuestionnaireStateTrackerStorage);

            public FluentSyntax QuestionnaireStateTrackerStorage(
                out InMemoryKeyValueStorage<QuestionnaireStateTracker> questionnaireStateTrackerStorage)
            {
                questionnaireStateTrackerStorage = new InMemoryKeyValueStorage<QuestionnaireStateTracker>();
                this.Context.QuestionnaireStateTrackerStorage = questionnaireStateTrackerStorage;
                Setup.InstanceToMockedServiceLocator<IPlainKeyValueStorage<QuestionnaireStateTracker>>(questionnaireStateTrackerStorage);

                return this.Fluent;
            }

            public FluentSyntax HistoryPostProcessor(out HistoryPostProcessor historyPostProcessor)
            {
                historyPostProcessor = Create.HistoryPostProcessor();
                this.Context.HistoryPostProcessor = historyPostProcessor;

                return this.Fluent;
            }

            public FluentSyntax QuestionnaireDocument(Guid id, IComposite[] children)
            {
                QuestionnaireDocument _;
                return this.QuestionnaireDocument(out _, id: id, children: children);
            }

            public FluentSyntax QuestionnaireDocument(out QuestionnaireDocument questionnaireDocument,
                Guid? id = null, string title = null, Guid? userId = null, IComposite[] children = null)
            {
                questionnaireDocument = Create.QuestionnaireDocument(id: id, title: title, userId: userId, children: children);

                this.Context.QuestionnaireDocument = questionnaireDocument;

                return this.Fluent;
            }

            public FluentSyntax QuestionnaireDocumentIsImportedByHistoryPostProcessor()
            {
                HistoryPostProcessor historyPostProcessor = this.Context.HistoryPostProcessor;
                QuestionnaireDocument questionnaireDocument = this.Context.QuestionnaireDocument;

                historyPostProcessor.Process(null,
                    Create.Command.ImportQuestionnaire(questionnaireDocument: questionnaireDocument));

                return this.Fluent;
            }
        }

        #endregion

        [Test]
        public void When_CloneQuestionnaire_Then_new_history_item_should_be_added_with_spacified_parameters()
        {
            // arrange
            Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
            Guid responsibleId = Guid.Parse("22222222222222222222222222222222");
            Guid questionnaireOwner = Guid.Parse("33333333333333333333333333333333");
            string ownerName = "owner";

            AssemblyContext.SetupServiceLocator();
            var historyStorage = new TestPlainStorage<QuestionnaireChangeRecord>();
            Setup.InstanceToMockedServiceLocator<IPlainStorageAccessor<QuestionnaireChangeRecord>>(historyStorage);

            var usersStorage = new TestInMemoryWriter<AccountDocument>();
            usersStorage.Store(new AccountDocument { ProviderUserKey = questionnaireOwner, UserName = ownerName}, questionnaireOwner.FormatGuid());
            Setup.InstanceToMockedServiceLocator<IReadSideRepositoryWriter<AccountDocument>>(usersStorage);

            var questionnaireStateTackerStorage = new InMemoryKeyValueStorage<QuestionnaireStateTracker>();
            Setup.InstanceToMockedServiceLocator<IPlainKeyValueStorage<QuestionnaireStateTracker>>(questionnaireStateTackerStorage);

            var command = new CloneQuestionnaire(questionnaireId, "title", responsibleId, true,
                Create.QuestionnaireDocumentWithSharedPersons(questionnaireId, questionnaireOwner));
            
            var historyPostProcessor = CreateHistoryPostProcessor();

            // act
            historyPostProcessor.Process(null, command);

            // assert
            var questionnaireHistoryItem = historyStorage.Query(
                historyItems => historyItems.First(historyItem =>
                    historyItem.QuestionnaireId == questionnaireId.FormatGuid()));

            Assert.That(questionnaireHistoryItem, Is.Not.Null);
            Assert.That(questionnaireHistoryItem.QuestionnaireId, Is.EqualTo(command.QuestionnaireId.FormatGuid()));
            Assert.That(questionnaireHistoryItem.ActionType, Is.EqualTo(QuestionnaireActionType.Clone));
            Assert.That(questionnaireHistoryItem.UserId, Is.EqualTo(questionnaireOwner));
            Assert.That(questionnaireHistoryItem.UserName, Is.EqualTo(ownerName));
            Assert.That(questionnaireHistoryItem.Sequence, Is.EqualTo(0));
            Assert.That(questionnaireHistoryItem.TargetItemType, Is.EqualTo(QuestionnaireItemType.Questionnaire));
            Assert.That(questionnaireHistoryItem.TargetItemId, Is.EqualTo(questionnaireId));
            Assert.That(questionnaireHistoryItem.TargetItemTitle, Is.EqualTo(command.Title));
        }

        [Test]
        public void When_CreateQuestionnaire_Then_new_history_item_should_be_added_with_spacified_parameters()
        {
            // arrange
            Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
            Guid chapterId = Guid.Parse("A1111111111111111111111111111111");
            Guid responsibleId = Guid.Parse("22222222222222222222222222222222");
            string responsibleName = "owner";

            AssemblyContext.SetupServiceLocator();
            var historyStorage = new TestPlainStorage<QuestionnaireChangeRecord>();
            Setup.InstanceToMockedServiceLocator<IPlainStorageAccessor<QuestionnaireChangeRecord>>(historyStorage);

            var usersStorage = new TestInMemoryWriter<AccountDocument>();
            usersStorage.Store(new AccountDocument { ProviderUserKey = responsibleId, UserName = responsibleName }, responsibleId.FormatGuid());
            Setup.InstanceToMockedServiceLocator<IReadSideRepositoryWriter<AccountDocument>>(usersStorage);

            var questionnaireStateTackerStorage = new InMemoryKeyValueStorage<QuestionnaireStateTracker>();
            Setup.InstanceToMockedServiceLocator<IPlainKeyValueStorage<QuestionnaireStateTracker>>(questionnaireStateTackerStorage);

            var questionnaireDocument = new QuestionnaireDocument()
            {
                Children = new List<IComposite>()
                {
                    Create.Group(chapterId)
                }
            };

            var questionnaire = Create.Questionnaire();

            questionnaire.Initialize(questionnaireId, questionnaireDocument, Enumerable.Empty<SharedPerson>());

            var command = new CreateQuestionnaire(questionnaireId, "title", responsibleId, true);

            var historyPostProcessor = CreateHistoryPostProcessor();
            // act
            historyPostProcessor.Process(questionnaire, command);

            // assert
            var questionnaireHistoryItem = historyStorage.Query(
                historyItems => historyItems.First(historyItem =>
                    historyItem.QuestionnaireId == questionnaireId.FormatGuid()));

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

        }

        [Test]
        public void when_delete_group_it_should_remove_child_question()
        {
            Guid questionnaireId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
            Guid removedGroupId = Guid.Parse("11111111111111111111111111111111");
            Guid notRemovedQuestionId = Guid.Parse("33333333333333333333333333333333");
            HistoryPostProcessor historyPostProcessor;
            InMemoryKeyValueStorage<QuestionnaireStateTracker> questionnaireStateTrackerStorage;

            Given().ServiceLocator().
                And.QuestionnaireChangeRecordStorage().
                And.AccountDocumentStorage().
                And.QuestionnaireStateTrackerStorage(out questionnaireStateTrackerStorage).
                And.HistoryPostProcessor(out historyPostProcessor).
                And.QuestionnaireDocument(id: questionnaireId, children: new IComposite[]
                {
                    Create.Group(groupId: removedGroupId, children: new IComposite[]
                    {
                        Create.Question(),
                    }),
                    Create.Group(children: new IComposite[]
                    {
                        Create.Question(questionId: notRemovedQuestionId),
                    }),
                }).
                And.QuestionnaireDocumentIsImportedByHistoryPostProcessor();

            // when
            historyPostProcessor.Process(null, Create.Command.DeleteGroup(questionnaireId: questionnaireId, groupId: removedGroupId));

            // then
            var questionnaireStateTracker = questionnaireStateTrackerStorage.GetById(questionnaireId.FormatGuid());
            var questions = questionnaireStateTracker.QuestionsState.Keys;

            Then(() => questions.ShouldBeEquivalentTo(new[] {notRemovedQuestionId}));
        }

        [Test]
        public void When_ImportQuestionnaire_Then_new_history_item_should_be_added_with_spacified_parameters()
        {
            Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
            Guid questionnaireOwner = Guid.Parse("33333333333333333333333333333333");
            string ownerName = "owner";
            string questionnnaireTitle = "name of questionnaire";
            TestPlainStorage<QuestionnaireChangeRecord> historyStorage;
            HistoryPostProcessor historyPostProcessor;
            QuestionnaireDocument questionnaireDocument;

            Given().ServiceLocator().
                And.QuestionnaireChangeRecordStorage(out historyStorage).
                And.AccountDocumentStorage().
                And.AccountDocument(questionnaireOwner, ownerName).
                And.QuestionnaireStateTrackerStorage().
                And.QuestionnaireDocument(out questionnaireDocument, id: questionnaireId, title: questionnnaireTitle, userId: questionnaireOwner).
                And.HistoryPostProcessor(out historyPostProcessor);

            // when
            var command = Create.Command.ImportQuestionnaire(questionnaireDocument: questionnaireDocument);
            historyPostProcessor.Process(null, command);

            // then
            var questionnaireHistoryItem = historyStorage.Query(historyItems
                => historyItems.First(historyItem => historyItem.QuestionnaireId == questionnaireId.FormatGuid()));

            Then(() => questionnaireHistoryItem.ShouldNotBeNull(),
                () => questionnaireHistoryItem.QuestionnaireId.ShouldEqual(command.QuestionnaireId.FormatGuid()),
                () => questionnaireHistoryItem.ActionType.ShouldEqual(QuestionnaireActionType.Import),
                () => questionnaireHistoryItem.UserId.ShouldEqual(questionnaireOwner),
                () => questionnaireHistoryItem.UserName.ShouldEqual(ownerName),
                () => questionnaireHistoryItem.Sequence.ShouldEqual(0),
                () => questionnaireHistoryItem.TargetItemType.ShouldEqual(QuestionnaireItemType.Questionnaire),
                () => questionnaireHistoryItem.TargetItemId.ShouldEqual(questionnaireId),
                () => questionnaireHistoryItem.TargetItemTitle.ShouldEqual(questionnnaireTitle));
        }

        [Test]
        public void When_DeleteQuestionnaire_Then_new_history_item_should_be_added_with_spacified_parameters()
        {
            // arrange
            Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
            Guid responsibleId = Guid.Parse("22222222222222222222222222222222");
            Guid questionnaireOwnerId = Guid.Parse("33333333333333333333333333333333");
            string ownerName = "owner";
            string questionnaireTitle = "title of questionnaire";

            AssemblyContext.SetupServiceLocator();
            var historyStorage = new TestPlainStorage<QuestionnaireChangeRecord>();
            Setup.InstanceToMockedServiceLocator<IPlainStorageAccessor<QuestionnaireChangeRecord>>(historyStorage);

            var usersStorage = new TestInMemoryWriter<AccountDocument>();
            usersStorage.Store(new AccountDocument { ProviderUserKey = questionnaireOwnerId, UserName = ownerName }, questionnaireOwnerId.FormatGuid());
            Setup.InstanceToMockedServiceLocator<IReadSideRepositoryWriter<AccountDocument>>(usersStorage);

            var questionnaireStateTackerStorage = new InMemoryKeyValueStorage<QuestionnaireStateTracker>();
            questionnaireStateTackerStorage.Store(
                new QuestionnaireStateTracker
                {
                    CreatedBy = questionnaireOwnerId,
                    GroupsState = new Dictionary<Guid, string>() {{questionnaireId, questionnaireTitle}}
                },
                questionnaireId.FormatGuid());
            Setup.InstanceToMockedServiceLocator<IPlainKeyValueStorage<QuestionnaireStateTracker>>(questionnaireStateTackerStorage);

            var command = new DeleteQuestionnaire(questionnaireId, responsibleId);

            var historyPostProcessor = CreateHistoryPostProcessor();
            // act
            historyPostProcessor.Process(null, command);

            // assert
            var questionnaireHistoryItem = historyStorage.Query(
                historyItems => historyItems.First(historyItem =>
                    historyItem.QuestionnaireId == questionnaireId.FormatGuid()));

            Assert.That(questionnaireHistoryItem, Is.Not.Null);
            Assert.That(questionnaireHistoryItem.QuestionnaireId, Is.EqualTo(command.QuestionnaireId.FormatGuid()));
            Assert.That(questionnaireHistoryItem.ActionType, Is.EqualTo(QuestionnaireActionType.Delete));
            Assert.That(questionnaireHistoryItem.UserId, Is.EqualTo(questionnaireOwnerId));
            Assert.That(questionnaireHistoryItem.UserName, Is.EqualTo(ownerName));
            Assert.That(questionnaireHistoryItem.Sequence, Is.EqualTo(0));
            Assert.That(questionnaireHistoryItem.TargetItemType, Is.EqualTo(QuestionnaireItemType.Questionnaire));
            Assert.That(questionnaireHistoryItem.TargetItemId, Is.EqualTo(questionnaireId));
            Assert.That(questionnaireHistoryItem.TargetItemTitle, Is.EqualTo(questionnaireTitle));
        }

        [Test]
        public void When_DeleteAttachment_Then_new_history_item_should_be_added_with_spacified_parameters()
        {
            // arrange
            Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
            Guid responsibleId = Guid.Parse("22222222222222222222222222222222");
            Guid attachmentId = Guid.Parse("44444444444444444444444444444444");
            string ownerName = "owner";
            string attachmentName = "attachment";

            AssemblyContext.SetupServiceLocator();
            var historyStorage = new TestPlainStorage<QuestionnaireChangeRecord>();
            Setup.InstanceToMockedServiceLocator<IPlainStorageAccessor<QuestionnaireChangeRecord>>(historyStorage);

            var usersStorage = new TestInMemoryWriter<AccountDocument>();
            usersStorage.Store(new AccountDocument { ProviderUserKey = responsibleId, UserName = ownerName }, responsibleId.FormatGuid());
            Setup.InstanceToMockedServiceLocator<IReadSideRepositoryWriter<AccountDocument>>(usersStorage);

            var questionnaireStateTackerStorage = new InMemoryKeyValueStorage<QuestionnaireStateTracker>();
            questionnaireStateTackerStorage.Store(
                new QuestionnaireStateTracker
                {
                    CreatedBy = responsibleId,
                    AttachmentState = new Dictionary<Guid, string>() { { attachmentId, attachmentName } }
                },
                questionnaireId.FormatGuid());
            Setup.InstanceToMockedServiceLocator<IPlainKeyValueStorage<QuestionnaireStateTracker>>(questionnaireStateTackerStorage);

            var command = new DeleteAttachment(questionnaireId, attachmentId, responsibleId);

            var historyPostProcessor = CreateHistoryPostProcessor();
            // act
            historyPostProcessor.Process(null, command);

            // assert
            var questionnaireHistoryItem = historyStorage.Query(
                historyItems => historyItems.First(historyItem =>
                    historyItem.QuestionnaireId == questionnaireId.FormatGuid()));

            Assert.That(questionnaireHistoryItem, Is.Not.Null);
            Assert.That(questionnaireHistoryItem.QuestionnaireId, Is.EqualTo(command.QuestionnaireId.FormatGuid()));
            Assert.That(questionnaireHistoryItem.ActionType, Is.EqualTo(QuestionnaireActionType.Delete));
            Assert.That(questionnaireHistoryItem.UserId, Is.EqualTo(responsibleId));
            Assert.That(questionnaireHistoryItem.UserName, Is.EqualTo(ownerName));
            Assert.That(questionnaireHistoryItem.Sequence, Is.EqualTo(0));
            Assert.That(questionnaireHistoryItem.TargetItemType, Is.EqualTo(QuestionnaireItemType.Attachment));
            Assert.That(questionnaireHistoryItem.TargetItemId, Is.EqualTo(attachmentId));
            Assert.That(questionnaireHistoryItem.TargetItemTitle, Is.EqualTo(attachmentName));
        }

        [Test]
        public void When_UpdateQuestionnaire_Then_new_history_item_should_be_added_with_spacified_parameters()
        {
            // arrange
            Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
            Guid responsibleId = Guid.Parse("22222222222222222222222222222222");
            string responsibleName = "owner";

            AssemblyContext.SetupServiceLocator();
            var historyStorage = new TestPlainStorage<QuestionnaireChangeRecord>();
            Setup.InstanceToMockedServiceLocator<IPlainStorageAccessor<QuestionnaireChangeRecord>>(historyStorage);

            var usersStorage = new TestInMemoryWriter<AccountDocument>();
            usersStorage.Store(new AccountDocument { ProviderUserKey = responsibleId, UserName = responsibleName }, responsibleId.FormatGuid());
            Setup.InstanceToMockedServiceLocator<IReadSideRepositoryWriter<AccountDocument>>(usersStorage);
            
            var command = new UpdateQuestionnaire(questionnaireId, "title", true, responsibleId, false);

            var historyPostProcessor = CreateHistoryPostProcessor();
            // act
            historyPostProcessor.Process(null, command);

            // assert
            var questionnaireHistoryItem = historyStorage.Query(
                historyItems => historyItems.First(historyItem =>
                    historyItem.QuestionnaireId == questionnaireId.FormatGuid()));

            Assert.That(questionnaireHistoryItem, Is.Not.Null);
            Assert.That(questionnaireHistoryItem.QuestionnaireId, Is.EqualTo(command.QuestionnaireId.FormatGuid()));
            Assert.That(questionnaireHistoryItem.ActionType, Is.EqualTo(QuestionnaireActionType.Update));
            Assert.That(questionnaireHistoryItem.UserId, Is.EqualTo(responsibleId));
            Assert.That(questionnaireHistoryItem.UserName, Is.EqualTo(responsibleName));
            Assert.That(questionnaireHistoryItem.Sequence, Is.EqualTo(0));
            Assert.That(questionnaireHistoryItem.TargetItemType, Is.EqualTo(QuestionnaireItemType.Questionnaire));
            Assert.That(questionnaireHistoryItem.TargetItemId, Is.EqualTo(questionnaireId));
            Assert.That(questionnaireHistoryItem.TargetItemTitle, Is.EqualTo(command.Title));
        }

        [Test]
        public void When_AddSharedPersonToQuestionnaire_Then_new_history_item_should_be_added_with_spacified_parameters()
        {
            // arrange
            Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
            Guid responsibleId = Guid.Parse("22222222222222222222222222222222");
            Guid sharedWithId = Guid.Parse("33333333333333333333333333333333");
            string responsibleUserName = "responsible";
            string sharedWithUserName = "shared with";

            AssemblyContext.SetupServiceLocator();
            var historyStorage = new TestPlainStorage<QuestionnaireChangeRecord>();
            Setup.InstanceToMockedServiceLocator<IPlainStorageAccessor<QuestionnaireChangeRecord>>(historyStorage);

            var usersStorage = new TestInMemoryWriter<AccountDocument>();
            usersStorage.Store(new AccountDocument { ProviderUserKey = responsibleId, UserName = responsibleUserName }, responsibleId.FormatGuid());
            usersStorage.Store(new AccountDocument { ProviderUserKey = sharedWithId, UserName = sharedWithUserName }, sharedWithId.FormatGuid());
            Setup.InstanceToMockedServiceLocator<IReadSideRepositoryWriter<AccountDocument>>(usersStorage);

            var command = new AddSharedPersonToQuestionnaire(questionnaireId, sharedWithId, "", ShareType.Edit,
                responsibleId);

            var historyPostProcessor = CreateHistoryPostProcessor();
            // act
            historyPostProcessor.Process(null, command);

            // assert
            var questionnaireHistoryItem = historyStorage.Query(
                historyItems => historyItems.First(historyItem =>
                    historyItem.QuestionnaireId == questionnaireId.FormatGuid()));

            Assert.That(questionnaireHistoryItem, Is.Not.Null);
            Assert.That(questionnaireHistoryItem.QuestionnaireId, Is.EqualTo(command.QuestionnaireId.FormatGuid()));
            Assert.That(questionnaireHistoryItem.ActionType, Is.EqualTo(QuestionnaireActionType.Add));
            Assert.That(questionnaireHistoryItem.UserId, Is.EqualTo(responsibleId));
            Assert.That(questionnaireHistoryItem.UserName, Is.EqualTo(responsibleUserName));
            Assert.That(questionnaireHistoryItem.Sequence, Is.EqualTo(0));
            Assert.That(questionnaireHistoryItem.TargetItemType, Is.EqualTo(QuestionnaireItemType.Person));
            Assert.That(questionnaireHistoryItem.TargetItemId, Is.EqualTo(sharedWithId));
            Assert.That(questionnaireHistoryItem.TargetItemTitle, Is.EqualTo(sharedWithUserName));
        }

        [Test]
        public void When_RemoveSharedPersonFromQuestionnaire_Then_new_history_item_should_be_added_with_spacified_parameters()
        {
            // arrange
            Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
            Guid responsibleId = Guid.Parse("22222222222222222222222222222222");
            Guid sharedWithId = Guid.Parse("33333333333333333333333333333333");
            string responsibleUserName = "responsible";
            string sharedWithUserName = "shared with";

            AssemblyContext.SetupServiceLocator();
            var historyStorage = new TestPlainStorage<QuestionnaireChangeRecord>();
            Setup.InstanceToMockedServiceLocator<IPlainStorageAccessor<QuestionnaireChangeRecord>>(historyStorage);

            var usersStorage = new TestInMemoryWriter<AccountDocument>();
            usersStorage.Store(new AccountDocument { ProviderUserKey = responsibleId, UserName = responsibleUserName }, responsibleId.FormatGuid());
            usersStorage.Store(new AccountDocument { ProviderUserKey = sharedWithId, UserName = sharedWithUserName }, sharedWithId.FormatGuid());
            Setup.InstanceToMockedServiceLocator<IReadSideRepositoryWriter<AccountDocument>>(usersStorage);

            var command = new RemoveSharedPersonFromQuestionnaire(questionnaireId, sharedWithId, "", responsibleId);

            var historyPostProcessor = CreateHistoryPostProcessor();
            // act
            historyPostProcessor.Process(null, command);

            // assert
            var questionnaireHistoryItem = historyStorage.Query(
                historyItems => historyItems.First(historyItem =>
                    historyItem.QuestionnaireId == questionnaireId.FormatGuid()));

            Assert.That(questionnaireHistoryItem, Is.Not.Null);
            Assert.That(questionnaireHistoryItem.QuestionnaireId, Is.EqualTo(command.QuestionnaireId.FormatGuid()));
            Assert.That(questionnaireHistoryItem.ActionType, Is.EqualTo(QuestionnaireActionType.Delete));
            Assert.That(questionnaireHistoryItem.UserId, Is.EqualTo(responsibleId));
            Assert.That(questionnaireHistoryItem.UserName, Is.EqualTo(responsibleUserName));
            Assert.That(questionnaireHistoryItem.Sequence, Is.EqualTo(0));
            Assert.That(questionnaireHistoryItem.TargetItemType, Is.EqualTo(QuestionnaireItemType.Person));
            Assert.That(questionnaireHistoryItem.TargetItemId, Is.EqualTo(sharedWithId));
            Assert.That(questionnaireHistoryItem.TargetItemTitle, Is.EqualTo(sharedWithUserName));
        }

        [Test]
        public void When_UpdateAttachment_Then_new_history_item_should_be_added_with_spacified_parameters()
        {
            // arrange
            Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
            Guid responsibleId = Guid.Parse("22222222222222222222222222222222");
            Guid attachmentId = Guid.Parse("33333333333333333333333333333333");
            string responsibleUserName = "responsible";
            string attachmentName = "shared with";

            AssemblyContext.SetupServiceLocator();
            var historyStorage = new TestPlainStorage<QuestionnaireChangeRecord>();
            Setup.InstanceToMockedServiceLocator<IPlainStorageAccessor<QuestionnaireChangeRecord>>(historyStorage);

            var usersStorage = new TestInMemoryWriter<AccountDocument>();
            usersStorage.Store(new AccountDocument { ProviderUserKey = responsibleId, UserName = responsibleUserName }, responsibleId.FormatGuid());
            Setup.InstanceToMockedServiceLocator<IReadSideRepositoryWriter<AccountDocument>>(usersStorage);

            var command = new AddOrUpdateAttachment(questionnaireId, attachmentId, responsibleId, attachmentName, "");

            var historyPostProcessor = CreateHistoryPostProcessor();
            // act
            historyPostProcessor.Process(null, command);

            // assert
            var questionnaireHistoryItem = historyStorage.Query(
                historyItems => historyItems.First(historyItem =>
                    historyItem.QuestionnaireId == questionnaireId.FormatGuid()));

            Assert.That(questionnaireHistoryItem, Is.Not.Null);
            Assert.That(questionnaireHistoryItem.QuestionnaireId, Is.EqualTo(command.QuestionnaireId.FormatGuid()));
            Assert.That(questionnaireHistoryItem.ActionType, Is.EqualTo(QuestionnaireActionType.Update));
            Assert.That(questionnaireHistoryItem.UserId, Is.EqualTo(responsibleId));
            Assert.That(questionnaireHistoryItem.UserName, Is.EqualTo(responsibleUserName));
            Assert.That(questionnaireHistoryItem.Sequence, Is.EqualTo(0));
            Assert.That(questionnaireHistoryItem.TargetItemType, Is.EqualTo(QuestionnaireItemType.Attachment));
            Assert.That(questionnaireHistoryItem.TargetItemId, Is.EqualTo(attachmentId));
            Assert.That(questionnaireHistoryItem.TargetItemTitle, Is.EqualTo(attachmentName));
        }

        [Test]
        public void When_UpdateStaticText_Then_new_history_item_should_be_added_with_spacified_parameters()
        {
            // arrange
            Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
            Guid responsibleId = Guid.Parse("22222222222222222222222222222222");
            Guid staticTextId = Guid.Parse("33333333333333333333333333333333");
            string responsibleUserName = "responsible";
            string staticText = "static text";

            AssemblyContext.SetupServiceLocator();
            var historyStorage = new TestPlainStorage<QuestionnaireChangeRecord>();
            Setup.InstanceToMockedServiceLocator<IPlainStorageAccessor<QuestionnaireChangeRecord>>(historyStorage);

            var usersStorage = new TestInMemoryWriter<AccountDocument>();
            usersStorage.Store(new AccountDocument { ProviderUserKey = responsibleId, UserName = responsibleUserName }, responsibleId.FormatGuid());
            Setup.InstanceToMockedServiceLocator<IReadSideRepositoryWriter<AccountDocument>>(usersStorage);

            var command = Create.Command.UpdateStaticText(questionnaireId, staticTextId, staticText, "", responsibleId, "");

            var historyPostProcessor = CreateHistoryPostProcessor();
            // act
            historyPostProcessor.Process(null, command);

            // assert
            var questionnaireHistoryItem = historyStorage.Query(
                historyItems => historyItems.First(historyItem =>
                    historyItem.QuestionnaireId == questionnaireId.FormatGuid()));

            Assert.That(questionnaireHistoryItem, Is.Not.Null);
            Assert.That(questionnaireHistoryItem.QuestionnaireId, Is.EqualTo(command.QuestionnaireId.FormatGuid()));
            Assert.That(questionnaireHistoryItem.ActionType, Is.EqualTo(QuestionnaireActionType.Update));
            Assert.That(questionnaireHistoryItem.UserId, Is.EqualTo(responsibleId));
            Assert.That(questionnaireHistoryItem.UserName, Is.EqualTo(responsibleUserName));
            Assert.That(questionnaireHistoryItem.Sequence, Is.EqualTo(0));
            Assert.That(questionnaireHistoryItem.TargetItemType, Is.EqualTo(QuestionnaireItemType.StaticText));
            Assert.That(questionnaireHistoryItem.TargetItemId, Is.EqualTo(staticTextId));
            Assert.That(questionnaireHistoryItem.TargetItemTitle, Is.EqualTo(staticText));
        }

        [Test]
        public void When_UpdateVariable_Then_new_history_item_should_be_added_with_spacified_parameters()
        {
            // arrange
            Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
            Guid responsibleId = Guid.Parse("22222222222222222222222222222222");
            Guid variableId = Guid.Parse("33333333333333333333333333333333");
            string responsibleUserName = "responsible";
            string variableName = "variable";

            AssemblyContext.SetupServiceLocator();
            var historyStorage = new TestPlainStorage<QuestionnaireChangeRecord>();
            Setup.InstanceToMockedServiceLocator<IPlainStorageAccessor<QuestionnaireChangeRecord>>(historyStorage);

            var usersStorage = new TestInMemoryWriter<AccountDocument>();
            usersStorage.Store(new AccountDocument { ProviderUserKey = responsibleId, UserName = responsibleUserName }, responsibleId.FormatGuid());
            Setup.InstanceToMockedServiceLocator<IReadSideRepositoryWriter<AccountDocument>>(usersStorage);

            var command = Create.Command.UpdateVariable(questionnaireId, variableId, VariableType.Boolean, variableName, "", responsibleId);

            var historyPostProcessor = CreateHistoryPostProcessor();
            // act
            historyPostProcessor.Process(null, command);

            // assert
            var questionnaireHistoryItem = historyStorage.Query(
                historyItems => historyItems.First(historyItem =>
                    historyItem.QuestionnaireId == questionnaireId.FormatGuid()));

            Assert.That(questionnaireHistoryItem, Is.Not.Null);
            Assert.That(questionnaireHistoryItem.QuestionnaireId, Is.EqualTo(command.QuestionnaireId.FormatGuid()));
            Assert.That(questionnaireHistoryItem.ActionType, Is.EqualTo(QuestionnaireActionType.Update));
            Assert.That(questionnaireHistoryItem.UserId, Is.EqualTo(responsibleId));
            Assert.That(questionnaireHistoryItem.UserName, Is.EqualTo(responsibleUserName));
            Assert.That(questionnaireHistoryItem.Sequence, Is.EqualTo(0));
            Assert.That(questionnaireHistoryItem.TargetItemType, Is.EqualTo(QuestionnaireItemType.Variable));
            Assert.That(questionnaireHistoryItem.TargetItemId, Is.EqualTo(variableId));
            Assert.That(questionnaireHistoryItem.TargetItemTitle, Is.EqualTo(variableName));
        }

        [Test]
        public void When_UpdateGroup_Then_new_history_item_should_be_added_with_spacified_parameters()
        {
            // arrange
            Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
            Guid responsibleId = Guid.Parse("22222222222222222222222222222222");
            Guid groupId = Guid.Parse("33333333333333333333333333333333");
            string responsibleUserName = "responsible";
            string variable = "variable";

            AssemblyContext.SetupServiceLocator();
            var historyStorage = new TestPlainStorage<QuestionnaireChangeRecord>();
            Setup.InstanceToMockedServiceLocator<IPlainStorageAccessor<QuestionnaireChangeRecord>>(historyStorage);

            var usersStorage = new TestInMemoryWriter<AccountDocument>();
            usersStorage.Store(new AccountDocument { ProviderUserKey = responsibleId, UserName = responsibleUserName }, responsibleId.FormatGuid());
            Setup.InstanceToMockedServiceLocator<IReadSideRepositoryWriter<AccountDocument>>(usersStorage);

            var questionnaireStateTackerStorage = new InMemoryKeyValueStorage<QuestionnaireStateTracker>();
            questionnaireStateTackerStorage.Store(
                new QuestionnaireStateTracker
                {
                    CreatedBy = responsibleId,
                    GroupsState = new Dictionary<Guid, string>() { { groupId, variable } }
                },
                questionnaireId.FormatGuid());
            Setup.InstanceToMockedServiceLocator<IPlainKeyValueStorage<QuestionnaireStateTracker>>(questionnaireStateTackerStorage);

            var command = Create.Command.UpdateGroup(questionnaireId, groupId, responsibleId, variableName: variable);

            var historyPostProcessor = CreateHistoryPostProcessor();
            // act
            historyPostProcessor.Process(null, command);

            // assert
            var questionnaireHistoryItem = historyStorage.Query(
                historyItems => historyItems.First(historyItem =>
                    historyItem.QuestionnaireId == questionnaireId.FormatGuid()));

            Assert.That(questionnaireHistoryItem, Is.Not.Null);
            Assert.That(questionnaireHistoryItem.QuestionnaireId, Is.EqualTo(command.QuestionnaireId.FormatGuid()));
            Assert.That(questionnaireHistoryItem.ActionType, Is.EqualTo(QuestionnaireActionType.Update));
            Assert.That(questionnaireHistoryItem.UserId, Is.EqualTo(responsibleId));
            Assert.That(questionnaireHistoryItem.UserName, Is.EqualTo(responsibleUserName));
            Assert.That(questionnaireHistoryItem.Sequence, Is.EqualTo(0));
            Assert.That(questionnaireHistoryItem.TargetItemType, Is.EqualTo(QuestionnaireItemType.Group));
            Assert.That(questionnaireHistoryItem.TargetItemId, Is.EqualTo(groupId));
            Assert.That(questionnaireHistoryItem.TargetItemTitle, Is.EqualTo(variable));
        }

        [Test]
        public void When_UpdateGroup_and_group_became_a_roster_Then_new_history_item_should_be_added_with_spacified_parameters()
        {
            // arrange
            Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
            Guid responsibleId = Guid.Parse("22222222222222222222222222222222");
            Guid rosterId = Guid.Parse("33333333333333333333333333333333");
            string responsibleUserName = "responsible";
            string variable = "variable";

            AssemblyContext.SetupServiceLocator();
            var historyStorage = new TestPlainStorage<QuestionnaireChangeRecord>();
            Setup.InstanceToMockedServiceLocator<IPlainStorageAccessor<QuestionnaireChangeRecord>>(historyStorage);

            var usersStorage = new TestInMemoryWriter<AccountDocument>();
            usersStorage.Store(new AccountDocument { ProviderUserKey = responsibleId, UserName = responsibleUserName }, responsibleId.FormatGuid());
            Setup.InstanceToMockedServiceLocator<IReadSideRepositoryWriter<AccountDocument>>(usersStorage);

            var questionnaireStateTackerStorage = new InMemoryKeyValueStorage<QuestionnaireStateTracker>();
            questionnaireStateTackerStorage.Store(
                new QuestionnaireStateTracker
                {
                    CreatedBy = responsibleId,
                    GroupsState = new Dictionary<Guid, string>() { { rosterId, variable } }
                },
                questionnaireId.FormatGuid());
            Setup.InstanceToMockedServiceLocator<IPlainKeyValueStorage<QuestionnaireStateTracker>>(questionnaireStateTackerStorage);

            var command = Create.Command.UpdateGroup(questionnaireId, rosterId, responsibleId, variableName: variable, isRoster: true);

            var historyPostProcessor = CreateHistoryPostProcessor();
            // act
            historyPostProcessor.Process(null, command);

            // assert

            var allHistoryItems = historyStorage.Query(historyItems => historyItems.ToList());

            var updateGroupHistoryItem = allHistoryItems[0];

            Assert.That(updateGroupHistoryItem, Is.Not.Null);
            Assert.That(updateGroupHistoryItem.QuestionnaireId, Is.EqualTo(command.QuestionnaireId.FormatGuid()));
            Assert.That(updateGroupHistoryItem.ActionType, Is.EqualTo(QuestionnaireActionType.Update));
            Assert.That(updateGroupHistoryItem.UserId, Is.EqualTo(responsibleId));
            Assert.That(updateGroupHistoryItem.UserName, Is.EqualTo(responsibleUserName));
            Assert.That(updateGroupHistoryItem.Sequence, Is.EqualTo(0));
            Assert.That(updateGroupHistoryItem.TargetItemType, Is.EqualTo(QuestionnaireItemType.Group));
            Assert.That(updateGroupHistoryItem.TargetItemId, Is.EqualTo(rosterId));
            Assert.That(updateGroupHistoryItem.TargetItemTitle, Is.EqualTo(variable));


            var groupBecameARosterHistoryItem = allHistoryItems[1];

            Assert.That(groupBecameARosterHistoryItem, Is.Not.Null);
            Assert.That(groupBecameARosterHistoryItem.QuestionnaireId, Is.EqualTo(command.QuestionnaireId.FormatGuid()));
            Assert.That(groupBecameARosterHistoryItem.ActionType, Is.EqualTo(QuestionnaireActionType.GroupBecameARoster));
            Assert.That(groupBecameARosterHistoryItem.UserId, Is.EqualTo(responsibleId));
            Assert.That(groupBecameARosterHistoryItem.UserName, Is.EqualTo(responsibleUserName));
            Assert.That(groupBecameARosterHistoryItem.Sequence, Is.EqualTo(1));
            Assert.That(groupBecameARosterHistoryItem.TargetItemType, Is.EqualTo(QuestionnaireItemType.Group));
            Assert.That(groupBecameARosterHistoryItem.TargetItemId, Is.EqualTo(rosterId));
            Assert.That(groupBecameARosterHistoryItem.TargetItemTitle, Is.EqualTo(variable));

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
        }

        [Test]
        public void When_UpdateRoster_and_roster_became_a_group_Then_new_history_item_should_be_added_with_spacified_parameters()
        {
            // arrange
            Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
            Guid responsibleId = Guid.Parse("22222222222222222222222222222222");
            Guid groupId = Guid.Parse("33333333333333333333333333333333");
            string responsibleUserName = "responsible";
            string variable = "variable";

            AssemblyContext.SetupServiceLocator();
            var historyStorage = new TestPlainStorage<QuestionnaireChangeRecord>();
            Setup.InstanceToMockedServiceLocator<IPlainStorageAccessor<QuestionnaireChangeRecord>>(historyStorage);

            var usersStorage = new TestInMemoryWriter<AccountDocument>();
            usersStorage.Store(new AccountDocument { ProviderUserKey = responsibleId, UserName = responsibleUserName }, responsibleId.FormatGuid());
            Setup.InstanceToMockedServiceLocator<IReadSideRepositoryWriter<AccountDocument>>(usersStorage);

            var questionnaireStateTackerStorage = new InMemoryKeyValueStorage<QuestionnaireStateTracker>();
            questionnaireStateTackerStorage.Store(
                new QuestionnaireStateTracker
                {
                    CreatedBy = responsibleId,
                    RosterState = new Dictionary<Guid, string>() { { groupId, variable } }
                },
                questionnaireId.FormatGuid());
            Setup.InstanceToMockedServiceLocator<IPlainKeyValueStorage<QuestionnaireStateTracker>>(questionnaireStateTackerStorage);

            var command = Create.Command.UpdateGroup(questionnaireId, groupId, responsibleId, variableName: variable);

            var historyPostProcessor = CreateHistoryPostProcessor();
            // act
            historyPostProcessor.Process(null, command);

            // assert
            var updateRosterHistoryItem = historyStorage.Query(
                historyItems => historyItems.First(historyItem =>
                    historyItem.QuestionnaireId == questionnaireId.FormatGuid()));

            Assert.That(updateRosterHistoryItem, Is.Not.Null);
            Assert.That(updateRosterHistoryItem.QuestionnaireId, Is.EqualTo(command.QuestionnaireId.FormatGuid()));
            Assert.That(updateRosterHistoryItem.ActionType, Is.EqualTo(QuestionnaireActionType.Update));
            Assert.That(updateRosterHistoryItem.UserId, Is.EqualTo(responsibleId));
            Assert.That(updateRosterHistoryItem.UserName, Is.EqualTo(responsibleUserName));
            Assert.That(updateRosterHistoryItem.Sequence, Is.EqualTo(0));
            Assert.That(updateRosterHistoryItem.TargetItemType, Is.EqualTo(QuestionnaireItemType.Roster));
            Assert.That(updateRosterHistoryItem.TargetItemId, Is.EqualTo(groupId));
            Assert.That(updateRosterHistoryItem.TargetItemTitle, Is.EqualTo(variable));


            var rosterBecameAGroupHistoryItem = historyStorage.Query(
                historyItems => historyItems.Last(historyItem =>
                    historyItem.QuestionnaireId == questionnaireId.FormatGuid()));

            Assert.That(rosterBecameAGroupHistoryItem, Is.Not.Null);
            Assert.That(rosterBecameAGroupHistoryItem.QuestionnaireId, Is.EqualTo(command.QuestionnaireId.FormatGuid()));
            Assert.That(rosterBecameAGroupHistoryItem.ActionType, Is.EqualTo(QuestionnaireActionType.RosterBecameAGroup));
            Assert.That(rosterBecameAGroupHistoryItem.UserId, Is.EqualTo(responsibleId));
            Assert.That(rosterBecameAGroupHistoryItem.UserName, Is.EqualTo(responsibleUserName));
            Assert.That(rosterBecameAGroupHistoryItem.Sequence, Is.EqualTo(1));
            Assert.That(rosterBecameAGroupHistoryItem.TargetItemType, Is.EqualTo(QuestionnaireItemType.Roster));
            Assert.That(rosterBecameAGroupHistoryItem.TargetItemId, Is.EqualTo(groupId));
            Assert.That(rosterBecameAGroupHistoryItem.TargetItemTitle, Is.EqualTo(variable));
        }

        [Test]
        public void When_move_roster_and_group_and_static_text_and_text_qeustion_Then_4_new_history_items_should_be_added()
        {
            // arrange
            Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
            Guid responsibleId = Guid.Parse("22222222222222222222222222222222");
            Guid groupId = Guid.Parse("33333333333333333333333333333333");
            Guid staticTextId = Guid.Parse("44444444444444444444444444444444");
            Guid variableId = Guid.Parse("55555555555555555555555555555555");
            Guid questionId = Guid.Parse("66666666666666666666666666666666");
            Guid targetGroupId = Guid.Parse("77777777777777777777777777777777");

            AssemblyContext.SetupServiceLocator();
            var historyStorage = new TestPlainStorage<QuestionnaireChangeRecord>();
            Setup.InstanceToMockedServiceLocator<IPlainStorageAccessor<QuestionnaireChangeRecord>>(historyStorage);

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
            Setup.InstanceToMockedServiceLocator<IPlainKeyValueStorage<QuestionnaireStateTracker>>(questionnaireStateTackerStorage);

            var moveGroupCommand = Create.Command.MoveGroup(questionnaireId, groupId, responsibleId, targetGroupId);
            var moveStaticTextCommand = Create.Command.MoveStaticText(questionnaireId, staticTextId, responsibleId, targetGroupId);
            var moveVariableCommand = Create.Command.MoveVariable(questionnaireId, variableId, responsibleId, targetGroupId);
            var moveQuestionCommand = Create.Command.MoveQuestion(questionnaireId, questionId, responsibleId, targetGroupId);

            var historyPostProcessor = CreateHistoryPostProcessor();
            // act
            historyPostProcessor.Process(null, moveGroupCommand);
            historyPostProcessor.Process(null, moveStaticTextCommand);
            historyPostProcessor.Process(null, moveVariableCommand);
            historyPostProcessor.Process(null, moveQuestionCommand);

            // assert
            var newHistoryItems = historyStorage.Query(historyItems => historyItems.ToArray());

            Assert.That(newHistoryItems.Length, Is.EqualTo(4));
            Assert.That(newHistoryItems[0].TargetItemType, Is.EqualTo(QuestionnaireItemType.Group));
            Assert.That(newHistoryItems[1].TargetItemType, Is.EqualTo(QuestionnaireItemType.StaticText));
            Assert.That(newHistoryItems[2].TargetItemType, Is.EqualTo(QuestionnaireItemType.Variable));
            Assert.That(newHistoryItems[3].TargetItemType, Is.EqualTo(QuestionnaireItemType.Question));
        }

        [Test]
        public void When_pasting_after_group_Then_children_should_be_added_to_history()
        {
            // arrange
            Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
            Guid responsibleId = Guid.Parse("22222222222222222222222222222222");
            Guid groupId = Guid.Parse("33333333333333333333333333333333");
            Guid staticTextId = Guid.Parse("44444444444444444444444444444444");
            Guid variableId = Guid.Parse("55555555555555555555555555555555");
            Guid questionId = Guid.Parse("66666666666666666666666666666666");
            Guid targetGroupId = Guid.Parse("77777777777777777777777777777777");

            AssemblyContext.SetupServiceLocator();
            var historyStorage = new TestPlainStorage<QuestionnaireChangeRecord>();
            Setup.InstanceToMockedServiceLocator<IPlainStorageAccessor<QuestionnaireChangeRecord>>(historyStorage);

            var questionnaireStateTackerStorage = new InMemoryKeyValueStorage<QuestionnaireStateTracker>();
            questionnaireStateTackerStorage.Store(
                new QuestionnaireStateTracker
                {
                    CreatedBy = responsibleId,
                    GroupsState = new Dictionary<Guid, string>() { { groupId, "" } },
                    VariableState = new Dictionary<Guid, string>() {  },
                    StaticTextState = new Dictionary<Guid, string>() {  },
                    QuestionsState = new Dictionary<Guid, string>() {  }
                },
                questionnaireId.FormatGuid());
            Setup.InstanceToMockedServiceLocator<IPlainKeyValueStorage<QuestionnaireStateTracker>>(questionnaireStateTackerStorage);

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
                }
            };
            
            var questionnaire = Create.Questionnaire();

            questionnaire.Initialize(questionnaireId, questionnaireDocument, Enumerable.Empty<SharedPerson>());
            var pastAfterCommand = Create.Command.PasteAfter(questionnaireId, targetGroupId, groupId, questionnaireId, groupId, responsibleId);
            var historyPostProcessor = CreateHistoryPostProcessor();

            //questionnaireStateTackerStorage

            // act
            historyPostProcessor.Process(questionnaire, pastAfterCommand);

            // assert
            var newHistoryItems = historyStorage.Query(historyItems => historyItems.ToArray());
            var state = questionnaireStateTackerStorage.GetById(questionnaireId.FormatGuid());

            Assert.That(newHistoryItems.Length, Is.EqualTo(1));
            Assert.That(newHistoryItems[0].TargetItemType, Is.EqualTo(QuestionnaireItemType.Group));

            Assert.That(state.VariableState.ContainsKey(variableId));
            Assert.That(state.QuestionsState.ContainsKey(questionId));
            Assert.That(state.StaticTextState.ContainsKey(staticTextId));
            Assert.That(state.GroupsState.ContainsKey(targetGroupId));
        }


        private static HistoryPostProcessor CreateHistoryPostProcessor() => Create.HistoryPostProcessor();
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.AbstractFactories;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Main.Core.EventHandlers;
using Main.Core.Events.Questionnaire;
using Main.Core.Tests.EventHandlers;
using NUnit.Framework;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;

namespace Main.DenormalizerStorage.Tests
{
    [TestFixture]
    public class QuestionnaireDenormalizerTests
    {
        [Test]
        public void HandleGroupUpdated_When_GroupUpdated_event_is_come_and_group_propagation_kind_is_changed_from_AutoPropagate_to_None_Then_all_triggers_in_autoptopagate_questions_should_not_contains_this_group_id()
        {
            // Arrange
            Guid questionnaireId = Guid.NewGuid();
            Guid updatedGroupId = Guid.NewGuid();
            Guid autoQuestionId = Guid.NewGuid();

            var storageStub = new QuestionnaireDenormalizerStorageStub
                {
                    Document = this.CreateQuestionnaireDocumentWithAutoPropagateGroupAndRegularGroupWithAutoPropagateQuestion(questionnaireId, updatedGroupId, autoQuestionId)
                };

            var target = new QuestionnaireDenormalizer(storageStub, new CompleteQuestionFactory());

            GroupUpdated @event = CreateGroupUpdatedEvent(updatedGroupId, propagationKind: Propagate.None);

            // Act
            target.Handle((IPublishedEvent<GroupUpdated>) CreatePublishedEvent(questionnaireId, @event));

            // Assert
            #warning: 'TLK: searching for question is not тру. Have you some ideas?
            var autoQuestion = storageStub.Document.Find<AbstractQuestion>(autoQuestionId) as AutoPropagateQuestion;
            Assert.That(autoQuestion.Triggers, !Contains.Item(updatedGroupId));
        }

        [Test]
        public void HandleQuestionChanged_AutopropagateQuestionUpdateEventIsCome_TriggersAndMaxValueUpdated()
        {
            // Arrange
            var triggers = new List<Guid> {Guid.NewGuid(), Guid.NewGuid()};
            var innerDocument = new QuestionnaireDocument();
            var group = new Group("group");
            innerDocument.Children.Add(@group);
            var question = new AutoPropagateQuestion("question")
                {
                    MaxValue = 8,
                    Triggers = new List<Guid>(),
                    QuestionType = QuestionType.AutoPropagate
                };
            @group.Children.Add(question);

            var storageStub = new QuestionnaireDenormalizerStorageStub
                {
                    Document = innerDocument
                };

            var target = new QuestionnaireDenormalizer(storageStub, new CompleteQuestionFactory());

            var evnt = new QuestionChanged
                {
                    QuestionType = QuestionType.AutoPropagate,
                    PublicKey = question.PublicKey,
                    Featured = true,
                    MaxValue = 10,
                    Triggers = triggers
                };

            // Act
            target.Handle(CreatePublishedEvent(innerDocument.PublicKey, evnt));

            // Assert
            var resultQuestion = @group.Find<AutoPropagateQuestion>(question.PublicKey);

            Assert.AreEqual(evnt.Featured, resultQuestion.Featured);
            Assert.AreEqual(evnt.MaxValue, resultQuestion.MaxValue);
            Assert.AreEqual(evnt.Triggers.Count, resultQuestion.Triggers.Count);
            Assert.IsTrue(!resultQuestion.Triggers.Except(triggers).Any(), "Triiggers list is not updated");
        }

        [Test]
        public void HandleQuestionChanged_When_QuestionUpdate_event_is_come_Then_all_abstractQuestion_fields_are_updated()
        {
            // Arrange
            Guid questionnaireId = Guid.NewGuid();
            Guid groupId = Guid.NewGuid();
            Guid questionId = Guid.NewGuid();

            var storageStub = new QuestionnaireDenormalizerStorageStub
                {
                    Document = CreateQuestionnaireDocument(questionnaireId, groupId, questionId)
                };

            var target = new QuestionnaireDenormalizer(storageStub, new CompleteQuestionFactory());

            QuestionChanged evnt = CreateQuestionChangedEvent(questionId);

            // Act
            target.Handle(CreatePublishedEvent(questionnaireId, evnt));

            // Assert
            var question = storageStub.Document.Find<IQuestion>(questionId);

            Assert.That(evnt.QuestionText, Is.EqualTo(question.QuestionText), "QuestionText is not updated");
            Assert.That(evnt.QuestionType, Is.EqualTo(question.QuestionType), "QuestionType is not updated");
            Assert.That(evnt.Featured, Is.EqualTo(question.Featured), "Featured is not updated");
            Assert.That(evnt.AnswerOrder, Is.EqualTo(question.AnswerOrder), "AnswerOrder is not updated");
            Assert.That(evnt.ConditionExpression, Is.EqualTo(question.ConditionExpression),
                        "ConditionExpression is not updated");
            Assert.That(evnt.Instructions, Is.EqualTo(question.Instructions), "Instructions is not updated");
            Assert.That(evnt.StataExportCaption, Is.EqualTo(question.StataExportCaption),
                        "StataExportCaption is not updated");
            Assert.That(evnt.ValidationExpression, Is.EqualTo(question.ValidationExpression),
                        "ValidationExpression is not updated");
            Assert.That(evnt.ValidationMessage, Is.EqualTo(question.ValidationMessage),
                        "ValidationMessage is not updated");
        }

        private static IPublishedEvent<T> CreatePublishedEvent<T>(Guid questionnaireId, T evnt)
        {
            IPublishedEvent<T> e = new PublishedEvent<T>(new UncommittedEvent(Guid.NewGuid(),
                                                                              questionnaireId,
                                                                              1,
                                                                              1,
                                                                              DateTime.Now,
                                                                              evnt,
                                                                              new Version(1, 0))
                );
            return e;
        }

        private static QuestionChanged CreateQuestionChangedEvent(Guid questionId)
        {
            return new QuestionChanged
                {
                    QuestionText = "What is your name",
                    QuestionType = QuestionType.Text,
                    PublicKey = questionId,
                    Featured = true,
                    AnswerOrder = Order.AsIs,
                    ConditionExpression = string.Empty,
                    Answers = null,
                    Instructions = "Answer this question, please",
                    StataExportCaption = "name",
                    ValidationExpression = "[this]!=''",
                    ValidationMessage = "Empty names is invalid answer"
                };
        }

        private static GroupUpdated CreateGroupUpdatedEvent(Guid groupId, Propagate propagationKind = Propagate.None)
        {
            return new GroupUpdated
                {
                    GroupPublicKey = groupId,
                    Propagateble = propagationKind
                };
        }

        private static QuestionnaireDocument CreateQuestionnaireDocument(Guid questionnaireId, Guid groupId,
                                                                         Guid questionId)
        {
            var innerDocument = new QuestionnaireDocument {PublicKey = questionnaireId};
            var group = new Group("group") {PublicKey = groupId};
            innerDocument.Children.Add(@group);
            var question = new TextQuestion("question")
                {
                    PublicKey = questionId,
                    QuestionType = QuestionType.Text,
                    AnswerOrder = Order.AZ,
                    Capital = false,
                    ConditionExpression = "[f7b6842d-c17f-495c-bcbd-ba96dd64e527]==1",
                    Featured = false,
                    Answers = null,
                    Comments = "no comments",
                    Instructions = string.Empty,
                    StataExportCaption = "text",
                    ValidationExpression = string.Empty,
                    ValidationMessage = string.Empty
                };
            @group.Children.Add(question);
            return innerDocument;
        }

        private QuestionnaireDocument
            CreateQuestionnaireDocumentWithAutoPropagateGroupAndRegularGroupWithAutoPropagateQuestion(
            Guid questionnaireId, Guid autoGroupId, Guid autoQuestionId)
        {
            Guid groupId = Guid.NewGuid();
            var innerDocument = new QuestionnaireDocument {PublicKey = questionnaireId};
            var regularGroup = new Group("group") {PublicKey = groupId};
            innerDocument.Children.Add(regularGroup);

            var autoPropagateGroup = new Group("group") {PublicKey = autoGroupId, Propagated = Propagate.AutoPropagated};
            innerDocument.Children.Add(autoPropagateGroup);

            var autoPropagateQuestion = new AutoPropagateQuestion("question")
                {
                    PublicKey = autoQuestionId,
                    QuestionType = QuestionType.AutoPropagate,
                    Triggers = new List<Guid> {autoGroupId}
                };
            regularGroup.Children.Add(autoPropagateQuestion);

            return innerDocument;
        }
    }
}
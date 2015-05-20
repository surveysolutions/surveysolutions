using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Moq;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Document;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireDenormalizerTests
{
    [TestFixture]
    public class QuestionnaireDenormalizerTests
    {
        [SetUp]
        public void SetUp()
        {
            AssemblyContext.SetupServiceLocator();
        }

        [Test]
        public void HandleQuestionChanged_When_QuestionUpdate_event_is_come_Then_all_abstractQuestion_fields_are_updated()
        {
            // Arrange
            var questionnaireId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var questionId = Guid.Parse("22222222-2222-2222-2222-222222222222");

            var innerDocument = CreateQuestionnaireDocument(questionnaireId);

            innerDocument
                .AddChapter(Guid.NewGuid())
                .AddQuestion(questionId);

            var storageStub = CreateQuestionnaireDenormalizerStorageStub(innerDocument);

            var denormalizer = CreateQuestionnaireDenormalizer(storageStub);

            QuestionChanged evnt = CreateQuestionChangedEvent(questionId);

            // Act
            denormalizer.Handle(CreatePublishedEvent(questionnaireId, evnt));

            // Assert
            #warning: updated question is a new entity, that's why we should search for it by it's id
            var question = innerDocument.Find<IQuestion>(questionId);

            Assert.That(evnt.QuestionText, Is.EqualTo(question.QuestionText));
            Assert.That(evnt.QuestionType, Is.EqualTo(question.QuestionType));
            Assert.That(evnt.Featured, Is.EqualTo(question.Featured));
            Assert.That(evnt.AnswerOrder, Is.EqualTo(question.AnswerOrder));
            Assert.That(evnt.ConditionExpression, Is.EqualTo(question.ConditionExpression));
            Assert.That(evnt.Instructions, Is.EqualTo(question.Instructions));
            Assert.That(evnt.StataExportCaption, Is.EqualTo(question.StataExportCaption));
            Assert.That(evnt.ValidationExpression, Is.EqualTo(question.ValidationExpression));
            Assert.That(evnt.ValidationMessage, Is.EqualTo(question.ValidationMessage));
        }

        private static QuestionnaireDenormalizer CreateQuestionnaireDenormalizer(Mock<IReadSideKeyValueStorage<QuestionnaireDocument>> storageStub)
        {
            #warning: we shouldn't use CompleteQuestionFactory here?
            var denormalizer = new QuestionnaireDenormalizer(storageStub.Object, new QuestionnaireEntityFactory(), Mock.Of<ILogger>());

            return denormalizer;
        }

        private static Mock<IReadSideKeyValueStorage<QuestionnaireDocument>> CreateQuestionnaireDenormalizerStorageStub(QuestionnaireDocument document)
        {
            var storageStub = new Mock<IReadSideKeyValueStorage<QuestionnaireDocument>>();

            storageStub.Setup(d => d.GetById(document.PublicKey.FormatGuid())).Returns(document);

            return storageStub;
        }

        private static QuestionnaireDocument CreateQuestionnaireDocument(Guid questionnaireId)
        {
            var innerDocument = new QuestionnaireDocument
            {
                Title = string.Format("Questionnaire {0}", questionnaireId),
                PublicKey = questionnaireId
            };
            return innerDocument;
        }

        private static QuestionChanged CreateQuestionChangedEvent(Guid questionId, QuestionType type = QuestionType.Text, int maxValue = 0, List<Guid> triggers = null)
        {
            return new QuestionChanged
                {
                    QuestionText = "What is your name",
                    QuestionType = type,
                    PublicKey = questionId,
                    Featured = true,
                    AnswerOrder = Order.AsIs,
                    ConditionExpression = string.Empty,
                    Answers = null,
                    Instructions = "Answer this question, please",
                    StataExportCaption = "name",
                    ValidationExpression = "[this]!=''",
                    ValidationMessage = "Empty names is invalid answer",
                    Triggers = triggers,
                };
        }

        private static GroupUpdated CreateGroupUpdatedEvent(Guid groupId, Propagate propagationKind = Propagate.None)
        {
            return new GroupUpdated
                {
                    GroupPublicKey = groupId
                };
        }

        private static IPublishedEvent<T> CreatePublishedEvent<T>(Guid questionnaireId, T evnt)
        {
            IPublishedEvent<T> e = new PublishedEvent<T>(new UncommittedEvent(Guid.NewGuid(),
                                                                              questionnaireId,
                                                                              1,
                                                                              1,
                                                                              DateTime.Now,
                                                                              evnt)
                );
            return e;
        }
    }
}
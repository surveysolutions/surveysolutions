using System;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Domain;
using Main.Core.Events.Questionnaire;
using Main.Core.View;
using Moq;
using NUnit.Framework;
using Ncqrs;
using Ncqrs.Domain;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using Ncqrs.Restoring.EventStapshoot;
using Ncqrs.Spec;
using WB.Core.Questionnaire.ImportService;
using WB.Core.Questionnaire.ImportService.Commands;
using WB.UI.Designer.Views.Questionnaire;

namespace WB.Core.Questionnaire.Tests
{
    [TestFixture]
    public class DefaultImportServiceTests
    {
        protected Mock<IUnitOfWorkContext> unitOfWorkContextMock;
        protected Mock<IEventStore> eventStoreMock;
        protected Mock<IEventBus> eventBusMock;
        protected Mock<IUnitOfWorkFactory> unitOfWorkFactoryMock;
       
        [SetUp]
        public void Setup()
        {
            unitOfWorkContextMock=new Mock<IUnitOfWorkContext>();
            eventStoreMock=new Mock<IEventStore>();
            eventBusMock = new Mock<IEventBus>();
            unitOfWorkFactoryMock=new Mock<IUnitOfWorkFactory>();
            unitOfWorkFactoryMock.Setup(x => x.CreateUnitOfWork(It.IsAny<Guid>())).Returns(unitOfWorkContextMock.Object);
            NcqrsEnvironment.SetDefault(eventStoreMock.Object);
            NcqrsEnvironment.SetDefault(eventBusMock.Object);
        }

        [Test]
        public void Execute_When_UserExistsSourceIsDocument_Then_NewQuestionnaireIsSaved()
        {
            // arrange
            DefaultImportService importService = CreateDefaultImportService();
            var questionnaireId = Guid.NewGuid();
            var creatorId = Guid.NewGuid();
            // act
            importService.Execute(new ImportQuestionnaireCommand(creatorId,
                                                                 new QuestionnaireDocument()
                                                                     {
                                                                         Title = "new questionnaire",
                                                                         PublicKey = questionnaireId
                                                                     }));
            // assert
            Func<QuestionnaireDocument, bool> validateEventFunk = (evt) =>
                {
                    if (questionnaireId != evt.PublicKey)
                        return false;
                    if (creatorId != evt.CreatedBy)
                        return false;
                    if ("new questionnaire" != evt.Title)
                        return false;
                    return true;
                };
            ValidateFirstEventStored(validateEventFunk);
            ValidateFirstEventPublished(validateEventFunk);
        }

        [Test]
        public void Execute_When_QuestionnairePresentWithSameId_Then_ArgumentException_should_be_thrown()
        {
            // arrange
            DefaultImportService importService = CreateDefaultImportService();
            var questionnaireId = Guid.NewGuid();
            this.unitOfWorkContextMock.Setup(x => x.GetById<QuestionnaireAR>(questionnaireId))
                .Returns(new QuestionnaireAR());
            // act
            TestDelegate act =
                () =>
                importService.Execute(new ImportQuestionnaireCommand(Guid.NewGuid(),
                                                                     new QuestionnaireDocument(){ PublicKey = questionnaireId}));
            // assert
            Assert.Throws<ArgumentException>(act);
        }

        [Test]
        public void Execute_When_SourceIsNotQuestionnaireDocument_Then_ArgumentException_should_be_thrown()
        {
            // arrange
            DefaultImportService importService = CreateDefaultImportService();
            Mock<IQuestionnaireDocument> docMock=new Mock<IQuestionnaireDocument>();
            // act
            TestDelegate act =
                () =>
                importService.Execute(new ImportQuestionnaireCommand( Guid.NewGuid(), docMock.Object));
            // assert
            Assert.Throws<ArgumentException>(act);
        }

        private DefaultImportService CreateDefaultImportService()
        {
            return new DefaultImportService(unitOfWorkFactoryMock.Object);
        }

        private void ValidateFirstEventStored<T>(Func<T, bool> validator) where T : class
        {
            eventStoreMock.Verify(x => x.Store(It.Is<UncommittedEventStream>(s => ValidateStream(s, validator))),
                                  Times.Once());
        }

        private void ValidateFirstEventPublished<T>(Func<T, bool> validator) where T : class
        {
            eventBusMock.Verify(x => x.Publish(It.Is<UncommittedEvent>(e => ValudateEvent(e, validator))),
                                  Times.Once());
        }

        private bool ValidateStream<T>(UncommittedEventStream stream, Func<T, bool> validator) where T : class
        {
            if (!stream.Any())
                return false;
            return ValudateEvent(stream.First(), validator);
        }

        private bool ValudateEvent<T>(UncommittedEvent evt, Func<T, bool> validator) where T : class
        {
            var snapshotEvent = evt.Payload as SnapshootLoaded;
            if (snapshotEvent == null)
                return false;
            var doc = snapshotEvent.Template.Payload as T;
            if (doc == null)
                return false;
            return validator(doc);
        }
    }
}

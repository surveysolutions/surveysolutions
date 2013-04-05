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
      
        protected EventStoreStub eventStoreStub;
        protected EventBusStub eventBusStub;
        
        [SetUp]
        public void Setup()
        {
            eventStoreStub = new EventStoreStub();
            eventBusStub = new EventBusStub();
            NcqrsEnvironment.SetDefault<IEventStore>(eventStoreStub);
            NcqrsEnvironment.SetDefault<IEventBus>(eventBusStub);
        }

        [Test]
        public void Execute_When_UserExistsSourceIsDocument_Then_NewQuestionnaireIsSavedToEventStore()
        {
            // arrange
            DefaultImportService importService = CreateDefaultImportService();
            var questionnaireId = Guid.NewGuid();
            var creatorId = Guid.NewGuid();
            // act
            string questionnaireTitle = "new questionnaire";
            importService.Execute(new ImportQuestionnaireCommand(creatorId,
                                                                 new QuestionnaireDocument()
                                                                     {
                                                                         Title = questionnaireTitle,
                                                                         PublicKey = questionnaireId
                                                                     }));
            // assert
            var storedEvent =GetSingleEventFromStore<SnapshootLoaded>();
            Assert.That(storedEvent.Template.Payload, Is.TypeOf<QuestionnaireDocument>());
            var storedDocument = storedEvent.Template.Payload as QuestionnaireDocument;
            Assert.That(storedDocument.CreatedBy, Is.EqualTo(creatorId));
            Assert.That(storedDocument.PublicKey, Is.EqualTo(questionnaireId));
            Assert.That(storedDocument.Title, Is.EqualTo(questionnaireTitle));

            
        }

        [Test]
        public void Execute_When_UserExistsSourceIsDocument_Then_StoredEventEqualToPublished()
        {
            // arrange
            DefaultImportService importService = CreateDefaultImportService();
            var questionnaireId = Guid.NewGuid();
            var creatorId = Guid.NewGuid();
            // act
            string questionnaireTitle = "new questionnaire";
            importService.Execute(new ImportQuestionnaireCommand(creatorId,
                                                                 new QuestionnaireDocument()
                                                                 {
                                                                     Title = questionnaireTitle,
                                                                     PublicKey = questionnaireId
                                                                 }));
            // assert
            var storedEvent = GetSingleEventFromStore<SnapshootLoaded>();
            var publishedEvent = GetSingleEventFromBus<SnapshootLoaded>();
            Assert.That(publishedEvent, Is.EqualTo(storedEvent));
        }

        [Test]
        public void Execute_When_QuestionnairePresentWithSameId_Then_TemplateLoadedAsLastEventInStream()
        {
            // arrange

            var ar = new QuestionnaireAR();
            ar.InitializeFromHistory(new CommittedEventStream(ar.EventSourceId,
                                                              new CommittedEvent(Guid.NewGuid(), Guid.NewGuid(),
                                                                                 ar.EventSourceId, 1, DateTime.Now,
                                                                                 new NewQuestionnaireCreated(),
                                                                                 new Version())));

            DefaultImportService importService = CreateDefaultImportServiceWhichIsReturnPassedAR(ar);
            // act

            importService.Execute(new ImportQuestionnaireCommand(Guid.NewGuid(),
                                                                 new QuestionnaireDocument()
                                                                     {
                                                                         PublicKey = ar.EventSourceId
                                                                     }));
            // assert
            var storedEvent = GetSingleEventFromStore<SnapshootLoaded>();
            Assert.That(storedEvent.Template.Version, Is.EqualTo(2));
            //Assert.That(storedEvent.);
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

        private T GetSingleEventFromStore<T>()
        {
            return (T)eventStoreStub.Events.Single(e => e.Payload is T).Payload;
        }

        private T GetSingleEventFromBus<T>()
        {
            return (T)eventBusStub.Events.Single(e => e.Payload is T).Payload;
        }

        private DefaultImportService CreateDefaultImportService()
        {
            return new DefaultImportService();
        }

        private DefaultImportService CreateDefaultImportServiceWhichIsReturnPassedAR(QuestionnaireAR ar)
        {
            Mock<IUnitOfWorkContext> unitOfWorkContextMock = new Mock<IUnitOfWorkContext>();
            Mock<IUnitOfWorkFactory> unitOfWorkFactoryMock = new Mock<IUnitOfWorkFactory>();
            unitOfWorkFactoryMock.Setup(x => x.CreateUnitOfWork(It.IsAny<Guid>())).Returns(unitOfWorkContextMock.Object);
            unitOfWorkContextMock.Setup(x => x.GetById<QuestionnaireAR>(ar.EventSourceId))
               .Returns(ar);
            return new DefaultImportService(unitOfWorkFactoryMock.Object);

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

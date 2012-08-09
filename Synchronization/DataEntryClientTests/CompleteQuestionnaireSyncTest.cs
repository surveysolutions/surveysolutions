using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using DataEntryClient.CompleteQuestionnaire;
using DataEntryClient.WcfInfrastructure;
using DataEntryClientTests.Stubs;
using Moq;
using NUnit.Framework;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Ninject;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.ClientSettingsProvider;
using RavenQuestionnaire.Core.Commands.Questionnaire.Question;
using RavenQuestionnaire.Core.Commands.Synchronization;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Events;
using RavenQuestionnaire.Core.Views.ClientSettings;
using RavenQuestionnaire.Core.Views.Event;
using SynchronizationMessages.CompleteQuestionnaire;
using SynchronizationMessages.Handshake;

namespace DataEntryClientTests
{
    [TestFixture]
    public class CompleteQuestionnaireSyncTest
    {
        protected IKernel Kernel;
        protected Mock<ICommandService> CommandService;
        protected Mock<IEventSync> EventStore;
    //    protected Mock<IClientSettingsProvider> clientSettingsMock;
        [SetUp]
        public void CreateObjects()
        {
            
            CommandService=new Mock<ICommandService>();
            Kernel = new StandardKernel();
            EventStore=new Mock<IEventSync>();
            Kernel.Bind<IEventSync>().ToConstant(EventStore.Object);

          /*  clientSettingsMock = new Mock<IClientSettingsProvider>();
            clientSettingsMock.Setup(x => x.ClientSettings).Returns(
                new ClientSettingsView(new ClientSettingsDocument() { PublicKey = Guid.NewGuid() }));
            Kernel.Bind<IClientSettingsProvider>().ToConstant(clientSettingsMock.Object);*/
            NcqrsEnvironment.SetDefault<ICommandService>(CommandService.Object);
        }

        [Test]
        public void GetLastEventGuid_ResultNotNull_GuidIsReturned()
        {
            Mock<IGetLastSyncEvent> serviceMock = new Mock<IGetLastSyncEvent>();
            IChanelFactoryWrapper chanelFactoryStub = new ChanelFactoryStub(serviceMock);
            Kernel.Bind<IChanelFactoryWrapper>().ToConstant(chanelFactoryStub);
            

            var clientGuid = Guid.NewGuid();
            var eventGuid = Guid.NewGuid();

            serviceMock.Setup(x => x.Process(clientGuid)).Returns(eventGuid);
                 var target = new CompleteQuestionnaireSync(Kernel, Guid.NewGuid(), string.Empty);

            var result =target.GetLastSyncEventGuid(clientGuid);
            Assert.AreEqual(result, eventGuid);
        }
        [Test]
        public void GetLastEventGuid_ResultNotNull_GuidIsNotReturned()
        {
            Mock<IGetLastSyncEvent> serviceMock = new Mock<IGetLastSyncEvent>();
            IChanelFactoryWrapper chanelFactoryStub = new ChanelFactoryStub(serviceMock);
            Kernel.Bind<IChanelFactoryWrapper>().ToConstant(chanelFactoryStub);
            var clientGuid = Guid.NewGuid();
            Guid? eventGuid = null;

            serviceMock.Setup(x => x.Process(clientGuid)).Returns(eventGuid);
              var target = new CompleteQuestionnaireSync(Kernel, Guid.NewGuid(), string.Empty);

            var result = target.GetLastSyncEventGuid(clientGuid);
            Assert.AreEqual(result, null);
        }
        [Test]
        public void UploadEvents_2Events_AllEventsAreDelivered()
        {
           
            Mock<IEventPipe> serviceMock = new Mock<IEventPipe>();
            IChanelFactoryWrapper chanelFactoryStub = new ChanelFactoryStub(serviceMock);
            Kernel.Bind<IChanelFactoryWrapper>().ToConstant(chanelFactoryStub);

            var clientGuid = Guid.NewGuid();
            Guid? eventGuid = Guid.NewGuid();
            
            serviceMock.Setup(x => x.Process(It.IsAny<EventSyncMessage>())).Returns(ErrorCodes.None);
            EventStore.Setup(x => x.ReadCompleteQuestionare()).Returns(new List<AggregateRootEventStream>()
                                                                           {

                                                                               new AggregateRootEventStream(
                                                                                   new []
                                                                                       {
                                                                                           new AggregateRootEvent(){EventIdentifier = Guid.NewGuid()},
                                                                                           new AggregateRootEvent(){EventIdentifier = Guid.NewGuid()}
                                                                                       }, long.MinValue, long.MaxValue,
                                                                                   Guid.NewGuid())
                                                                           });
            var target = new CompleteQuestionnaireSync(Kernel, Guid.NewGuid(), string.Empty);

            target.UploadEvents(clientGuid,eventGuid);

            serviceMock.Verify(x => x.Process(It.Is<EventSyncMessage>(e => e.Command.Events.Count() == 2)),
                               Times.Exactly(1));
            //events were pushed
            CommandService.Verify(x => x.Execute(It.IsAny<PushEventsCommand>()),Times.Once());
            //events were marked as started and later as completes
            CommandService.Verify(x => x.Execute(It.IsAny<ChangeEventStatusCommand>()), Times.Exactly(2));
            //process is finisheed
            CommandService.Verify(x => x.Execute(It.IsAny<EndProcessComand>()), Times.Once());

        }
        [Test]
        public void ImportEvents_2Events_AllEventsAreDeliveredToClient()
        {

            Mock<IGetAggragateRootList> serviceMock = new Mock<IGetAggragateRootList>();
            Mock<IGetEventStream> eventServiceMock = new Mock<IGetEventStream>();
            IChanelFactoryWrapper chanelFactoryStub =
                new ChanelFactoryStub(new object[] {serviceMock, eventServiceMock});
            Kernel.Bind<IChanelFactoryWrapper>().ToConstant(chanelFactoryStub);

            var clientGuid = Guid.NewGuid();
            Guid? eventGuid = Guid.NewGuid();
            var serviceResult = new ListOfAggregateRootsForImportMessage()
                                    {
                                        Roots =
                                            new ProcessedAggregateRoot[]
                                                {new ProcessedAggregateRoot() {AggregateRootPublicKey = Guid.NewGuid()}}
                                    };

            serviceMock.Setup(x => x.Process()).Returns(serviceResult);
            eventServiceMock.Setup(x => x.Process(serviceResult.Roots[0].AggregateRootPublicKey)).Returns(new ImportSynchronizationMessage());
            var target = new CompleteQuestionnaireSync(Kernel, Guid.NewGuid(), string.Empty);

            target.Import(Guid.NewGuid());

            serviceMock.Verify(x => x.Process(),
                               Times.Exactly(1));
            eventServiceMock.Verify(x => x.Process(serviceResult.Roots[0].AggregateRootPublicKey), Times.Exactly(1));

            EventStore.Verify(x => x.WriteEvents(It.IsAny<IEnumerable<AggregateRootEventStream>>()), Times.Exactly(1));


        }
    }
}

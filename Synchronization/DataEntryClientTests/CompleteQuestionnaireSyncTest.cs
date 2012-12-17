// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteQuestionnaireSyncTest.cs" company="">
//   
// </copyright>
// <summary>
//   The complete questionnaire sync test.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataEntryClientTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DataEntryClient.CompleteQuestionnaire;
    using DataEntryClient.SycProcess;
    using DataEntryClient.WcfInfrastructure;

    using DataEntryClientTests.Stubs;

    using Main.Core.Commands.Synchronization;
    using Main.Core.Documents;
    using Main.Core.Events;

    using Moq;

    using Ncqrs;
    using Ncqrs.Commanding.ServiceModel;

    using Ninject;

    using NUnit.Framework;

    using SynchronizationMessages.CompleteQuestionnaire;
    using SynchronizationMessages.Handshake;

    /// <summary>
    /// The complete questionnaire sync test.
    /// </summary>
    [TestFixture]
    public class CompleteQuestionnaireSyncTest
    {
        #region Fields

        /// <summary>
        /// The command service.
        /// </summary>
        protected Mock<ICommandService> CommandService;

        /// <summary>
        /// The event store.
        /// </summary>
        protected Mock<IEventStreamReader> EventStore;

        /// <summary>
        /// The kernel.
        /// </summary>
        protected IKernel Kernel;

        /// <summary>
        /// Sync repository
        /// </summary>
        private Mock<ISyncProcessRepository> SyncProcessRepository;

        #endregion

        // protected Mock<IClientSettingsProvider> clientSettingsMock;
        #region Public Methods and Operators

        /// <summary>
        /// The create objects.
        /// </summary>
        [SetUp]
        public void CreateObjects()
        {
            this.CommandService = new Mock<ICommandService>();
            this.Kernel = new StandardKernel();

            this.EventStore = new Mock<IEventStreamReader>();
            this.Kernel.Bind<IEventStreamReader>().ToConstant(this.EventStore.Object);

            this.SyncProcessRepository = new Mock<ISyncProcessRepository>();
            this.Kernel.Bind<ISyncProcessRepository>().ToConstant(this.SyncProcessRepository.Object);

            /*  clientSettingsMock = new Mock<IClientSettingsProvider>();
            clientSettingsMock.Setup(x => x.ClientSettings).Returns(
                new ClientSettingsView(new ClientSettingsDocument() { PublicKey = Guid.NewGuid() }));
            Kernel.Bind<IClientSettingsProvider>().ToConstant(clientSettingsMock.Object);*/
            NcqrsEnvironment.SetDefault<ICommandService>(this.CommandService.Object);
        }

        /// <summary>
        /// The get last event guid_ result not null_ guid is not returned.
        /// </summary>
        [Test]
        public void GetLastEventGuid_ResultNotNull_GuidIsNotReturned()
        {
            var serviceMock = new Mock<IGetLastSyncEvent>();
            IChanelFactoryWrapper chanelFactoryStub = new ChanelFactoryStub(serviceMock);
            this.Kernel.Bind<IChanelFactoryWrapper>().ToConstant(chanelFactoryStub);
            Guid clientGuid = Guid.NewGuid();
            Guid? eventGuid = null;

            serviceMock.Setup(x => x.Process(clientGuid)).Returns(eventGuid);
            var target = new WirelessSyncProcess(this.Kernel, Guid.NewGuid(), string.Empty);
            Guid? result = target.GetLastSyncEventGuid(clientGuid);
            Assert.AreEqual(result, null);
        }

        /// <summary>
        /// The get last event guid_ result not null_ guid is returned.
        /// </summary>
        [Test]
        public void GetLastEventGuid_ResultNotNull_GuidIsReturned()
        {
            var serviceMock = new Mock<IGetLastSyncEvent>();
            IChanelFactoryWrapper chanelFactoryStub = new ChanelFactoryStub(serviceMock);
            this.Kernel.Bind<IChanelFactoryWrapper>().ToConstant(chanelFactoryStub);

            Guid clientGuid = Guid.NewGuid();
            Guid eventGuid = Guid.NewGuid();

            serviceMock.Setup(x => x.Process(clientGuid)).Returns(eventGuid);
            var target = new WirelessSyncProcess(this.Kernel, Guid.NewGuid(), string.Empty);

            Guid? result = target.GetLastSyncEventGuid(clientGuid);
            Assert.AreEqual(result, eventGuid);
        }

        /// <summary>
        /// The import events_2 events_ all events are delivered to client.
        /// </summary>
        [Test]
        public void ImportEvents_2Events_AllEventsAreDeliveredToClient()
        {
            var serviceMock = new Mock<IGetAggragateRootList>();
            var eventServiceMock = new Mock<IGetEventStream>();
            IChanelFactoryWrapper chanelFactoryStub =
                new ChanelFactoryStub(new object[] { serviceMock, eventServiceMock });
            this.Kernel.Bind<IChanelFactoryWrapper>().ToConstant(chanelFactoryStub);

            Guid clientGuid = Guid.NewGuid();
            Guid? eventGuid = Guid.NewGuid();
            var syncProcessGiud = Guid.NewGuid();
            var serviceResult = new ListOfAggregateRootsForImportMessage
                {
                    Roots =
                        new[]
                            {
                                new ProcessedEventChunk
                                    {
                                        EventChunckPublicKey = Guid.NewGuid(), 
                                        EventKeys = new List<Guid> { Guid.NewGuid() }
                                    }
                            }
                };

            this.SyncProcessRepository.Setup(x => x.GetProcess(It.IsAny<Guid>())).Returns(new SyncProcessorStub());
            serviceMock.Setup(x => x.Process()).Returns(serviceResult);
            eventServiceMock.Setup(
                x => x.Process(serviceResult.Roots[0].EventKeys.First(), serviceResult.Roots[0].EventKeys.Count)).
                Returns(new ImportSynchronizationMessage { EventStream = new AggregateRootEvent[0] });
            var target = new WirelessSyncProcess(this.Kernel, Guid.NewGuid(), string.Empty);

            target.Import(string.Empty);

            serviceMock.Verify(x => x.Process(), Times.Exactly(1));
            eventServiceMock.Verify(
                x => x.Process(serviceResult.Roots[0].EventKeys.First(), serviceResult.Roots[0].EventKeys.Count), 
                Times.Exactly(1));

            this.SyncProcessRepository.Verify(x => x.GetProcess(It.IsAny<Guid>()), Times.Exactly(1));
        }

        #endregion
    }
}
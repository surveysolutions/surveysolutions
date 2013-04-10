// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WirelessSyncProcessTests.cs" company="">
//   
// </copyright>
// <summary>
//   TODO: Update summary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace DataEntryClientTests.SycProcess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DataEntryClient.SycProcess;

    using DataEntryClientTests.Stubs;

    using Main.Core.Documents;
    using Main.Core.Events;
    using Main.Synchronization.SycProcessRepository;

    using Moq;

    using Ncqrs;
    using Ncqrs.Commanding;
    using Ncqrs.Commanding.ServiceModel;

    using Ninject;

    using NUnit.Framework;

    using SynchronizationMessages.CompleteQuestionnaire;
    using SynchronizationMessages.Synchronization;
    using SynchronizationMessages.WcfInfrastructure;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class WirelessSyncProcessTests
    {
        #region Fields

        /// <summary>
        /// The command service.
        /// </summary>
        private Mock<ICommandService> commandService;

        /// <summary>
        /// The event store.
        /// </summary>
        private Mock<IEventStreamReader> eventStore;

        /// <summary>
        /// The kernel.
        /// </summary>
        private IKernel kernel;

        /// <summary>
        /// Sync repository
        /// </summary>
        private Mock<ISyncProcessRepository> syncProcessRepository;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The create objects.
        /// </summary>
        [SetUp]
        public void CreateObjects()
        {
            this.kernel = new StandardKernel();

            this.eventStore = new Mock<IEventStreamReader>();
            this.kernel.Bind<IEventStreamReader>().ToConstant(this.eventStore.Object);

            this.syncProcessRepository = new Mock<ISyncProcessRepository>();
            this.kernel.Bind<ISyncProcessRepository>().ToConstant(this.syncProcessRepository.Object);

            this.commandService = new Mock<ICommandService>();
            NcqrsEnvironment.SetDefault(this.commandService.Object);
        }

        /// <summary>
        /// The get last event guid_ result not null_ guid is not returned.
        /// </summary>
        [Test]
        public void GetLastEventGuid_ResultNotNull_GuidIsNotReturned()
        {
            var serviceMock = new Mock<IGetLastSyncEvent>();
            IChanelFactoryWrapper chanelFactoryStub = new ChanelFactoryStub(serviceMock);
            this.kernel.Bind<IChanelFactoryWrapper>().ToConstant(chanelFactoryStub);
            Guid clientGuid = Guid.NewGuid();
            Guid? eventGuid = null;

            serviceMock.Setup(x => x.Process(clientGuid)).Returns(eventGuid);
            var target = new WirelessSyncProcess(this.kernel, Guid.NewGuid());
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
            this.kernel.Bind<IChanelFactoryWrapper>().ToConstant(chanelFactoryStub);

            Guid clientGuid = Guid.NewGuid();
            Guid eventGuid = Guid.NewGuid();

            serviceMock.Setup(x => x.Process(clientGuid)).Returns(eventGuid);
            var target = new WirelessSyncProcess(this.kernel, Guid.NewGuid());

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
            this.kernel.Bind<IChanelFactoryWrapper>().ToConstant(chanelFactoryStub);

            Guid clientGuid = Guid.NewGuid();
            Guid? eventGuid = Guid.NewGuid();
            Guid syncProcessGiud = Guid.NewGuid();
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

            this.syncProcessRepository.Setup(x => x.GetProcessor(It.IsAny<Guid>())).Returns(new SyncProcessorStub());
            serviceMock.Setup(x => x.Process()).Returns(serviceResult);
            eventServiceMock.Setup(
                x => x.Process(serviceResult.Roots[0].EventKeys.First(), serviceResult.Roots[0].EventKeys.Count)).
                Returns(new ImportSynchronizationMessage { EventStream = new AggregateRootEvent[0] });
            var target = new WirelessSyncProcess(this.kernel, Guid.NewGuid());

            target.Import(string.Empty, string.Empty);

            serviceMock.Verify(x => x.Process(), Times.Exactly(1));
            eventServiceMock.Verify(
                x => x.Process(serviceResult.Roots[0].EventKeys.First(), serviceResult.Roots[0].EventKeys.Count), 
                Times.Exactly(1));

            this.syncProcessRepository.Verify(x => x.GetProcessor(It.IsAny<Guid>()), Times.Exactly(1));
        }

        /// <summary>
        /// </summary>
        [Test]
        public void WirelessSyncProcess_CheckExportScenario()
        {
            Guid processGuid = Guid.NewGuid();
            var pipeMock = new Mock<IEventPipe>();

            var eventServiceMock = new Mock<IGetEventStream>();
            IChanelFactoryWrapper chanelFactoryStub = new ChanelFactoryStub(new object[] { pipeMock, eventServiceMock });
            this.kernel.Bind<IChanelFactoryWrapper>().ToConstant(chanelFactoryStub);

            var process = new WirelessSyncProcess(this.kernel, processGuid);

            ErrorCodes result = process.Export("Test", "http://192.162.0.0");

            Assert.AreEqual(ErrorCodes.None, result);

            this.commandService.Verify(x => x.Execute(It.IsAny<ICommand>()), Times.AtLeast(2));
        }

        /// <summary>
        /// </summary>
        [Test]
        public void WirelessSyncProcess_CheckImportScenario()
        {
            Guid processGuid = Guid.NewGuid();

            var syncProcessor = new Mock<ISyncProcessor>();

            var serviceMock = new Mock<IGetAggragateRootList>();
            var eventServiceMock = new Mock<IGetEventStream>();
            IChanelFactoryWrapper chanelFactoryStub =
                new ChanelFactoryStub(new object[] { serviceMock, eventServiceMock });
            this.kernel.Bind<IChanelFactoryWrapper>().ToConstant(chanelFactoryStub);

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

            this.syncProcessRepository.Setup(x => x.GetProcessor(It.IsAny<Guid>())).Returns(new SyncProcessorStub());
            serviceMock.Setup(x => x.Process()).Returns(serviceResult);
            eventServiceMock.Setup(
                x => x.Process(serviceResult.Roots[0].EventKeys.First(), serviceResult.Roots[0].EventKeys.Count)).
                Returns(new ImportSynchronizationMessage { EventStream = new AggregateRootEvent[0] });

            this.syncProcessRepository.Setup(r => r.GetProcessor(processGuid)).Returns(syncProcessor.Object);

            var process = new WirelessSyncProcess(this.kernel, processGuid);

            ErrorCodes result = process.Import("Test", "http://192.162.0.0");

            Assert.AreEqual(ErrorCodes.None, result);

            this.commandService.Verify(x => x.Execute(It.IsAny<ICommand>()), Times.AtLeast(3));

            this.syncProcessRepository.Verify(x => x.GetProcessor(processGuid), Times.Once());

            syncProcessor.Verify(x => x.Merge(It.IsAny<IEnumerable<AggregateRootEvent>>()), Times.Exactly(1));

            syncProcessor.Verify(x => x.PostProcess(), Times.Exactly(1));

            syncProcessor.Verify(x => x.Commit(), Times.Exactly(1));
        }

        /// <summary>
        /// </summary>
        [Test]
        public void WirelessSyncProcess_ExceptionWhileEventsLoading_ImportIsFailed()
        {
            Guid processGuid = Guid.NewGuid();

            var syncProcessor = new Mock<ISyncProcessor>();

            var serviceMock = new Mock<IGetAggragateRootList>();
            var eventServiceMock = new Mock<IGetEventStream>();
            IChanelFactoryWrapper chanelFactoryStub =
                new ChanelFactoryStub(new object[] { serviceMock, eventServiceMock });
            this.kernel.Bind<IChanelFactoryWrapper>().ToConstant(chanelFactoryStub);

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

            this.syncProcessRepository.Setup(x => x.GetProcessor(It.IsAny<Guid>())).Returns(new SyncProcessorStub());
            serviceMock.Setup(x => x.Process()).Returns(serviceResult);
            eventServiceMock.Setup(
                x => x.Process(serviceResult.Roots[0].EventKeys.First(), serviceResult.Roots[0].EventKeys.Count)).Throws
                (new Exception("Test exception"));

            this.syncProcessRepository.Setup(r => r.GetProcessor(processGuid)).Returns(syncProcessor.Object);

            var process = new WirelessSyncProcess(this.kernel, processGuid);

            ErrorCodes result = process.Import("Test", "http://192.162.0.0");

            Assert.AreEqual(ErrorCodes.Fail, result);

            this.commandService.Verify(x => x.Execute(It.IsAny<ICommand>()), Times.AtLeast(2));

            this.syncProcessRepository.Verify(x => x.GetProcessor(processGuid), Times.Never());

            syncProcessor.Verify(x => x.Merge(It.IsAny<IEnumerable<AggregateRootEvent>>()), Times.Never());

            syncProcessor.Verify(x => x.PostProcess(), Times.Never());

            syncProcessor.Verify(x => x.Commit(), Times.Never());
        }

        #endregion
    }
}
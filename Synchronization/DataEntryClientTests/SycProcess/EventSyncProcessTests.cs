// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EventSyncProcessTests.cs" company="">
//   
// </copyright>
// <summary>
//   TODO: Update summary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace DataEntryClientTests.SycProcess
{
    using Main.Core.Events;
    using Main.Synchronization.SycProcessRepository;

    using Moq;

    using Ncqrs;
    using Ncqrs.Commanding.ServiceModel;

    using Ninject;

    using NUnit.Framework;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class EventSyncProcessTests
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
        /// The event sync process_ check import scenario.
        /// </summary>
        [Test]
        public void EventSyncProcess_CheckImportScenario()
        {
            /*
            var processGuid = Guid.NewGuid();

            this.eventStore.Setup(x => x.ReadEventsByChunks(100)).Returns(
                new List<IEnumerable<AggregateRootEvent>>
                    { 
                        new List<AggregateRootEvent>
                        {
                            new AggregateRootEvent(new CommittedEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, 1, DateTime.Now, null, new Version(1, 0))), 
                            new AggregateRootEvent(new CommittedEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, 1, DateTime.Now, null, new Version(1, 0))), 
                            new AggregateRootEvent(new CommittedEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, 1, DateTime.Now, null, new Version(1, 0)))
                        }, 
                        new List<AggregateRootEvent>
                        {
                            new AggregateRootEvent(new CommittedEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, 1, DateTime.Now, null, new Version(1, 0))), 
                            new AggregateRootEvent(new CommittedEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, 1, DateTime.Now, null, new Version(1, 0))), 
                            new AggregateRootEvent(new CommittedEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, 1, DateTime.Now, null, new Version(1, 0))), 
                        } 
                    });

            var process = new EventSyncProcess(this.kernel, processGuid);

            var result = process.Export("Test");

            Assert.AreEqual(6, result.Roots.Count);

            this.commandService.Verify(x => x.Execute(It.IsAny<ICommand>()), Times.AtLeast(4));

            this.eventStore.Verify(x => x.ReadEventsByChunks(100), Times.Once());
            */
        }

        #endregion
    }
}
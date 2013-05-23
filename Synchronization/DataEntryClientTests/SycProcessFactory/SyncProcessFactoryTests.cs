// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SyncProcessFactoryTests.cs" company="">
//   
// </copyright>
// <summary>
//   TODO: Update summary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace DataEntryClientTests.SycProcessFactory
{
    using System;

    using DataEntryClient.SycProcess.Interfaces;
    using DataEntryClient.SycProcessFactory;

    using DataEntryClientTests.Stubs;

    using Main.Core.Events;
    using Main.Synchronization.SycProcessRepository;

    using Moq;

    using Ncqrs;
    using Ncqrs.Commanding.ServiceModel;

    using Ninject;

    using NUnit.Framework;

    using SynchronizationMessages.Synchronization;
    using SynchronizationMessages.WcfInfrastructure;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [TestFixture]
    public class SyncProcessFactoryTests
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

            var serviceMock = new Mock<IGetLastSyncEvent>();
            IChanelFactoryWrapper chanelFactoryStub = new ChanelFactoryStub(serviceMock);
            this.kernel.Bind<IChanelFactoryWrapper>().ToConstant(chanelFactoryStub);
        }


        #endregion
    }
}
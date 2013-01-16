// -----------------------------------------------------------------------
// <copyright file="SyncProcessFactoryTests.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace DataEntryClientTests.SycProcessFactory
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using DataEntryClient.SycProcessFactory;
    using DataEntryClient.SycProcessRepository;
    using DataEntryClient.WcfInfrastructure;

    using DataEntryClientTests.Stubs;

    using Main.Core.Events;

    using Moq;

    using NUnit.Framework;

    using Ncqrs;
    using Ncqrs.Commanding.ServiceModel;

    using Ninject;

    using SynchronizationMessages.Synchronization;

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
            NcqrsEnvironment.SetDefault<ICommandService>(this.commandService.Object);

            var serviceMock = new Mock<IGetLastSyncEvent>();
            IChanelFactoryWrapper chanelFactoryStub = new ChanelFactoryStub(serviceMock);
            this.kernel.Bind<IChanelFactoryWrapper>().ToConstant(chanelFactoryStub);
        }

        /// <summary>
        /// Factory should return objects for all SyncProcessTypes
        /// </summary>
        [Test]
        public void Factory_should_return_objects_for_all_SyncProcessTypes()
        {
            var factory = new SyncProcessFactory(this.kernel);
            foreach (SyncProcessType type in Enum.GetValues(typeof(SyncProcessType)))
            {
                try
                {
                    var syncProcess = factory.GetProcess(type, Guid.NewGuid(), null);
                }
                catch (Exception ex)
                {
                    Assert.Fail("Expected no exception, but got: " + ex.Message);
                }
            }
        }
    }
}

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

    using DataEntryClient.SycProcess;
    using DataEntryClient.SycProcessRepository;
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

        #endregion
    }
}
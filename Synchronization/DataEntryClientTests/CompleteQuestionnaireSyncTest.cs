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
    using Main.Core.Events;
    using Main.Synchronization.SycProcessRepository;

    using Moq;

    using Ncqrs;
    using Ncqrs.Commanding.ServiceModel;

    using Ninject;

    using NUnit.Framework;

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
            
            NcqrsEnvironment.SetDefault(this.CommandService.Object);
        }

        #endregion
    }
}
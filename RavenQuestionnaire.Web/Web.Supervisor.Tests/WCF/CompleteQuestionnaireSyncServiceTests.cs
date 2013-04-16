// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteQuestionnaireSyncServiceTests.cs" company="">
//   
// </copyright>
// <summary>
//   The complete questionnaire sync service tests.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Web.Tests.WCF
{
    using System;
    using System.Collections.Generic;

    using DataEntryClient.SycProcess;
    using DataEntryClient.SycProcess.Interfaces;
    using DataEntryClient.SycProcessFactory;

    using Main.Core.Events;

    using Moq;

    using Ninject;

    using NUnit.Framework;

    using SynchronizationMessages.CompleteQuestionnaire;

    using global::Web.Supervisor.WCF;

    /// <summary>
    /// The complete questionnaire sync service tests.
    /// </summary>
    [TestFixture]
    public class CompleteQuestionnaireSyncServiceTests
    {
        #region Public Methods and Operators

        /// <summary>
        /// The process_ avent arrived_ eventprocessed.
        /// </summary>
        [Test]
        public void Process_EventArrived_Eventprocessed()
        {
            IKernel kernel = new StandardKernel();
            var eventSync = new Mock<IEventStreamReader>();
            kernel.Bind<IEventStreamReader>().ToConstant(eventSync.Object);
            var factory = new Mock<ISyncProcessFactory>();
            var syncProcessMock = new Mock<IEventSyncProcess>();

            factory.Setup(f => f.GetProcess(
                It.IsAny<SyncProcessType>(), 
                It.IsAny<Guid>(), 
                It.IsAny<Guid?>()))
                .Returns(syncProcessMock.Object);

            var target = new EventPipeService(kernel, factory.Object);

            // TODO: insert some assertion here
            for (int i = 0; i < 10; i++)
            {
                ErrorCodes result = target.Process(new EventSyncMessage());
                Assert.AreEqual(result, ErrorCodes.None);
            }
            
            //eventSync.Verify(x => x.WriteEvents(It.IsAny<IEnumerable<AggregateRootEvent>>()), Times.Exactly(10));
        }

        #endregion
    }
}
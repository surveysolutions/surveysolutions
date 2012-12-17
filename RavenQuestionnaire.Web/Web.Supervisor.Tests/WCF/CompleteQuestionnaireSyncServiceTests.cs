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
    using System.Collections.Generic;

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
        public void Process_AventArrived_Eventprocessed()
        {
            IKernel kernel = new StandardKernel();
            var eventSync = new Mock<IEventStreamReader>();
            kernel.Bind<IEventStreamReader>().ToConstant(eventSync.Object);
            var target = new EventPipeService(kernel);

            for (int i = 0; i < 10; i++)
            {
                ErrorCodes result = target.Process(new EventSyncMessage());
                Assert.AreEqual(result, ErrorCodes.None);
            }
            // TODO: insert some assertion here
            //eventSync.Verify(x => x.WriteEvents(It.IsAny<IEnumerable<AggregateRootEvent>>()), Times.Exactly(10));
        }

        #endregion
    }
}
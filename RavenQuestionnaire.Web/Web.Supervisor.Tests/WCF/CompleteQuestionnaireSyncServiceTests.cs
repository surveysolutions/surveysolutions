using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Ninject;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Events;
using SynchronizationMessages.CompleteQuestionnaire;
using Web.Supervisor.WCF;

namespace RavenQuestionnaire.Web.Tests.WCF
{
    [TestFixture]
    public class CompleteQuestionnaireSyncServiceTests
    {
        [Test]
        public void Process_AventArrived_Eventprocessed()
        {
            IKernel kernel = new StandardKernel();
            //    Mock<ICommandInvoker> invokerMock = new Mock<ICommandInvoker>();
            Mock<IEventSync> eventSync = new Mock<IEventSync>();
            //    kernel.Bind<ICommandInvoker>().ToConstant(invokerMock.Object);
            kernel.Bind<IEventSync>().ToConstant(eventSync.Object);
            EventPipeService target = new EventPipeService(kernel);

            for (int i = 0; i < 10; i++)
            {


                var result = target.Process(new EventSyncMessage());
                Assert.AreEqual(result, ErrorCodes.None);
            }
            eventSync.Verify(x => x.WriteEvents(It.IsAny<IEnumerable<AggregateRootEvent>>()),
                             Times.Exactly(10));
        }
    }
}

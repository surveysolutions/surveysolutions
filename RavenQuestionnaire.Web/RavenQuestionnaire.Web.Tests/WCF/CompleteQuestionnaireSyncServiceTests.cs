using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataEntryWCFServer;
using Moq;
using NUnit.Framework;
using Ninject;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Views.Event;
using SynchronizationMessages.CompleteQuestionnaire;

namespace DataEntryWCFServerTests
{
    [TestFixture]
    public class CompleteQuestionnaireSyncServiceTests
    {
        [Test]
        public void Process_AventArrived_Eventprocessed()
        {
            IKernel kernel=new StandardKernel();
            Mock<ICommandInvoker> invokerMock = new Mock<ICommandInvoker>();
            kernel.Bind<ICommandInvoker>().ToConstant(invokerMock.Object);
            EventDocumentSyncService target = new EventDocumentSyncService(kernel);

            for (int i = 0; i < 10; i++)
            {


                var result = target.Process(new EventSyncMessage());
                Assert.AreEqual(result, ErrorCodes.None);
            }
            invokerMock.Verify(x => x.Execute(It.IsAny<ICommand>(), It.IsAny<Guid>(), It.IsAny<Guid>()),
                               Times.Exactly(10));
        }
    }
}

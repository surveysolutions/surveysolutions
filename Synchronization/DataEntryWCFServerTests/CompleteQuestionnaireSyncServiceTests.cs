using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataEntryWCFServer;
using Moq;
using NUnit.Framework;
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
         /*   Mock<ICommandInvoker> invokerMock = new Mock<ICommandInvoker>();

            EventDocumentSyncService target = new EventDocumentSyncService(invokerMock.Object);

            for (int i = 0; i < 10; i++)
            {


                var result = target.Process(new EventSyncMessage());
                Assert.AreEqual(result, ErrorCodes.None);
            }
            invokerMock.Verify(x => x.Execute(It.IsAny<ICommand>(), It.IsAny<Guid>(), It.IsAny<Guid>()),
                               Times.Exactly(10));*/
        }
    }
}

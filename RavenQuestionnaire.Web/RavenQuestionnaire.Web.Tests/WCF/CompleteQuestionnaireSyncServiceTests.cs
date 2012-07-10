using System;
using Moq;
using NUnit.Framework;
using Ninject;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Web.WCF;
using SynchronizationMessages.CompleteQuestionnaire;

namespace RavenQuestionnaire.Web.Tests.WCF
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
            EventPipeService target = new EventPipeService(kernel);

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

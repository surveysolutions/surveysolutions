using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Views.Event;
using RavenQuestionnaire.Web.WCF;

namespace DataEntryWCFServerTests
{
    [TestFixture]
    public class GetLastSyncEventServiceTests
    {
        [Test]
        public void Process_FirstSync_NullReturned()
        {
            Mock<ICommandInvoker> invokerMock = new Mock<ICommandInvoker>();
            Mock<IViewRepository> repositoryMock = new Mock<IViewRepository>();
            EventView eventGuid = null;
            repositoryMock.Setup(x => x.Load<EventViewInputModel, EventView>(It.IsAny<EventViewInputModel>())).Returns(
                eventGuid);

            GetLastSyncEventService target = new GetLastSyncEventService();
            target.viewRepository = repositoryMock.Object;
            var result = target.Process(Guid.NewGuid());
            Assert.IsNull(result);
        }

        [Test]
        public void Process_NotFirstSync_GuidReturned()
        {
            Mock<ICommandInvoker> invokerMock = new Mock<ICommandInvoker>();
            Mock<IViewRepository> repositoryMock = new Mock<IViewRepository>();
            EventView eventGuid = new EventView(Guid.NewGuid(), Guid.NewGuid(), DateTime.Now, null);
            repositoryMock.Setup(x => x.Load<EventViewInputModel, EventView>(It.IsAny<EventViewInputModel>())).Returns(
                eventGuid);

            GetLastSyncEventService target = new GetLastSyncEventService();
            target.viewRepository = repositoryMock.Object;
            var result = target.Process(Guid.NewGuid());
            Assert.AreEqual(result, eventGuid.PublicKey);
        }
    }
}

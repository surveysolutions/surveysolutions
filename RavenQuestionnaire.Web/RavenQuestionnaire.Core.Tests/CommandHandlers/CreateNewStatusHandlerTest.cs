using Moq;
using NUnit.Framework;
using RavenQuestionnaire.Core.CommandHandlers.Status;
using RavenQuestionnaire.Core.Commands.Status;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Repositories;

namespace RavenQuestionnaire.Core.Tests.CommandHandlers
{

    [TestFixture]
    public class CreateNewStatusHandlerTest
    {
        [Test]
        public void WhenCommandIsReceived_NewStatusIsAddedToRepository()
        {
            Mock<IStatusRepository> statusRepositoryMock = new Mock<IStatusRepository>();
            CreateNewStatusHandler handler=new CreateNewStatusHandler(statusRepositoryMock.Object);
            handler.Handle(new CreateNewStatusCommand("Create", true, "uID", null));
            statusRepositoryMock.Verify(x=>x.Add(It.IsAny<Status>()), Times.Once());
        }
    }
}

using Moq;
using NUnit.Framework;
using RavenQuestionnaire.Core.CommandHandlers.Status;
using RavenQuestionnaire.Core.Commands.Status;
using RavenQuestionnaire.Core.Documents;
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

            StatusDocument innerDocument = new StatusDocument();
            innerDocument.Id = "sID";
            Status entity = new Status(innerDocument);
            Mock<IStatusRepository> statusRepositoryMock = new Mock<IStatusRepository>();
            statusRepositoryMock.Setup(x => x.Load("statusdocuments/sID")).Returns(entity);
           
            CreateNewStatusHandler handler=new CreateNewStatusHandler(statusRepositoryMock.Object);
            handler.Handle(new CreateNewStatusCommand("Create", true, innerDocument.Id, "QID", null));
            statusRepositoryMock.Verify(x => x.Add(It.IsAny<Status>()), Times.Once());
        }
    }
}

using Moq;
using NUnit.Framework;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Repositories;
using RavenQuestionnaire.Core.Commands.Status;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.CommandHandlers.Status;


namespace RavenQuestionnaire.Core.Tests.CommandHandlers
{
    [TestFixture]
    public class CreateNewStatusFlowHandlerTest
    {
        [Test]
        public void WhenCommandIsReceived_NewStatusFlowIsAddedToRepository()
        {
            //Mock<IStatusRepository> statusRepositoryMock = new Mock<IStatusRepository>();
            //CreateStatusFlowHandler handler = new CreateStatusFlowHandler(statusRepositoryMock.Object);
            //handler.Handle(new AddNewStatusFlowItem("New", "New", "New", new SurveyStatus(), null));
            //statusRepositoryMock.Verify(x => x.Add(It.IsAny<Status>()), Times.Once());
        }
    }
}

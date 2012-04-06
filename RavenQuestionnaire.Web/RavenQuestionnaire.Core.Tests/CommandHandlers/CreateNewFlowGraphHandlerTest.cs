using Moq;
using NUnit.Framework;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Repositories;
using RavenQuestionnaire.Core.Commands.Questionnaire.Flow;
using RavenQuestionnaire.Core.CommandHandlers.Questionnaire.Flow;

namespace RavenQuestionnaire.Core.Tests.CommandHandlers
{
    [TestFixture]
    public class CreateNewFlowGraphHandlerTest
    {
        [Test]
        public void WhenCommandIsReceived_NewFlowGraphIsAddedToRepository()
        {
            Mock<IFlowGraphRepository> flowGraphRepositoryMock = new Mock<IFlowGraphRepository>();
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            innerDocument.Id = "qID";
            Questionnaire entity = new Questionnaire(innerDocument);
            CreateNewFlowGraphHandler handler = new CreateNewFlowGraphHandler(flowGraphRepositoryMock.Object);
            handler.Handle(new CreateNewFlowGraphCommand(entity, null));
            flowGraphRepositoryMock.Verify(x=>x.Add(It.IsAny<FlowGraph>()), Times.Once());
        }
    }
}

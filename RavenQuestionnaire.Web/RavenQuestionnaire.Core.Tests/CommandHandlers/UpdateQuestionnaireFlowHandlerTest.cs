using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Repositories;
using RavenQuestionnaire.Core.CommandHandlers;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Commands.Questionnaire.Flow;

namespace RavenQuestionnaire.Core.Tests.CommandHandlers
{
    [TestFixture]
    public class UpdateQuestionnaireFlowHandlerTest
    {
        [Test]
        public void WhenCommandIsReceived_QuestionIsUpdatedToRepository()
        {
            FlowGraphDocument innerDocument = new FlowGraphDocument();
            innerDocument.Id = "uID";
            FlowGraph entity = new FlowGraph(innerDocument);
            Mock<IFlowGraphRepository> flowgraphRepositoryMock = new Mock<IFlowGraphRepository>();
            flowgraphRepositoryMock.Setup(x => x.Load("flowgraphdocuments/uID")).Returns(entity);
            UpdateQuestionnaireFlowHandler handler=new UpdateQuestionnaireFlowHandler(flowgraphRepositoryMock.Object);
            handler.Handle(new UpdateQuestionnaireFlowCommand("uID", new List<FlowBlock>()
                                                                         {
                                                                             new FlowBlock()
                                                                         }, 
                                                                         new List<FlowConnection>()
                                                                             {
                                                                                 new FlowConnection()
                                                                             }, null));
            Assert.True(innerDocument.Blocks.Count==1 && innerDocument.Connections.Count==1);
        }
    }
}

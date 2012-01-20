using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using RavenQuestionnaire.Core.CommandHandlers;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Repositories;

namespace RavenQuestionnaire.Core.Tests.CommandHandlers
{
    [TestFixture]
    public class DeleteCompleteQuestionnaireComHandlerTest
    {
        public Mock<IIteratorContainer> iteratorContainerMock;
        [SetUp]
        public void CreateObjects()
        {
            iteratorContainerMock = new Mock<IIteratorContainer>();
        }
        [Test]
        public void WhenCommandIsReceived_QuestionnaireIsDeletedFromRepository()
        {
            CompleteQuestionnaireDocument innerDocument = new CompleteQuestionnaireDocument();
            innerDocument.Id = "cqID";

            CompleteQuestionnaire entity = new CompleteQuestionnaire(innerDocument, iteratorContainerMock.Object);

            Mock<ICompleteQuestionnaireRepository> questionnaireRepositoryMock = new Mock<ICompleteQuestionnaireRepository>();
            questionnaireRepositoryMock.Setup(x => x.Load("completequestionnairedocuments/cqID")).Returns(entity);

            DeleteCompleteQuestionnaireHandler handler = new DeleteCompleteQuestionnaireHandler(questionnaireRepositoryMock.Object);
            handler.Handle(new Commands.DeleteCompleteQuestionnaireCommand(entity.CompleteQuestinnaireId, null));
            questionnaireRepositoryMock.Verify(x => x.Remove(entity));
        }
    }
}

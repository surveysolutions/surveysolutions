using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using RavenQuestionnaire.Core.CommandHandlers;
using RavenQuestionnaire.Core.CommandHandlers.Questionnaire.Completed;
using RavenQuestionnaire.Core.Commands.Questionnaire.Completed;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Repositories;
using RavenQuestionnaire.Core.Services;

namespace RavenQuestionnaire.Core.Tests.CommandHandlers
{
    [TestFixture]
    public class DeleteCompleteQuestionnaireComHandlerTest
    {
        [SetUp]
        public void CreateObjects()
        {
        }
        [Test]
        public void WhenCommandIsReceived_QuestionnaireIsDeletedFromRepository()
        {
            CompleteQuestionnaireDocument innerDocument = new CompleteQuestionnaireDocument();
            innerDocument.Id = "cqID";

            CompleteQuestionnaire entity = new CompleteQuestionnaire(innerDocument);

            Mock<ICompleteQuestionnaireUploaderService> questionnaireRepositoryMock = new Mock<ICompleteQuestionnaireUploaderService>();
         //   questionnaireRepositoryMock.Setup(x => x.DeleteCompleteQuestionnaire("completequestionnairedocuments/cqID")).Returns(entity);

            DeleteCompleteQuestionnaireHandler handler = new DeleteCompleteQuestionnaireHandler(questionnaireRepositoryMock.Object);
            handler.Handle(new DeleteCompleteQuestionnaireCommand(entity.CompleteQuestinnaireId, null));
            questionnaireRepositoryMock.Verify(x => x.DeleteCompleteQuestionnaire("cqID"));
        }
    }
}

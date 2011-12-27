using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using RavenQuestionnaire.Core.CommandHandlers;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Repositories;

namespace RavenQuestionnaire.Core.Tests.CommandHandlers
{
    [TestFixture]
    public class DeleteQuestionnaireHandlerTest
    {
        [Test]
        public void WhenCommandIsReceived_QuestionnaireIsDeletedFromRepository()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            innerDocument.Id = "qID";

            Questionnaire entity = new Questionnaire(innerDocument);

            Mock<IQuestionnaireRepository> questionnaireRepositoryMock = new Mock<IQuestionnaireRepository>();
            questionnaireRepositoryMock.Setup(x => x.Load("questionnairedocuments/qID")).Returns(entity);

            DeleteQuestionnaireHandler handler = new DeleteQuestionnaireHandler(questionnaireRepositoryMock.Object);
            handler.Handle(new Commands.DeleteQuestionnaireCommand(entity.QuestionnaireId, null));
            questionnaireRepositoryMock.Verify(x => x.Remove(entity));
        }
    }
}

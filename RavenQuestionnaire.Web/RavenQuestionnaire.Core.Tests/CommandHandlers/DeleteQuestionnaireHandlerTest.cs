using System;
using Moq;
using NUnit.Framework;
using RavenQuestionnaire.Core.CommandHandlers;
using RavenQuestionnaire.Core.Commands.Questionnaire;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;
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
            Guid key = Guid.NewGuid();
            innerDocument.PublicKey = key;

            Questionnaire entity = new Questionnaire(innerDocument);

            Mock<IQuestionnaireRepository> questionnaireRepositoryMock = new Mock<IQuestionnaireRepository>();
            questionnaireRepositoryMock.Setup(x => x.Load(key.ToString())).Returns(entity);

            DeleteQuestionnaireHandler handler = new DeleteQuestionnaireHandler(questionnaireRepositoryMock.Object);
            handler.Handle(new DeleteQuestionnaireCommand(entity.QuestionnaireId, null));
            questionnaireRepositoryMock.Verify(x => x.Remove(entity));
        }
    }
}

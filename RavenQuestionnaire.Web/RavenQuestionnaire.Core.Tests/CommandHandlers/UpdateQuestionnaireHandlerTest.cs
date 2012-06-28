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
    public class UpdateQuestionnaireHandlerTest
    {
        [Test]
        public void WhenCommandIsReceived_QuestionIsUpdatedToRepository()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            Guid key = Guid.NewGuid();
            innerDocument.PublicKey = key;
            Questionnaire entity = new Questionnaire(innerDocument);
            Mock<IQuestionnaireRepository> questionnaireRepositoryMock = new Mock<IQuestionnaireRepository>();
            questionnaireRepositoryMock.Setup(x => x.Load(key.ToString())).Returns(entity);
            UpdateQuestionnaireHandler handler = new UpdateQuestionnaireHandler(questionnaireRepositoryMock.Object);
            handler.Handle(new Commands.UpdateQuestionnaireCommand(entity.QuestionnaireId, "title", null));

            Assert.True(
                innerDocument.Title == "title");

        }
    }
}

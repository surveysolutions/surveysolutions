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
    public class DeleteGroupHandlerTest
    {
        [Test]
        public void WhenCommandIsReceived_GroupIsDeletedFromRepository()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            innerDocument.Id = "qID";

            Questionnaire entity = new Questionnaire(innerDocument);
            var group = new Group("group");
            innerDocument.Groups.Add(group);
            Mock<IQuestionnaireRepository> questionnaireRepositoryMock = new Mock<IQuestionnaireRepository>();
            questionnaireRepositoryMock.Setup(x => x.Load("questionnairedocuments/qID")).Returns(entity);

            DeleteGroupHandler handler = new DeleteGroupHandler(questionnaireRepositoryMock.Object);
            handler.Handle(new Commands.DeleteGroupCommand(group.PublicKey, entity.QuestionnaireId));

            Assert.True(
                innerDocument.Questions.Count == 0);

        }
    }
}

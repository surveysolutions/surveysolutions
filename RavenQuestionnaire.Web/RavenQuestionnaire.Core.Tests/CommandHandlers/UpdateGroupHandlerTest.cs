using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using RavenQuestionnaire.Core.CommandHandlers;
using RavenQuestionnaire.Core.CommandHandlers.Questionnaire.Group;
using RavenQuestionnaire.Core.Commands.Questionnaire.Group;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Repositories;

namespace RavenQuestionnaire.Core.Tests.CommandHandlers
{
    [TestFixture]
    public class UpdateGroupHandlerTest
    {

        [Test]
        public void WhenCommandIsReceived_NewgroupIsAddedToQuestionanire()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            innerDocument.Id = "qID";
            Questionnaire entity = new Questionnaire(innerDocument);
            Group groupForUpdate = new Group();
            innerDocument.Groups.Add(groupForUpdate);
            Mock<IQuestionnaireRepository> questionnaireRepositoryMock = new Mock<IQuestionnaireRepository>();
            questionnaireRepositoryMock.Setup(x => x.Load("questionnairedocuments/qID")).Returns(entity);
            UpdateGroupHandler handler = new UpdateGroupHandler(questionnaireRepositoryMock.Object);
      /*      AnswerView[] answers = new AnswerView[] { new AnswerView() { AnswerText = "answer", AnswerType = AnswerType.Text } };*/
            handler.Handle(new UpdateGroupCommand("test", Propagate.None, entity.QuestionnaireId, groupForUpdate.PublicKey, null));
            Assert.AreEqual(innerDocument.Groups[0].Title, "test");
            questionnaireRepositoryMock.Verify(x => x.Load("questionnairedocuments/qID"), Times.Once());

        }
    }
}

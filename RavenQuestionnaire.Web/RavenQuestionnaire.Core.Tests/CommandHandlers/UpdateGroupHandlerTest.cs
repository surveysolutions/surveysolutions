using System;
using Moq;
using NUnit.Framework;
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
            Guid key = Guid.NewGuid();
            innerDocument.PublicKey = key;
            Questionnaire entity = new Questionnaire(innerDocument);
            Group groupForUpdate = new Group();
            innerDocument.Children.Add(groupForUpdate);
            Mock<IQuestionnaireRepository> questionnaireRepositoryMock = new Mock<IQuestionnaireRepository>();
            questionnaireRepositoryMock.Setup(x => x.Load(key.ToString())).Returns(entity);
            UpdateGroupHandler handler = new UpdateGroupHandler(questionnaireRepositoryMock.Object);
      /*      AnswerView[] answers = new AnswerView[] { new AnswerView() { Title = "answer", AnswerType = AnswerType.Text } };*/
            handler.Handle(new UpdateGroupCommand("test", Propagate.None, entity.QuestionnaireId, groupForUpdate.PublicKey, null));
            Assert.AreEqual(((IGroup)innerDocument.Children[0]).Title, "test");
            questionnaireRepositoryMock.Verify(x => x.Load(key.ToString()), Times.Once());

        }
    }
}

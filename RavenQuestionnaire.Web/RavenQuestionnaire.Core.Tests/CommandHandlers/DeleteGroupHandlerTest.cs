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
    public class DeleteGroupHandlerTest
    {
        [Test]
        public void WhenCommandIsReceived_GroupIsDeletedFromRepository()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();

            Guid key = Guid.NewGuid();
            innerDocument.PublicKey = key;

            Questionnaire entity = new Questionnaire(innerDocument);
            var group = new Group("group");
            innerDocument.Children.Add(group);
            Mock<IQuestionnaireRepository> questionnaireRepositoryMock = new Mock<IQuestionnaireRepository>();
            questionnaireRepositoryMock.Setup(x => x.Load(key.ToString())).Returns(entity);

            DeleteGroupHandler handler = new DeleteGroupHandler(questionnaireRepositoryMock.Object);
            handler.Handle(new DeleteGroupCommand(group.PublicKey, entity.QuestionnaireId, null));

            Assert.True(
                innerDocument.Children.Count == 0);

        }
    }
}
